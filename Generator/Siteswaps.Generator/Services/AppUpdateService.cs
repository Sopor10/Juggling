using Microsoft.JSInterop;

namespace Siteswaps.Generator.Services;

public sealed class AppUpdateService(IJSRuntime jsRuntime)
{
    public async Task<AppUpdateCheckResult> CheckForUpdatesAsync()
    {
        try
        {
            return await jsRuntime.InvokeAsync<AppUpdateCheckResult>("appUpdate.checkForUpdates");
        }
        catch (JSException exception)
        {
            return new AppUpdateCheckResult(AppUpdateStatus.CheckFailed, exception.Message);
        }
    }

    public Task ApplyUpdateAsync() => jsRuntime.InvokeVoidAsync("appUpdate.applyUpdate").AsTask();

    public Task SubscribeAsync<T>(DotNetObjectReference<T> reference)
        where T : class => jsRuntime.InvokeVoidAsync("appUpdate.subscribe", reference).AsTask();

    public Task UnsubscribeAsync<T>(DotNetObjectReference<T> reference)
        where T : class => jsRuntime.InvokeVoidAsync("appUpdate.unsubscribe", reference).AsTask();
}
