using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Siteswaps.Generator.Components.GenerationWorkflow;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.Feeding;

public partial class FeedingPage : ComponentBase
{
    private const string PartnerB1 = "B1";
    private const string PartnerB2 = "B2";

    private NormalFeedSession? _session;
    private string? _loadError;
    private string? _loadedNotation;
    private FeedingPhase _phase = FeedingPhase.Setup;
    private GenerationWorkflowConfig _workflowConfig = new();
    private IReadOnlyList<LocalFeedSiteswap> _b1Locals = [];
    private IReadOnlyList<LocalFeedSiteswap> _b2Locals = [];
    private string? _activeRole;

    [Parameter, SupplyParameterFromQuery(Name = "s")]
    public string? SiteswapNotation { get; set; }

    [Inject]
    private IStringLocalizer<FeedingPage> L { get; set; } = default!;

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

    protected override void OnParametersSet()
    {
        if (string.Equals(_loadedNotation, SiteswapNotation, StringComparison.Ordinal))
        {
            return;
        }

        _loadedNotation = SiteswapNotation;
        TryLoadSession();
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
            _loadError = L["Provide a two-person siteswap via ?s=."];
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
        _phase = FeedingPhase.GenerateB1;
    }

    private void StartGenerateB2()
    {
        if (_session is null || !_session.CanGenerate)
        {
            return;
        }

        _workflowConfig = _session.ToGenerationWorkflowConfig("B2");
        _activeRole = "B2";
        _phase = FeedingPhase.GenerateB2;
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

            _phase = FeedingPhase.Setup;
            return;
        }

        _b2Locals = locals;
        if (locals.Count > 0)
        {
            _session.SelectSiteswap("B2", locals[0].Global);
        }

        _phase = FeedingPhase.Results;
    }

    private void SelectLocal(string role, LocalFeedSiteswap local)
    {
        _session?.SelectSiteswap(role, local.Global);
    }

    private void Rotate(int steps) => _session?.Rotate(steps);

    private void BackToSetup() => _phase = FeedingPhase.Setup;

    private void ShowCombination() => _phase = FeedingPhase.Results;
}
