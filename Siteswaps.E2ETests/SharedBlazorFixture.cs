using Microsoft.Playwright;
using PlaywrightTesting.Infrastructure;
using Xunit;
using Program = Siteswaps.E2ETests.Server.Program;

namespace Siteswaps.E2ETests;

/// <summary>
/// Class fixture that shares one <see cref="BlazorWebassemblyFixture{TEntryPoint}"/> across
/// parallel test classes. Startup uses <c>Lazy&lt;Task&lt;T&gt;&gt;</c>; teardown is ref-counted.
/// </summary>
public sealed class SharedBlazorFixture : IAsyncLifetime
{
    private static Lazy<Task<BlazorWebassemblyFixture<Program>>> Host = CreateHost();
    private static int Users;
    private static readonly object ResetGate = new();

    public BlazorWebassemblyFixture<Program> Fixture { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Fixture = await Host.Value;
        Interlocked.Increment(ref Users);
    }

    public async Task DisposeAsync()
    {
        if (Interlocked.Decrement(ref Users) != 0)
        {
            return;
        }

        Task<BlazorWebassemblyFixture<Program>>? hostTask;
        lock (ResetGate)
        {
            if (Users != 0)
            {
                return;
            }

            hostTask = Host.IsValueCreated ? Host.Value : null;
            Host = CreateHost();
        }

        if (hostTask is null)
        {
            return;
        }

        var fixture = await hostTask.ConfigureAwait(false);
        await fixture.DisposeAsync().ConfigureAwait(false);
    }

    private static Lazy<Task<BlazorWebassemblyFixture<Program>>> CreateHost() =>
        new(
            async () =>
            {
                var fixture = new BlazorWebassemblyFixture<Program>();
                await fixture.InitializeAsync();
                await E2ECulture.InstallAsync(fixture.Context);
                return fixture;
            },
            LazyThreadSafetyMode.ExecutionAndPublication
        );
}
