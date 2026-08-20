using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Siteswaps.Mcp.Server.Tools;

[McpServerToolType]
public class McpResourceAccessTools
{
    private readonly IEnumerable<McpServerResource> _resources;
    private readonly IServiceProvider _serviceProvider;
    private readonly Lazy<
        Dictionary<string, (MethodInfo Method, object? Instance)>
    > _resourceMethodCache;

    public McpResourceAccessTools(
        IEnumerable<McpServerResource> resources,
        IServiceProvider serviceProvider
    )
    {
        _resources = resources;
        _serviceProvider = serviceProvider;
        _resourceMethodCache = new Lazy<Dictionary<string, (MethodInfo Method, object? Instance)>>(
            BuildResourceMethodCache
        );
    }

    private Dictionary<string, (MethodInfo Method, object? Instance)> BuildResourceMethodCache()
    {
        var cache = new Dictionary<string, (MethodInfo Method, object? Instance)>(
            StringComparer.OrdinalIgnoreCase
        );
        var resourceTypes = AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttribute<McpServerResourceTypeAttribute>() != null);
        foreach (var type in resourceTypes)
        {
            object? instance = null;
            var methods = type.GetMethods(
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance
                )
                .Where(m => m.GetCustomAttribute<McpServerResourceAttribute>() != null);
            foreach (var method in methods)
            {
                if (!method.IsStatic && instance == null)
                    instance = ActivatorUtilities.GetServiceOrCreateInstance(
                        _serviceProvider,
                        type
                    );
                var attr = method.GetCustomAttribute<McpServerResourceAttribute>();
                var uri = attr?.UriTemplate ?? $"resource://mcp/{ConvertToSnakeCase(method.Name)}";
                cache[uri] = (method, method.IsStatic ? null : instance);
            }
        }
        return cache;
    }

    private static string ConvertToSnakeCase(string text)
    {
        text = System.Text.RegularExpressions.Regex.Replace(text, "([A-Z]+)([A-Z][a-z])", "$1_$2");
        text = System.Text.RegularExpressions.Regex.Replace(text, "([a-z\\d])([A-Z])", "$1_$2");
        return text.ToLowerInvariant();
    }

    [McpServerTool]
    [Description("Lists all available MCP resources.")]
    public ToolResult<List<ResourceListItem>> ListAllResources(string? contains = null)
    {
        return ToolResult.From(() =>
        {
            var query = _resources.Select(x => x.ProtocolResource).OfType<Resource>();
            if (!string.IsNullOrWhiteSpace(contains))
                query = query.Where(r =>
                    r.Uri.Contains(contains, StringComparison.OrdinalIgnoreCase)
                );
            return query
                .Select(r => new ResourceListItem(r.Uri, r.Name, r.Description ?? ""))
                .OrderBy(r => r.Uri)
                .ToList();
        });
    }

    [McpServerTool]
    [Description("Reads the full content of a single MCP resource by its URI.")]
    public async Task<ToolResult<string>> GetResource(string uri)
    {
        return await ToolResult.FromAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentException("URI cannot be empty", nameof(uri));
            var resource = _resources.FirstOrDefault(r =>
                r.ProtocolResource?.Uri.Equals(uri, StringComparison.OrdinalIgnoreCase) ?? false
            );
            if (resource == null)
                throw new ArgumentException($"Resource with URI '{uri}' not found.", nameof(uri));
            if (!_resourceMethodCache.Value.TryGetValue(uri, out var methodInfo))
                throw new InvalidOperationException($"Could not find method for resource: {uri}");
            var (method, instance) = methodInfo;
            var result = method.Invoke(instance, null);
            return result switch
            {
                string content => content,
                Task<string> taskString => await taskString,
                _ => throw new InvalidOperationException(
                    $"Resource method returned unexpected type: {result?.GetType().Name ?? "null"}"
                ),
            };
        });
    }

    public record ResourceListItem(string Uri, string Name, string Description);
}
