using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.WizardPage.Filters;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.WizardPage;

/// <summary>
/// Code-behind for the mobile-first, guided siteswap-generator wizard.
/// All page state (current step, inputs, flat filter list, generation results) lives in a
/// single plain <see cref="WizardState"/> instance here - no Fluxor store, no Radzen. Child
/// components communicate with this page through parameters and EventCallbacks only.
/// </summary>
public partial class WizardPage : ComponentBase, IAsyncDisposable
{
    private const int MinSpinnerVisibleMs = 500;

    private static readonly string[] StepTitles =
    {
        "Jongleure & Periode",
        "Keulen & Würfe",
        "Noch Filter?",
    };

    private readonly WizardState State = new();

    private FilterBottomSheet? _filterSheet;

    private bool _isStepTransitioning;
    private bool _isStartingGeneration;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<WizardPage>? _selfReference;
    private ElementReference _stepsElement;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    private bool IsStepActive(int step) => State.CurrentStep == step;

    private bool IsLastStep => State.CurrentStep == WizardState.TotalSteps - 1;

    private string NextButtonText => IsLastStep ? "Generieren →" : "Weiter";

    private bool IsNextDisabled =>
        _isStepTransitioning || _isStartingGeneration || State.Phase == WizardPhase.Generating;

    private string? NextPreviewText =>
        IsLastStep ? null : $"Weiter: {StepTitles[State.CurrentStep + 1]}";

    private string StepAnnouncement =>
        $"Schritt {State.CurrentStep + 1} / {WizardState.TotalSteps}: {StepTitles[State.CurrentStep]}";

    private static string? AriaHidden(bool isActive) => isActive ? null : "true";

    private string? AriaHidden(int step) => AriaHidden(IsStepActive(step));

