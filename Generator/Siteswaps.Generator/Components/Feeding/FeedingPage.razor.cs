using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Siteswaps.Generator.Components.GenerationWorkflow;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.Feeding;

public partial class FeedingPage : ComponentBase, IAsyncDisposable
{
    private const string PartnerB1 = "B1";
    private const string PartnerB2 = "B2";

    private NormalFeedSession? _session;
    private string? _loadError;
    private string? _loadedNotation;
    private bool _hasAttemptedLoad;
    private FeedingPhase _phase = FeedingPhase.Setup;
    private GenerationWorkflowConfig _workflowConfig = new();
    private IReadOnlyList<LocalFeedSiteswap> _b1Locals = [];
    private IReadOnlyList<LocalFeedSiteswap> _b2Locals = [];
    private IReadOnlyList<Siteswap> _b1Results = [];
    private IReadOnlyList<int> _interfaceMoveTargets = [];
    private string? _activeRole;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<FeedingPage>? _selfReference;
    private bool _historyReady;
    private ElementReference _setupTitle;
    private FeedingLocalResultsView? _resultsView;

    [Parameter, SupplyParameterFromQuery(Name = "s")]
    public string? SiteswapNotation { get; set; }

    [Inject]
    private IStringLocalizer<FeedingPage> L { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    private enum FeedingPhase
    {
        Setup,
        GenerateB1,
        GenerateB2,
        SelectB1,
        SelectB2,
        Results,
    }

    private string DetailsBackHref =>
        string.IsNullOrWhiteSpace(SiteswapNotation)
            ? "details"
            : $"details?s={Uri.EscapeDataString(SiteswapNotation)}";

    private string HeaderTitle =>
        _phase switch
        {
            FeedingPhase.GenerateB1 => L["Generate pattern for {0}", PartnerB1].Value,
            FeedingPhase.GenerateB2 => L["Generate pattern for {0}", PartnerB2].Value,
            FeedingPhase.SelectB1 => L["B1 local results"].Value,
            FeedingPhase.SelectB2 => L["B2 local results"].Value,
            FeedingPhase.Results => L["Feed combination"].Value,
            _ => L["3-person feed"].Value,
        };

    private string? InterfaceMoveRole =>
        _b1Results.Count > 0 && _session?.SelectedSiteswap(PartnerB1) is not null
            ? PartnerB1
            : null;

    private string? LocalizedBlockReason =>
        _session?.GenerationBlockCode switch
        {
            null or GenerationBlockCode.None => null,
            GenerationBlockCode.NoPasses => L["Feeder has no passes."].Value,
            GenerationBlockCode.IncompleteAssignments => L[
                "Pass assignments are incomplete."
            ].Value,
            GenerationBlockCode.SingleFedeeOnly => L[
                "Each fedee must receive at least one pass."
            ].Value,
            GenerationBlockCode.ClubsUnset => L["Club bounds for B1 and B2 must be set."].Value,
            _ => L["Generation is blocked."].Value,
        };

    private string SetupLeadText =>
        _b2Locals.Count > 0
            ? L["Both fedees are ready. Review the local patterns, then show the combination."]
        : _b1Locals.Count > 0
            ? _session?.SelectedSiteswap(PartnerB1) is not null
                    ? L["B1 is ready. A local pattern is selected — generate B2."]
                : L["B1 is ready. Pick a local pattern, then generate B2."]
        : L[
            "A keeps this two-person pattern. Assign each pass to B1 or B2, then generate both fedees."
        ];

    protected override void OnParametersSet()
    {
        if (
            _hasAttemptedLoad
            && string.Equals(_loadedNotation, SiteswapNotation, StringComparison.Ordinal)
        )
        {
            return;
        }

        _hasAttemptedLoad = true;
        _loadedNotation = SiteswapNotation;
        TryLoadSession();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            "./_content/Siteswaps.Generator/Components/Feeding/FeedingPage.razor.js"
        );
        _selfReference = DotNetObjectReference.Create(this);
        await _jsModule.InvokeVoidAsync("initHistory", _selfReference, _phase.ToString());
        _historyReady = true;
    }

    private void TryLoadSession()
    {
        _loadError = null;
        _session = null;
        _phase = FeedingPhase.Setup;
        _b1Locals = [];
        _b2Locals = [];
        _b1Results = [];
        _interfaceMoveTargets = [];

        if (string.IsNullOrWhiteSpace(SiteswapNotation))
        {
            _loadError = L["Open Feeding from a two-person siteswap on the details page."];
            return;
        }

        if (!Siteswap.TryCreate(SiteswapNotation, out var feeder) || feeder is null)
        {
            _loadError = L["Invalid siteswap."];
            return;
        }

        try
        {
            _session = NormalFeedSession.FromFeederSiteswap(feeder);
        }
        catch (ArgumentException)
        {
            _loadError = L["Invalid siteswap."];
        }

        _ = ReplaceHistoryPhaseAsync(FeedingPhase.Setup);
    }

