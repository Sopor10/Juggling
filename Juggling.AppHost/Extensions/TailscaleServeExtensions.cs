using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Juggling.AppHost.Extensions;

public static class TailscaleServeExtensions
{
    public static IResourceBuilder<TResource> WithTailscaleServe<TResource>(
        this IResourceBuilder<TResource> resource,
        IDistributedApplicationBuilder appBuilder)
        where TResource : IResourceWithEndpoints
    {
        var tailscaleNode = GetTailscaleNode();
        if (tailscaleNode is not { } node)
        {
            return resource;
        }

        var tailscaleServePort = (int?)null;
        appBuilder.Services.AddSingleton<IHostedService>(new TailscaleServeCleanup(
            () => tailscaleServePort,
            RemoveTailscaleServeRoute));

        resource.WithUrls(context =>
        {
            context.Urls.RemoveAll(url =>
                url.Endpoint is not null
                && Uri.TryCreate(url.Url, UriKind.Absolute, out var uri)
                && uri.Scheme == Uri.UriSchemeHttp
                && !uri.IsLoopback);

            var endpoint = context.GetEndpoint("http");
            if (endpoint is null)
            {
                return;
            }

            tailscaleServePort = ConfigureTailscaleServe(node, endpoint.Port);
            if (tailscaleServePort is not { } servePort)
            {
                return;
            }

            context.Urls.Add(new()
            {
                Url = $"https://{node.DnsName}{(servePort == 443 ? "" : $":{servePort}")}",
                DisplayText = "Tailscale HTTPS"
            });
        });

        return resource;
    }

    private static (string DnsName, IPAddress Ipv4Address)? GetTailscaleNode()
    {
        try
        {
            var result = RunProcess("tailscale", ["status", "--json"]);
            if (result.ExitCode != 0)
            {
                return null;
            }

            using var document = JsonDocument.Parse(result.StandardOutput);
            var self = document.RootElement.GetProperty("Self");
            var dnsName = self.GetProperty("DNSName").GetString()?.TrimEnd('.');
            var ipv4 = self.GetProperty("TailscaleIPs").EnumerateArray()
                .Select(ip => ip.GetString())
                .FirstOrDefault(ip => IPAddress.TryParse(ip, out var address)
                                      && address.AddressFamily == AddressFamily.InterNetwork);

            return string.IsNullOrWhiteSpace(dnsName) || !IPAddress.TryParse(ipv4, out var ipv4Address)
                ? null
                : (dnsName, ipv4Address);
        }
        catch
        {
            return null;
        }
    }

    private static int? ConfigureTailscaleServe((string DnsName, IPAddress Ipv4Address) node, int targetPort)
    {
        try
        {
            using var allocationLock = AcquireServeAllocationLock();
            if (allocationLock is null)
            {
                return null;
            }

            using var status = GetTailscaleServeStatus();
            if (status is null)
            {
                return null;
            }

            var existingPort = FindServePort(status.RootElement, node.DnsName, targetPort);
            if (existingPort is { } configuredPort)
            {
                return configuredPort;
            }

            var port = FindAvailableServePort(status.RootElement, node.Ipv4Address);
            if (port is null)
            {
                return null;
            }

            var result = RunProcess("sudo", [
                "-n", "tailscale", "serve", "--bg", $"--https={port.Value}",
                $"http://127.0.0.1:{targetPort.ToString(CultureInfo.InvariantCulture)}"
            ]);
            if (result.ExitCode != 0)
            {
                return null;
            }

            using var updatedStatus = GetTailscaleServeStatus();
            return updatedStatus is not null
                   && FindServePort(updatedStatus.RootElement, node.DnsName, targetPort) == port
                ? port
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static int? FindAvailableServePort(JsonElement status, IPAddress tailscaleAddress)
    {
        var configuredPorts = new HashSet<int>();
        if (status.TryGetProperty("TCP", out var tcp))
        {
            foreach (var property in tcp.EnumerateObject())
            {
                if (int.TryParse(property.Name, out var configuredPort))
                {
                    configuredPorts.Add(configuredPort);
                }
            }
        }

        for (var port = 10000; port < 20000; port++)
        {
            if (configuredPorts.Contains(port) || !CanBind(tailscaleAddress, port))
            {
                continue;
            }

            return port;
        }

        return null;
    }

    private static bool CanBind(IPAddress address, int port)
    {
        try
        {
            using var listener = new TcpListener(address, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    private static int? FindServePort(JsonElement status, string dnsName, int targetPort)
    {
        if (!status.TryGetProperty("Web", out var web)
            || !status.TryGetProperty("TCP", out var tcp))
        {
            return null;
        }

        foreach (var host in web.EnumerateObject())
        {
            var separator = host.Name.LastIndexOf(':');
            if (separator < 0
                || !string.Equals(host.Name[..separator], dnsName, StringComparison.OrdinalIgnoreCase)
                || !int.TryParse(host.Name[(separator + 1)..], out var port)
                || !tcp.TryGetProperty(port.ToString(CultureInfo.InvariantCulture), out var tcpPort)
                || !tcpPort.TryGetProperty("HTTPS", out var https)
                || !https.GetBoolean())
            {
                continue;
            }

            foreach (var handler in host.Value.GetProperty("Handlers").EnumerateObject())
            {
                if (handler.Value.TryGetProperty("Proxy", out var proxy)
                    && Uri.TryCreate(proxy.GetString(), UriKind.Absolute, out var proxyUri)
                    && proxyUri.Port == targetPort)
                {
                    return port;
                }
            }
        }

        return null;
    }

    private static JsonDocument? GetTailscaleServeStatus()
    {
        try
        {
            var result = RunProcess("tailscale", ["serve", "status", "--json"]);
            return result.ExitCode == 0 ? JsonDocument.Parse(result.StandardOutput) : null;
        }
        catch
        {
            return null;
        }
    }

    private static void RemoveTailscaleServeRoute(int port)
    {
        try
        {
            RunProcess("sudo", ["-n", "tailscale", "serve", $"--https={port}", "off"]);
        }
        catch
        {
        }
    }

    private static FileStream? AcquireServeAllocationLock()
    {
        var lockPath = Path.Combine(Path.GetTempPath(), "aspire-tailscale-serve-allocation.lock");
        for (var attempt = 0; attempt < 40; attempt++)
        {
            try
            {
                return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                Thread.Sleep(100);
            }
        }

        return null;
    }

    private static (int ExitCode, string StandardOutput, string StandardError) RunProcess(
        string fileName,
        string[] arguments)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start {fileName}.");
        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, standardOutput, standardError);
    }

    private sealed class TailscaleServeCleanup(Func<int?> getPort, Action<int> removeRoute) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (getPort() is { } port)
            {
                removeRoute(port);
            }

            return Task.CompletedTask;
        }
    }
}