    protected override void OnInitialized()
    {
        Navigation.LocationChanged += OnLocationChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./_content/Siteswaps.Generator/Components/WizardPage/WizardPage.razor.js"
            );
            _selfReference = DotNetObjectReference.Create(this);
            await _jsModule.InvokeVoidAsync("initHistory", _selfReference, State.CurrentStep);
            await _jsModule.InvokeVoidAsync("initTouchSwipe", _stepsElement, _selfReference);
        }
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (!IsWizardLocation(e.Location))
        {
            State.GenerationCancellation?.Cancel();
            return;
        }

        if (State.Phase is WizardPhase.Results or WizardPhase.Generating)
        {
            State.GenerationCancellation?.Cancel();
            State.Phase = WizardPhase.Editing;
            State.CurrentStep = Math.Min(State.CurrentStep, WizardState.TotalSteps - 1);
            _ = InvokeAsync(StateHasChanged);
        }
    }

    private static bool IsWizardLocation(string location) =>
        location.Contains("/wizard", StringComparison.OrdinalIgnoreCase);

    [JSInvokable]
    public async Task OnBrowserPopState(int step, bool isResults)
    {
        State.GenerationCancellation?.Cancel();
        _isStartingGeneration = false;
        State.CurrentStep = Math.Clamp(step, 0, WizardState.TotalSteps - 1);
        State.MarkVisited(State.CurrentStep);
        State.Phase =
            isResults && State.Results.Count > 0 ? WizardPhase.Results : WizardPhase.Editing;
        await InvokeAsync(StateHasChanged);
    }

    private void OnPeriodChanged(int value) => State.Period = new Period(value);

    private void OnClubsMinChanged(int value) =>
        State.Clubs = State.Clubs with { MinNumber = value };

    private void OnClubsMaxChanged(int value) =>
        State.Clubs = State.Clubs with { MaxNumber = value };

    private async Task OnNextClicked()
    {
        if (IsNextDisabled)
        {
            return;
        }

        if (IsLastStep)
        {
            await StartGenerationAsync();
        }
        else
        {
            await AdvanceStepAsync();
        }
    }

    private async Task AdvanceStepAsync()
    {
        if (IsLastStep || _isStepTransitioning)
        {
            return;
        }

        _isStepTransitioning = true;
        try
        {
            State.CurrentStep++;
            State.MarkVisited(State.CurrentStep);
            StateHasChanged();
            await PushEditorHistoryStateAsync();
            await Task.Delay(150);
        }
        finally
        {
            _isStepTransitioning = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task GoBackAsync()
    {
        if (State.CurrentStep <= 0 || _isStepTransitioning)
        {
            return;
        }

        _isStepTransitioning = true;
        try
        {
            if (_jsModule is not null)
            {
                await _jsModule.InvokeVoidAsync("back");
            }
            else
            {
                State.CurrentStep--;
                StateHasChanged();
            }

            await Task.Delay(150);
        }
        finally
        {
            _isStepTransitioning = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task SkipToLastStepAsync()
    {
        if (_isStepTransitioning)
        {
            return;
        }

        _isStepTransitioning = true;
        try
        {
            State.CurrentStep = WizardState.TotalSteps - 1;
            State.MarkVisited(State.CurrentStep);
            StateHasChanged();
            await PushEditorHistoryStateAsync();
            await Task.Delay(150);
        }
        finally
        {
            _isStepTransitioning = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task JumpToStepAsync(int step)
    {
        if (
            _isStepTransitioning
            || step < 0
            || step >= WizardState.TotalSteps
            || !State.VisitedSteps.Contains(step)
        )
        {
            return;
        }

        _isStepTransitioning = true;
        try
        {
            State.CurrentStep = step;
            StateHasChanged();
            await PushEditorHistoryStateAsync();
            await Task.Delay(150);
        }
        finally
        {
            _isStepTransitioning = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    [JSInvokable]
    public async Task OnTouchSwipe(bool next)
    {
        if (_isStepTransitioning)
        {
            return;
        }

        if (next)
        {
            await AdvanceStepAsync();
        }
        else
        {
            await GoBackAsync();
        }
    }

    private void OpenAddFilterSheet() => _filterSheet?.OpenForNew();

    private void OpenEditFilterSheet(WizardFilterEntry entry) => _filterSheet?.OpenForEdit(entry);

    private void OnFilterAdded(IFilterInformation filter)
    {
        var id = State.NextFilterId();
        if (State.Filters.Count > 0)
        {
            State.Connectors.Add(WizardFilterConnector.And);
        }

        State.Filters.Add(new WizardFilterEntry(id, filter));
    }

    private void OnFilterEdited((int Id, IFilterInformation Filter) args)
    {
        var index = State.Filters.FindIndex(f => f.Id == args.Id);
        if (index >= 0)
        {
            State.Filters[index] = new WizardFilterEntry(args.Id, args.Filter);
        }
    }

    private void RemoveFilter(int id)
    {
        var index = State.Filters.FindIndex(f => f.Id == id);
        if (index < 0)
        {
            return;
        }

        State.Filters.RemoveAt(index);
        if (State.Connectors.Count > 0)
        {
            var connectorIndex = Math.Min(index, State.Connectors.Count - 1);
            State.Connectors.RemoveAt(connectorIndex);
        }
    }

    private async Task StartGenerationAsync()
    {
        if (_isStartingGeneration || State.Phase == WizardPhase.Generating)
        {
            return;
        }

        _isStartingGeneration = true;
        State.GenerationCancellation?.Cancel();
        var cts = new CancellationTokenSource();
        State.GenerationCancellation = cts;

        State.Results.Clear();
        State.WasCancelled = false;
        State.Phase = WizardPhase.Generating;
        await InvokeAsync(StateHasChanged);
        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync("pushResultsState", State.CurrentStep);
        }

        await Task.Delay(1);

        var startedAt = Environment.TickCount64;
        var generators = FilterTranslation.CreateGenerators(State);
        var buffer = new List<Siteswap>();

        try
        {
            foreach (var generator in generators)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }

                await foreach (var siteswap in generator.GenerateAsync(cts.Token))
                {
                    if (cts.IsCancellationRequested)
                    {
                        break;
                    }

                    buffer.Add(siteswap);
                    if (
                        buffer.Count < 10
                        || Environment.TickCount64 - startedAt < MinSpinnerVisibleMs
                    )
                    {
                        continue;
                    }

                    State.Results.AddRange(buffer);
                    buffer.Clear();
                    await InvokeAsync(StateHasChanged);
                    await Task.Delay(1, CancellationToken.None);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation is surfaced below via cts.IsCancellationRequested.
        }

        var elapsed = Environment.TickCount64 - startedAt;
        if (elapsed < MinSpinnerVisibleMs)
        {
            await Task.Delay((int)(MinSpinnerVisibleMs - elapsed));
        }

        if (cts.IsCancellationRequested)
        {
            State.WasCancelled = true;
        }
        else
        {
            State.Results.AddRange(buffer);
        }

        State.Phase = WizardPhase.Results;
        _isStartingGeneration = false;
        await InvokeAsync(StateHasChanged);

        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync("scrollIntoView", ".wizard-results");
        }
    }

    private void CancelGeneration() => State.GenerationCancellation?.Cancel();

    private async Task BackToEditing()
    {
        State.GenerationCancellation?.Cancel();
        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync("back");
        }
        else
        {
            State.Phase = WizardPhase.Editing;
            State.CurrentStep = WizardState.TotalSteps - 1;
        }
    }

    private async Task StartOver()
    {
        State.ResetToDefaults();
        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync("replaceEditorState", State.CurrentStep);
        }
    }

    private async Task PushEditorHistoryStateAsync()
    {
        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync("pushEditorState", State.CurrentStep);
        }
    }

    public async ValueTask DisposeAsync()
    {
        Navigation.LocationChanged -= OnLocationChanged;
        State.GenerationCancellation?.Cancel();

        try
        {
            if (_jsModule is not null)
            {
                if (_selfReference is not null)
                {
                    await _jsModule.InvokeVoidAsync("disposeHistory", _selfReference);
                }

                await _jsModule.InvokeVoidAsync("disposeTouchSwipe", _stepsElement);
                await _jsModule.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Circuit already gone.
        }

        _selfReference?.Dispose();
    }
}