    private void AssignPass(int beatIndex, string partner)
    {
        if (_session is null)
        {
            return;
        }

        _session.AssignPass(beatIndex, partner);
        _b1Locals = [];
        _b2Locals = [];
        _b1Results = [];
        _interfaceMoveTargets = [];
    }

    private void OnClubsB1MinChanged(int value)
    {
        if (_session is null)
        {
            return;
        }

        _session.ClubsB1 = _session.ClubsB1 with { MinNumber = value };
    }

    private void OnClubsB1MaxChanged(int value)
    {
        if (_session is null)
        {
            return;
        }

        _session.ClubsB1 = _session.ClubsB1 with { MaxNumber = value };
    }

    private void OnClubsB2MinChanged(int value)
    {
        if (_session is null)
        {
            return;
        }

        _session.ClubsB2 = _session.ClubsB2 with { MinNumber = value };
    }

    private void OnClubsB2MaxChanged(int value)
    {
        if (_session is null)
        {
            return;
        }

        _session.ClubsB2 = _session.ClubsB2 with { MaxNumber = value };
    }

    private void StartGenerateB1()
    {
        if (_session is null || !_session.CanGenerate)
        {
            return;
        }

        _workflowConfig = _session.ToGenerationWorkflowConfig("B1");
        _activeRole = "B1";
        _ = SetPhaseAsync(FeedingPhase.GenerateB1, push: true);
    }

    private void StartGenerateB2()
    {
        if (_session is null || !_session.CanGenerate)
        {
            return;
        }

        _workflowConfig = _session.ToGenerationWorkflowConfig("B2");
        _activeRole = "B2";
        _ = SetPhaseAsync(FeedingPhase.GenerateB2, push: true);
    }

    private void OnWorkflowResults(IReadOnlyList<Siteswap> results)
    {
        if (_session is null || _activeRole is null)
        {
            return;
        }

        var locals = _session.ProjectLocalResults(_activeRole, results);
        if (_activeRole == "B1")
        {
            _b1Results = results;
            _b1Locals = locals;
            if (locals.Count > 0)
            {
                _session.SelectSiteswap("B1", locals[0].Global);
            }

            RefreshInterfaceMoveTargets();
            _ = SetPhaseAsync(FeedingPhase.SelectB1, push: true);
            _ = RestoreResultsFocusAsync();
            return;
        }

        _b2Locals = locals;
        if (locals.Count > 0)
        {
            _session.SelectSiteswap("B2", locals[0].Global);
        }

        _ = SetPhaseAsync(FeedingPhase.SelectB2, push: true);
        _ = RestoreResultsFocusAsync();
    }

    private void OnSelectFromResults(LocalFeedSiteswap local)
    {
        if (_session is null)
        {
            return;
        }

        var role = _phase == FeedingPhase.SelectB1 ? PartnerB1 : PartnerB2;
        _session.SelectSiteswap(role, local.Global);
        if (role == PartnerB1)
        {
            RefreshInterfaceMoveTargets();
            _b2Locals = [];
        }

        StateHasChanged();
    }

    private void ConfirmResultsSelection()
    {
        if (_phase == FeedingPhase.SelectB1)
        {
            StartGenerateB2();
            return;
        }

        if (_phase == FeedingPhase.SelectB2)
        {
            ShowCombination();
        }
    }

    private void BackFromResultsSelection() => BackToSetup();

    private void RefreshInterfaceMoveTargets()
    {
        _interfaceMoveTargets =
            _session?.SelectablePassInterfaceBeatsFor(PartnerB1, _b1Results) ?? [];
    }

    private void MoveB1PassInterface(int beat)
    {
        if (_session?.TrySelectPassInterfaceBeat(PartnerB1, beat, _b1Results) != true)
        {
            return;
        }

        _b2Locals = [];
        RefreshInterfaceMoveTargets();
    }

    private string? DescribeEmptyB2()
    {
        if (_session is null || _b2Locals.Count > 0)
        {
            return null;
        }

        var free = _session
            .FeederInterfaceOccupancy()
            .Where(slot => slot.Owner == FeedInterfaceOwner.Free)
            .Select(slot => L["Beat {0}", slot.Beat + 1].Value)
            .ToList();
        return free.Count == 0
            ? L["A's Interface is fully constrained after B1."].Value
            : L["Still free after B1: {0}", string.Join(", ", free)].Value;
    }

