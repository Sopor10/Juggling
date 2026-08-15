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
    private string? _activeRole;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<FeedingPage>? _selfReference;
    private bool _historyReady;

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
        Results,
    }

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
            _b1Locals = locals;
            if (locals.Count > 0)
            {
                _session.SelectSiteswap("B1", locals[0].Global);
            }

            _ = NavigateBackOrSetSetupAsync();
            return;
        }

        _b2Locals = locals;
        if (locals.Count > 0)
        {
            _session.SelectSiteswap("B2", locals[0].Global);
        }

        _ = NavigateBackOrSetSetupAsync();
    }

    private void SelectLocal(string role, LocalFeedSiteswap local)
    {
        _session?.SelectSiteswap(role, local.Global);
    }

    private void Rotate(int steps) => _session?.Rotate(steps);

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

        if (parsed == FeedingPhase.Results && (_session is null || _b2Locals.Count == 0))
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

    private async Task NavigateBackOrSetSetupAsync()
    {
        var shouldPop = _historyReady && _jsModule is not null && _phase != FeedingPhase.Setup;
        _phase = FeedingPhase.Setup;

        if (!shouldPop || _jsModule is null)
        {
            return;
        }

        try
        {
            await _jsModule.InvokeVoidAsync("back");
        }
        catch (JSDisconnectedException)
        {
            // Circuit already gone.
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
    }
}
