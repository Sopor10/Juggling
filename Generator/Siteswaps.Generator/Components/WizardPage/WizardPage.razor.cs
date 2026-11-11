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
using Siteswaps.Generator.Components.WizardPage.Results;
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
    private ResultsView? _resultsView;

    private bool _isStepTransitioning;
    private bool _isStartingGeneration;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<WizardPage>? _selfReference;
    private ElementReference _stepHeading0;
    private ElementReference _stepHeading1;
    private ElementReference _stepHeading2;
    private ElementReference _swipeHost;
    private bool? _swipeBoundToResults;
    private bool _loadedResultsDeepLink;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    [Inject]
    private IStringLocalizer<WizardPage> L { get; set; } = default!;

    [Parameter, SupplyParameterFromQuery(Name = "ws")]
    public string? UrlState { get; set; }

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
            WizardState.ProgressStepCount,
            StepTitle(State.CurrentStep)
        ];

    private static string? AriaHidden(bool isActive) => isActive ? null : "true";

    private string? AriaHidden(int step) => AriaHidden(IsStepActive(step));

    private async Task FocusActiveStepHeadingAsync()
    {
        if (State.Phase != WizardPhase.Editing || _filterSheet?.IsOpen == true)
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

    private async Task FocusResultsHeadingAsync()
    {
        if (_resultsView is null)
        {
            return;
        }

        try
        {
            await _resultsView.FocusTitleAsync();
        }
        catch (JSException)
        {
            // Results heading may not be attached yet during teardown / phase switches.
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

        RestoreUrlState(UrlState);
        _loadedResultsDeepLink =
            State.Phase == WizardPhase.Results && !string.IsNullOrWhiteSpace(UrlState);
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
            await _jsModule.InvokeVoidAsync(
                "initHistory",
                _selfReference,
                State.CurrentStep,
                State.Phase == WizardPhase.Results,
                CurrentWizardUrl()
            );
            if (State.Phase == WizardPhase.Results && State.Results.Count == 0)
            {
                await StartGenerationAsync(pushHistory: false);
            }
        }

        if (_swipeHost.Context is not null)
        {
            await EnsureTouchSwipeAsync();
        }
    }

    private async Task EnsureTouchSwipeAsync()
    {
        if (_jsModule is null || _selfReference is null || _swipeHost.Context is null)
        {
            return;
        }

        var swipeOnResults = State.Phase is WizardPhase.Results or WizardPhase.Generating;
        if (_swipeBoundToResults == swipeOnResults)
        {
            return;
        }

        await _jsModule.InvokeVoidAsync("initTouchSwipe", _swipeHost, _selfReference);
        _swipeBoundToResults = swipeOnResults;
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
    public async Task OnBrowserPopState(int step, bool isResults, string url)
    {
        State.GenerationCancellation?.Cancel();
        _isStartingGeneration = false;
        var restored = RestoreUrlState(url);
        if (!restored)
        {
            State.CurrentStep = Math.Clamp(step, 0, WizardState.TotalSteps - 1);
            State.MarkVisited(State.CurrentStep);
            State.Phase =
                isResults && State.Results.Count > 0 ? WizardPhase.Results : WizardPhase.Editing;
        }
        if (State.Phase == WizardPhase.Results)
        {
            State.Results.Clear();
            _ = StartGenerationAsync(pushHistory: false);
        }
        await InvokeAsync(StateHasChanged);
        await Task.Delay(150);
        if (State.Phase == WizardPhase.Editing)
        {
            await FocusActiveStepHeadingAsync();
        }
        else if (State.Phase == WizardPhase.Results)
        {
            await FocusResultsHeadingAsync();
        }
    }

    private void OnPeriodChanged(int value)
    {
        State.Period = new Period(value);
        _ = ReplaceUrlStateAsync();
    }

    private void OnClubsMinChanged(int value)
    {
        State.Clubs = State.Clubs with { MinNumber = value };
        _ = ReplaceUrlStateAsync();
    }

    private void OnClubsMaxChanged(int value)
    {
        State.Clubs = State.Clubs with { MaxNumber = value };
        _ = ReplaceUrlStateAsync();
    }

    private void OnJugglersChanged(int value)
    {
        State.NumberOfJugglers = value;
        _ = ReplaceUrlStateAsync();
    }

    private void OnThrowsChanged() => _ = ReplaceUrlStateAsync();

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
            _isStepTransitioning = false;
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
                await _jsModule.InvokeVoidAsync(
                    "replaceEditorState",
                    State.CurrentStep,
                    CurrentWizardUrl()
                );
            }

            _isStepTransitioning = false;
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
            _loadedResultsDeepLink = false;
            await InvokeAsync(StateHasChanged);
            if (_jsModule is not null)
            {
                await _jsModule.InvokeVoidAsync(
                    "pushResultsState",
                    State.CurrentStep,
                    CurrentWizardUrl()
                );
            }

            await FocusResultsHeadingAsync();
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

        if (State.Phase is WizardPhase.Results or WizardPhase.Generating)
        {
            if (!next)
            {
                await BackToEditing();
            }

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
        _ = ReplaceUrlStateAsync();
    }

    private void OnFilterAdded(IFilterInformation filter)
    {
        var id = State.NextFilterId();
        var leaf = new FilterLeaf(new WizardIdentifiedFilter(id, filter));
        State.FilterTree = WizardFilterTree.AddLeaf(State.FilterTree, _insertParent, leaf);
        _insertParent = null;
        _ = ReplaceUrlStateAsync();
    }

    private void OnFilterEdited((int Id, IFilterInformation Filter) args)
    {
        State.FilterTree = WizardFilterTree.ReplaceLeaf(State.FilterTree, args.Id, args.Filter);
        _ = ReplaceUrlStateAsync();
    }

    private void RemoveFilter(int id)
    {
        State.FilterTree = WizardFilterTree.RemoveLeaf(State.FilterTree, id);
        _insertParent = null;
        _ = ReplaceUrlStateAsync();
    }

    private async Task StartGenerationAsync(bool pushHistory = true)
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
        if (_jsModule is not null && pushHistory)
        {
            await _jsModule.InvokeVoidAsync(
                "pushResultsState",
                State.CurrentStep,
                CurrentWizardUrl()
            );
        }
        else if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync(
                "replaceResultsState",
                State.CurrentStep,
                CurrentWizardUrl()
            );
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

            if (cts.IsCancellationRequested || State.Phase != WizardPhase.Generating)
            {
                _isStartingGeneration = false;
                if (State.Phase == WizardPhase.Generating)
                {
                    State.Phase = WizardPhase.Results;
                    await InvokeAsync(StateHasChanged);
                }

                return;
            }

            State.Phase = WizardPhase.Results;
            _isStartingGeneration = false;
            await ReplaceResultsUrlStateAsync();
            await InvokeAsync(StateHasChanged);
            if (_jsModule is not null)
            {
                await _jsModule.InvokeVoidAsync("scrollIntoView", ".wizard-results");
            }

            await FocusResultsHeadingAsync();
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

        if (!cts.IsCancellationRequested && State.Phase == WizardPhase.Generating)
        {
            State.Results.AddRange(buffer);
            await SaveCachedResultsAsync(cacheKey, State.Results);

            State.Phase = WizardPhase.Results;
            _isStartingGeneration = false;
            await ReplaceResultsUrlStateAsync();
            await InvokeAsync(StateHasChanged);

            if (_jsModule is not null)
            {
                await _jsModule.InvokeVoidAsync("scrollIntoView", ".wizard-results");
            }

            await FocusResultsHeadingAsync();
            return;
        }

        _isStartingGeneration = false;
        if (State.Phase == WizardPhase.Generating)
        {
            State.Results.AddRange(buffer);
            State.Phase = WizardPhase.Results;
            await ReplaceResultsUrlStateAsync();
            await InvokeAsync(StateHasChanged);
            return;
        }

        State.WasCancelled = true;
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
        if (_loadedResultsDeepLink)
        {
            State.Phase = WizardPhase.Editing;
            State.CurrentStep = WizardState.TotalSteps - 1;
            _loadedResultsDeepLink = false;
            await ReplaceEditorUrlStateAsync();
            await InvokeAsync(StateHasChanged);
            await Task.Delay(150);
            await FocusActiveStepHeadingAsync();
        }
        else if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync("back");
        }
        else
        {
            State.Phase = WizardPhase.Editing;
            State.CurrentStep = WizardState.TotalSteps - 1;
            await InvokeAsync(StateHasChanged);
            await Task.Delay(150);
            await FocusActiveStepHeadingAsync();
        }
    }

    private async Task StartOver()
    {
        State.ResetToDefaults();
        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync(
                "replaceEditorState",
                State.CurrentStep,
                CurrentWizardUrl()
            );
        }
    }

    private async Task PushEditorHistoryStateAsync()
    {
        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync(
                "pushEditorState",
                State.CurrentStep,
                CurrentWizardUrl()
            );
        }
    }

    private bool RestoreUrlState(string? urlOrState)
    {
        if (string.IsNullOrWhiteSpace(urlOrState))
        {
            return false;
        }
        string? encoded = urlOrState;
        if (Uri.TryCreate(urlOrState, UriKind.Absolute, out var uri))
        {
            encoded = GetQueryValue(uri.Query, "ws");
        }
        else if (urlOrState.Contains('?'))
        {
            encoded = GetQueryValue(urlOrState, "ws");
        }

        var snapshot = WizardUrlState.Decode(encoded);
        if (snapshot is null)
        {
            return false;
        }
        WizardUrlState.Apply(snapshot, State);
        State.Results.Clear();
        return true;
    }

    private static string? GetQueryValue(string queryOrUrl, string key)
    {
        var queryStart = queryOrUrl.IndexOf('?');
        var query = queryStart >= 0 ? queryOrUrl[(queryStart + 1)..] : queryOrUrl.TrimStart('?');
        var fragmentStart = query.IndexOf('#');
        if (fragmentStart >= 0)
        {
            query = query[..fragmentStart];
        }

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');
            var name = separator < 0 ? pair : pair[..separator];
            if (!string.Equals(Uri.UnescapeDataString(name.Replace('+', ' ')), key, StringComparison.Ordinal))
            {
                continue;
            }

            var value = separator < 0 ? string.Empty : pair[(separator + 1)..];
            return Uri.UnescapeDataString(value.Replace('+', ' '));
        }

        return null;
    }

    private string CurrentWizardUrl()
    {
        var encoded = WizardUrlState.Encode(State);
        if (encoded is null)
        {
            return Navigation.Uri;
        }
        return Navigation.GetUriWithQueryParameter("ws", encoded);
    }

    private async Task ReplaceUrlStateAsync(bool results = false)
    {
        if (_jsModule is null)
        {
            return;
        }

        await _jsModule.InvokeVoidAsync(
            results ? "replaceResultsState" : "replaceEditorState",
            State.CurrentStep,
            CurrentWizardUrl()
        );
    }

    private Task ReplaceEditorUrlStateAsync() => ReplaceUrlStateAsync(results: false);

    private Task ReplaceResultsUrlStateAsync() => ReplaceUrlStateAsync(results: true);

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

                await _jsModule.InvokeVoidAsync("disposeTouchSwipe", _swipeHost);
                await _jsModule.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Circuit already gone.
        }

        _selfReference?.Dispose();
        GC.SuppressFinalize(this);
    }
}