    private static string FormatThrowDigit(int height) =>
        height < 10 ? height.ToString(System.Globalization.CultureInfo.InvariantCulture)
        : height < 36 ? ((char)('a' + height - 10)).ToString()
        : height.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private void BackToSetup()
    {
        if (_phase == FeedingPhase.Results)
        {
            _session?.RestoreOriginalRotation();
        }

        _ = NavigateBackOrSetSetupAsync();
    }

    private void ShowCombination() => _ = SetPhaseAsync(FeedingPhase.Results, push: true);

    [JSInvokable]
    public async Task OnBrowserPopState(string phase)
    {
        if (!Enum.TryParse<FeedingPhase>(phase, ignoreCase: true, out var parsed))
        {
            parsed = FeedingPhase.Setup;
        }

        if (
            parsed is FeedingPhase.SelectB1 && (_session is null || _b1Locals.Count == 0)
            || parsed is FeedingPhase.SelectB2 or FeedingPhase.Results
                && (_session is null || _b2Locals.Count == 0)
        )
        {
            parsed = FeedingPhase.Setup;
        }

        if (
            parsed is FeedingPhase.GenerateB1 or FeedingPhase.GenerateB2
            && (_session is null || !_session.CanGenerate)
        )
        {
            parsed = FeedingPhase.Setup;
        }

        if (_phase == FeedingPhase.Results && parsed != FeedingPhase.Results)
        {
            _session?.RestoreOriginalRotation();
        }

        if (parsed == FeedingPhase.GenerateB1)
        {
            _activeRole = PartnerB1;
            if (_session is not null)
            {
                _workflowConfig = _session.ToGenerationWorkflowConfig(PartnerB1);
            }
        }
        else if (parsed == FeedingPhase.GenerateB2)
        {
            _activeRole = PartnerB2;
            if (_session is not null)
            {
                _workflowConfig = _session.ToGenerationWorkflowConfig(PartnerB2);
            }
        }

        _phase = parsed;
        await InvokeAsync(StateHasChanged);
        if (parsed == FeedingPhase.Setup)
        {
            await Task.Delay(150);
            await FocusSetupHeadingAsync();
        }
        else if (parsed is FeedingPhase.SelectB1 or FeedingPhase.SelectB2)
        {
            await Task.Delay(150);
            if (_resultsView is not null)
            {
                await _resultsView.FocusTitleAsync();
            }
        }
    }

    private async Task SetPhaseAsync(FeedingPhase phase, bool push)
    {
        _phase = phase;
        if (!_historyReady || _jsModule is null)
        {
            return;
        }

        try
        {
            await _jsModule.InvokeVoidAsync(
                push ? "pushPhaseState" : "replacePhaseState",
                phase.ToString()
            );
        }
        catch (JSDisconnectedException)
        {
            // Circuit already gone.
        }
    }

    private async Task ReplaceHistoryPhaseAsync(FeedingPhase phase)
    {
        if (!_historyReady || _jsModule is null)
        {
            return;
        }

        try
        {
            await _jsModule.InvokeVoidAsync("replacePhaseState", phase.ToString());
        }
        catch (JSDisconnectedException)
        {
            // Circuit already gone.
        }
    }

    private async Task RestoreSetupFocusAsync()
    {
        await InvokeAsync(StateHasChanged);
        await Task.Delay(150);
        await FocusSetupHeadingAsync();
    }

    private async Task RestoreResultsFocusAsync()
    {
        await InvokeAsync(StateHasChanged);
        await Task.Delay(150);
        if (_resultsView is not null)
        {
            await _resultsView.FocusTitleAsync();
        }
    }

    private async Task NavigateBackOrSetSetupAsync()
    {
        var shouldPop = _historyReady && _jsModule is not null && _phase != FeedingPhase.Setup;
        _phase = FeedingPhase.Setup;

        if (!shouldPop || _jsModule is null)
        {
            await InvokeAsync(StateHasChanged);
            await Task.Delay(150);
            await FocusSetupHeadingAsync();
            return;
        }

        try
        {
            await _jsModule.InvokeVoidAsync("back");
        }
        catch (JSDisconnectedException)
        {
            // Circuit already gone.
            await InvokeAsync(StateHasChanged);
            await Task.Delay(150);
            await FocusSetupHeadingAsync();
        }
    }

    private async Task FocusSetupHeadingAsync()
    {
        try
        {
            await _setupTitle.FocusAsync();
        }
        catch (JSException)
        {
            // Title may not be attached yet during teardown / phase switches.
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_jsModule is not null)
            {
                if (_selfReference is not null)
                {
                    await _jsModule.InvokeVoidAsync("disposeHistory", _selfReference);
                }

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
