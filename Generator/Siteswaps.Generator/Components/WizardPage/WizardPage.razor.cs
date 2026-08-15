using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Siteswaps.Generator.Components;
using Siteswaps.Generator.Components.GenerationWorkflow;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;
using Siteswaps.Generator.Components.WizardPage.Filters;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.WizardPage;

/// <summary>
/// Code-behind for the mobile-first, guided siteswap-generator wizard.
/// All page state (current step, inputs, flat filter list, generation results) lives in a
/// single plain <see cref="WizardState"/> instance here - no Fluxor store. Child
/// components communicate with this page through parameters and EventCallbacks only.
/// </summary>
public partial class WizardPage : ComponentBase, IAsyncDisposable
{
    private const int MinSpinnerVisibleMs = 500;

    private readonly WizardState State = new();

    private readonly GenerationWorkflowConfig _wizardHostConfig = new();

    private FilterBottomSheet? _filterSheet;

    private bool _isStepTransitioning;
    private bool _isStartingGeneration;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<WizardPage>? _selfReference;
    private ElementReference _stepsElement;
    private ElementReference _stepHeading0;
    private ElementReference _stepHeading1;
    private ElementReference _stepHeading2;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    [Inject]
    private IStringLocalizer<WizardPage> L { get; set; } = default!;

    private string StepTitle(int step) =>
        step switch
        {
            0 => L["Jugglers & period"],
            1 => L["Clubs & throws"],
            2 => L["Any filters?"],
            _ => string.Empty,
        };

    private bool IsStepActive(int step) => State.CurrentStep == step;

    private bool IsLastStep => State.CurrentStep == WizardState.TotalSteps - 1;

    private int HeaderStep =>
        State.Phase is WizardPhase.Results or WizardPhase.Generating
            ? WizardState.ResultsStepIndex
            : State.CurrentStep;

    private IReadOnlySet<int> HeaderVisitedSteps =>
        State.VisitedSteps.Contains(WizardState.ResultsStepIndex)
        || State.Phase is WizardPhase.Results or WizardPhase.Generating
            ? WithResultsStepVisited(State.VisitedSteps)
            : State.VisitedSteps;

    private static IReadOnlySet<int> WithResultsStepVisited(IReadOnlySet<int> visited)
    {
        if (visited.Contains(WizardState.ResultsStepIndex))
        {
            return visited;
        }

        var copy = new HashSet<int>(visited) { WizardState.ResultsStepIndex };
        return copy;
    }

    private string NextButtonText => IsLastStep ? L["Generate →"] : L["Next"];

    private bool IsNextDisabled =>
        _isStepTransitioning || _isStartingGeneration || State.Phase == WizardPhase.Generating;

    private string? NextPreviewText =>
        IsLastStep ? null : L["Next: {0}", StepTitle(State.CurrentStep + 1)].Value;

    private string StepAnnouncement =>
        L[
            "Step {0} / {1}: {2}",
            State.CurrentStep + 1,
            WizardState.TotalSteps,
            StepTitle(State.CurrentStep)
        ];

    private static string? AriaHidden(bool isActive) => isActive ? null : "true";

    private string? AriaHidden(int step) => AriaHidden(IsStepActive(step));

    private async Task FocusActiveStepHeadingAsync()
    {
        if (State.Phase != WizardPhase.Editing)
        {
            return;
        }

        var heading = State.CurrentStep switch
        {
            0 => _stepHeading0,
            1 => _stepHeading1,
            2 => _stepHeading2,
            _ => default,
        };

        try
        {
            await heading.FocusAsync();
        }
        catch (JSException)
        {
            // Heading may not be attached yet during teardown / phase switches.
        }
    }

    protected override void OnInitialized()
    {
        Navigation.LocationChanged += OnLocationChanged;
    }

    protected override async Task OnInitializedAsync()
    {
        var settings = await LocalStorage.GetItemAsync<SettingsDto>("settings");
        State.ApplyMaxThrowHeight(settings?.MaxHeight ?? new SettingsDto().MaxHeight);
        if (settings is not null)
        {
            State.ShowThrowNames = settings.ShowThrowNames;
        }
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

    private bool IsWizardLocation(string location)
    {
        var relative = Navigation.ToBaseRelativePath(location);
        var pathOnly = relative.Split('?', '#')[0].TrimEnd('/');
        return string.IsNullOrEmpty(pathOnly)
            || pathOnly.Equals("wizard", StringComparison.OrdinalIgnoreCase);
    }

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
            await FocusActiveStepHeadingAsync();
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
            State.CurrentStep--;
            StateHasChanged();
            if (_jsModule is not null)
            {
                await _jsModule.InvokeVoidAsync("replaceEditorState", State.CurrentStep);
            }

            await Task.Delay(150);
            await FocusActiveStepHeadingAsync();
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
            await FocusActiveStepHeadingAsync();
        }
        finally
        {
            _isStepTransitioning = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnProgressStepSelectedAsync(int step)
    {
        if (_isStepTransitioning)
        {
            return;
        }

        if (step == WizardState.ResultsStepIndex)
        {
            if (
                !State.VisitedSteps.Contains(WizardState.ResultsStepIndex)
                && State.Phase is not (WizardPhase.Results or WizardPhase.Generating)
            )
            {
                return;
            }

            if (State.Phase is WizardPhase.Results or WizardPhase.Generating)
            {
                return;
            }

            State.MarkVisited(WizardState.ResultsStepIndex);
            State.Phase = WizardPhase.Results;
            await InvokeAsync(StateHasChanged);
            if (_jsModule is not null)
            {
                await _jsModule.InvokeVoidAsync("pushResultsState", State.CurrentStep);
            }

            return;
        }

        if (State.Phase != WizardPhase.Editing)
        {
            State.GenerationCancellation?.Cancel();
            State.Phase = WizardPhase.Editing;
            State.CurrentStep = step;
            State.MarkVisited(step);
            await PushEditorHistoryStateAsync();
            await InvokeAsync(StateHasChanged);
            await Task.Delay(150);
            await FocusActiveStepHeadingAsync();
            return;
        }

        await JumpToStepAsync(step);
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
            await OnNextClicked();
        }
        else
        {
            await GoBackAsync();
        }
    }

