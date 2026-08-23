using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Webassembly.Shared;

public sealed partial class PwaInstallButton : IAsyncDisposable
{
    private DotNetObjectReference<PwaInstallButton>? _self;
    private bool _visible;

    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    [Parameter]
    public EventCallback OnClicked { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _self = DotNetObjectReference.Create(this);
        await Js.InvokeVoidAsync("pwaInstall.subscribe", _self);
    }

    [JSInvokable]
    public Task OnInstallAvailabilityChanged(bool canPrompt)
    {
        if (_visible == canPrompt)
        {
            return Task.CompletedTask;
        }

        _visible = canPrompt;
        return InvokeAsync(StateHasChanged);
    }

    private async Task InstallAsync()
    {
        if (OnClicked.HasDelegate)
        {
            await OnClicked.InvokeAsync();
        }

        var outcome = await Js.InvokeAsync<string>("pwaInstall.prompt");
        _visible =
            !string.Equals(outcome, "accepted", StringComparison.Ordinal)
            && await Js.InvokeAsync<bool>("pwaInstall.canPrompt");
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await Js.InvokeVoidAsync("pwaInstall.unsubscribe");
        }
        catch (JSDisconnectedException) { }

        _self?.Dispose();
    }
}
