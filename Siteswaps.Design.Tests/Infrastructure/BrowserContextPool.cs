using System.Collections.Concurrent;
using Microsoft.Playwright;

namespace Siteswaps.Design.Tests.Infrastructure;

/// <summary>
/// Limits concurrent Playwright browser contexts (pool size 6). Used contexts are disposed and
/// the slot is refilled with a fresh context so tests never share a dirty context.
/// </summary>
public sealed class BrowserContextPool : IAsyncDisposable
{
    public const int PoolSize = 6;

    private readonly IBrowser _browser;
    private readonly SemaphoreSlim _slots = new(PoolSize, PoolSize);
    private readonly ConcurrentBag<(int Width, int Height, IBrowserContext Context)> _warm = new();
    private int _disposed;

    public BrowserContextPool(IBrowser browser)
    {
        _browser = browser;
    }

    public async Task<IBrowserContext> RentAsync(int width, int height)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

        await _slots.WaitAsync().ConfigureAwait(false);
        try
        {
            while (_warm.TryTake(out var item))
            {
                if (item.Width == width && item.Height == height)
                {
                    return item.Context;
                }

                // Wrong viewport: discard so we never exceed the concurrency budget.
                await item.Context.DisposeAsync().ConfigureAwait(false);
            }

            return await CreateAsync(width, height).ConfigureAwait(false);
        }
        catch
        {
            _slots.Release();
            throw;
        }
    }

    public async Task ReturnAsync(IBrowserContext context, int width, int height)
    {
        try
        {
            await context.DisposeAsync().ConfigureAwait(false);

            if (Volatile.Read(ref _disposed) == 0)
            {
                var fresh = await CreateAsync(width, height).ConfigureAwait(false);
                _warm.Add((width, height, fresh));
            }
        }
        finally
        {
            _slots.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        while (_warm.TryTake(out var item))
        {
            await item.Context.DisposeAsync().ConfigureAwait(false);
        }

        _slots.Dispose();
    }

    private async Task<IBrowserContext> CreateAsync(int width, int height)
    {
        var context = await _browser
            .NewContextAsync(DesignCulture.NewContextOptions(width, height))
            .ConfigureAwait(false);
        await DesignCulture.InstallAsync(context).ConfigureAwait(false);
        return context;
    }
}