    private FilterNode? _insertParent;

    private void OpenAddFilterSheet()
    {
        _insertParent = null;
        _filterSheet?.OpenForNew();
    }

    private void OpenAddFilterInGroup(FilterNode? parent)
    {
        _insertParent = parent;
        _filterSheet?.OpenForNew();
    }

    private void OpenEditFilterSheet(WizardFilterEntry entry) => _filterSheet?.OpenForEdit(entry);

    private void OnFilterTreeChanged(FilterTree tree)
    {
        State.FilterTree = tree;
    }

    private void OnFilterAdded(IFilterInformation filter)
    {
        var id = State.NextFilterId();
        var leaf = new FilterLeaf(new WizardIdentifiedFilter(id, filter));
        State.FilterTree = WizardFilterTree.AddLeaf(State.FilterTree, _insertParent, leaf);
        _insertParent = null;
    }

    private void OnFilterEdited((int Id, IFilterInformation Filter) args)
    {
        State.FilterTree = WizardFilterTree.ReplaceLeaf(State.FilterTree, args.Id, args.Filter);
    }

    private void RemoveFilter(int id)
    {
        State.FilterTree = WizardFilterTree.RemoveLeaf(State.FilterTree, id);
        _insertParent = null;
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
        State.MarkVisited(WizardState.ResultsStepIndex);
        await InvokeAsync(StateHasChanged);
        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync("pushResultsState", State.CurrentStep);
        }

        var cacheKey = WizardGenerationCacheKey.From(State);
        var startedAt = Environment.TickCount64;
        if (await TryLoadCachedResultsAsync(cacheKey))
        {
            var remaining = MinSpinnerVisibleMs - (int)(Environment.TickCount64 - startedAt);
            if (remaining > 0)
            {
                await Task.Delay(remaining);
            }

            State.Phase = WizardPhase.Results;
            _isStartingGeneration = false;
            await InvokeAsync(StateHasChanged);
            if (_jsModule is not null)
            {
                await _jsModule.InvokeVoidAsync("scrollIntoView", ".wizard-results");
            }

            return;
        }

        await Task.Delay(1);

        var buffer = new List<Siteswap>();

        try
        {
            await foreach (
                var siteswap in SiteswapListGeneration.GenerateStreamAsync(State, cts.Token)
            )
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }

                buffer.Add(siteswap);
                if (buffer.Count < 10 || Environment.TickCount64 - startedAt < MinSpinnerVisibleMs)
                {
                    continue;
                }

                State.Results.AddRange(buffer);
                buffer.Clear();
                await InvokeAsync(StateHasChanged);
                await Task.Delay(1, CancellationToken.None);
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
            await SaveCachedResultsAsync(cacheKey, State.Results);
        }

        State.Phase = WizardPhase.Results;
        _isStartingGeneration = false;
        await InvokeAsync(StateHasChanged);

        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync("scrollIntoView", ".wizard-results");
        }
    }

    private async Task<bool> TryLoadCachedResultsAsync(string cacheKey)
    {
        try
        {
            var cached = await LocalStorage.GetItemAsync<WizardGenerationCacheEntry>(cacheKey);
            if (cached is null)
            {
                return false;
            }

            if (cached.IsExpired())
            {
                await LocalStorage.RemoveItemAsync(cacheKey);
                return false;
            }

            State.Results.Clear();
            foreach (var value in cached.Results)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (Siteswap.TryCreate(value, out var siteswap) && siteswap is not null)
                {
                    State.Results.Add(siteswap);
                }
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task SaveCachedResultsAsync(string cacheKey, IReadOnlyList<Siteswap> results)
    {
        try
        {
            await LocalStorage.SetItemAsync(
                cacheKey,
                WizardGenerationCacheEntry.FromResults(results)
            );
        }
        catch (Exception)
        {
            // Quota / private mode: generation still succeeded; cache is best-effort.
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
