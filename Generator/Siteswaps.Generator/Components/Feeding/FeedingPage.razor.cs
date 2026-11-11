using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    private string? _loadedState;
    private bool _hasAttemptedLoad;
    private FeedingPhase _phase = FeedingPhase.Setup;
    private GenerationWorkflowConfig _workflowConfig = new();
    private string? _workflowSnapshot;
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

    [Parameter, SupplyParameterFromQuery(Name = "state")]
    public string? DeeplinkState { get; set; }

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

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
        string.IsNullOrWhiteSpace(_session?.FeederSiteswap.ToString() ?? SiteswapNotation)
            ? "details"
            : $"details?s={Uri.EscapeDataString(_session?.FeederSiteswap.ToString() ?? SiteswapNotation!)}";

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
            && string.Equals(_loadedState, DeeplinkState, StringComparison.Ordinal)
        )
        {
            return;
        }

        _hasAttemptedLoad = true;
        _loadedNotation = SiteswapNotation;
        _loadedState = DeeplinkState;
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
        await PersistStateAsync(push: false);
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
        _workflowSnapshot = null;

        var savedState = DecodeState(DeeplinkState);
        var feederNotation = savedState?.Feeder ?? SiteswapNotation;
        if (string.IsNullOrWhiteSpace(feederNotation))
        {
            _loadError = L["Open Feeding from a two-person siteswap on the details page."];
            return;
        }

        if (!Siteswap.TryCreate(feederNotation, out var feeder) || feeder is null)
        {
            _loadError = L["Invalid siteswap."];
            return;
        }

        try
        {
            _session = NormalFeedSession.FromFeederSiteswap(feeder);
            if (savedState is not null)
            {
                RestoreState(savedState);
            }
        }
        catch (ArgumentException)
        {
            _loadError = L["Invalid siteswap."];
        }

        _ = PersistStateAsync(push: false);
    }

    private void RestoreState(string encoded) => RestoreState(DecodeState(encoded));

    private void RestoreState(FeedingUrlState? saved)
    {
        if (saved is null || !Siteswap.TryCreate(saved.Feeder, out var feeder) || feeder is null)
        {
            return;
        }

        try
        {
            _session = NormalFeedSession.FromFeederSiteswap(feeder);
            _b1Locals = [];
            _b2Locals = [];
            _b1Results = [];
            _phase = FeedingPhase.Setup;
            var assignments = saved.Assignments ?? "";
            for (
                var beat = 0;
                beat < Math.Min(assignments.Length, _session.FeederSiteswap.Items.Length);
                beat++
            )
            {
                var assignment = assignments[beat] switch
                {
                    '1' => PartnerB1,
                    '2' => PartnerB2,
                    _ => null,
                };
                if (assignment is not null && _session.ThrowKinds[beat] == PassOrSelf.Pass)
                {
                    _session.AssignPass(beat, assignment);
                }
            }

            _session.ClubsB1 = SafeClubs(saved.B1Min, saved.B1Max, _session.ClubsB1);
            _session.ClubsB2 = SafeClubs(saved.B2Min, saved.B2Max, _session.ClubsB2);
            _b1Results = ParseSiteswaps(saved.B1Results, _session.FeederSiteswap.Items.Length);
            var b2Results = ParseSiteswaps(saved.B2Results, _session.FeederSiteswap.Items.Length);
            if (_session.ArePassAssignmentsComplete)
            {
                _b1Locals = _session.ProjectLocalResults(PartnerB1, _b1Results);
                _b2Locals = _session.ProjectLocalResults(PartnerB2, b2Results);
                RestoreSelection(PartnerB1, saved.B1Selected, saved.B1InterfaceBeat);
                RestoreSelection(PartnerB2, saved.B2Selected, saved.B2InterfaceBeat);
                RefreshInterfaceMoveTargets();
            }

            if (Enum.TryParse<FeedingPhase>(saved.Phase, true, out var phase))
            {
                _phase = phase switch
                {
                    FeedingPhase.SelectB1 when _b1Locals.Count > 0 => FeedingPhase.SelectB1,
                    FeedingPhase.SelectB2 when _b2Locals.Count > 0 => FeedingPhase.SelectB2,
                    FeedingPhase.Results
                        when _session.SelectedSiteswap(PartnerB1) is not null
                            && _session.SelectedSiteswap(PartnerB2) is not null =>
                        FeedingPhase.Results,
                    FeedingPhase.GenerateB1 or FeedingPhase.GenerateB2 when _session.CanGenerate =>
                        phase,
                    _ => FeedingPhase.Setup,
                };
                if (_phase is FeedingPhase.GenerateB1 or FeedingPhase.GenerateB2)
                {
                    _activeRole = _phase == FeedingPhase.GenerateB1 ? PartnerB1 : PartnerB2;
                    _workflowConfig = _session.ToGenerationWorkflowConfig(_activeRole);
                }
                _workflowSnapshot = saved.WorkflowSnapshot;
            }
        }
        catch (ArgumentException)
        {
            _phase = FeedingPhase.Setup;
            _b1Locals = [];
            _b2Locals = [];
            _b1Results = [];
        }
    }

    private void RestoreSelection(string role, string? notation, int? interfaceBeat)
    {
        if (string.IsNullOrWhiteSpace(notation) || _session is null)
        {
            return;
        }

        if (Siteswap.TryCreate(notation, out var selected) && selected is not null)
        {
            try
            {
                _session.SelectSiteswap(role, selected, interfaceBeat);
            }
            catch (ArgumentException)
            {
                // Invalid or no-longer-compatible selections are ignored.
            }
        }
    }

    private static Siteswap[] ParseSiteswaps(string[]? notations, int period)
    {
        if (notations is null)
        {
            return [];
        }

        return notations
            .Take(256)
            .Where(x => !string.IsNullOrWhiteSpace(x) && x.Length <= 128)
            .Select(x => Siteswap.TryCreate(x, out var parsed) ? parsed : null)
            .Where(x => x is not null && x.Items.Length == period)
            .Cast<Siteswap>()
            .ToArray();
    }

    private static Between SafeClubs(int min, int max, Between fallback)
    {
        min = Math.Clamp(min, 2, 30);
        max = Math.Clamp(max, 2, 30);
        return min <= max ? new Between { MinNumber = min, MaxNumber = max } : fallback;
    }

    private FeedingUrlState CreateUrlState()
    {
        var selectedB1 = _session?.SelectedSiteswap(PartnerB1);
        var selectedB2 = _session?.SelectedSiteswap(PartnerB2);
        var b1PassBeat = selectedB1 is null
            ? null
            : _session!.PassInterfaceBeatsOf(selectedB1).FirstOrDefault();
        var b2PassBeat = selectedB2 is null
            ? null
            : _session!.PassInterfaceBeatsOf(selectedB2).FirstOrDefault();
        return new FeedingUrlState
        {
            Feeder = _session?.FeederSiteswap.ToString() ?? SiteswapNotation ?? "",
            Phase = _phase.ToString(),
            Assignments = _session is null
                ? ""
                : string.Concat(
                    _session.PassAssignments.Select(x =>
                        x == PartnerB1 ? '1'
                        : x == PartnerB2 ? '2'
                        : '0'
                    )
                ),
            B1Min = _session?.ClubsB1.MinNumber ?? 2,
            B1Max = _session?.ClubsB1.MaxNumber ?? 2,
            B2Min = _session?.ClubsB2.MinNumber ?? 2,
            B2Max = _session?.ClubsB2.MaxNumber ?? 2,
            B1Results = _b1Results.Take(256).Select(x => x.ToString()).ToArray(),
            B2Results = _b2Locals.Take(256).Select(x => x.Global.ToString()).ToArray(),
            B1Selected = selectedB1?.ToString(),
            B2Selected = selectedB2?.ToString(),
            B1InterfaceBeat = selectedB1 is null ? null : b1PassBeat,
            B2InterfaceBeat = selectedB2 is null ? null : b2PassBeat,
            WorkflowSnapshot = _workflowSnapshot,
        };
    }

    private static string EncodeState(FeedingUrlState state)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Fastest, leaveOpen: true))
        {
            JsonSerializer.Serialize(gzip, state);
        }

        return Convert
            .ToBase64String(output.ToArray())
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static FeedingUrlState? DecodeState(string? encoded)
    {
        if (string.IsNullOrWhiteSpace(encoded) || encoded.Length > 32_000)
        {
            return null;
        }

        try
        {
            var base64 = encoded.Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(base64.Length + ((4 - base64.Length % 4) % 4), '=');
            using var input = new MemoryStream(Convert.FromBase64String(base64));
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            var buffer = new byte[4096];
            int read;
            while ((read = gzip.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (output.Length + read > 65_536)
                {
                    return null;
                }
                output.Write(buffer, 0, read);
            }
            return JsonSerializer.Deserialize<FeedingUrlState>(output.ToArray());
        }
        catch (Exception ex)
            when (ex is FormatException or InvalidDataException or JsonException or IOException)
        {
            return null;
        }
    }

    private async Task PersistStateAsync(bool push)
    {
        if (_jsModule is null || _session is null)
        {
            return;
        }

        var state = EncodeState(CreateUrlState());
        var url = Navigation.GetUriWithQueryParameter("s", _session.FeederSiteswap.ToString());
        url = Navigation.GetUriWithQueryParameter(url, "state", state);
        try
        {
            await _jsModule.InvokeVoidAsync(
                push ? "pushPhaseState" : "replacePhaseState",
                _phase.ToString(),
                url
            );
        }
        catch (JSDisconnectedException)
        {
            // Circuit already gone.
        }
    }

    private async Task OnWorkflowStateChanged(string? snapshot)
    {
        _workflowSnapshot = snapshot;
        await PersistStateAsync(push: false);
    }

    private sealed class FeedingUrlState
    {
        [JsonPropertyName("f")]
        public string Feeder { get; set; } = "";

        [JsonPropertyName("p")]
        public string Phase { get; set; } = "Setup";

        [JsonPropertyName("a")]
        public string Assignments { get; set; } = "";

        [JsonPropertyName("m1")]
        public int B1Min { get; set; } = 2;

        [JsonPropertyName("x1")]
        public int B1Max { get; set; } = 7;

        [JsonPropertyName("m2")]
        public int B2Min { get; set; } = 2;

        [JsonPropertyName("x2")]
        public int B2Max { get; set; } = 7;

        [JsonPropertyName("r1")]
        public string[]? B1Results { get; set; }

        [JsonPropertyName("r2")]
        public string[]? B2Results { get; set; }

        [JsonPropertyName("s1")]
        public string? B1Selected { get; set; }

        [JsonPropertyName("s2")]
        public string? B2Selected { get; set; }

        [JsonPropertyName("i1")]
        public int? B1InterfaceBeat { get; set; }

        [JsonPropertyName("i2")]
        public int? B2InterfaceBeat { get; set; }

        [JsonPropertyName("w")]
        public string? WorkflowSnapshot { get; set; }
    }

    private async Task AssignPass(int beatIndex, string partner)
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
        _workflowSnapshot = null;
        await PersistStateAsync(push: false);
    }

    private async Task OnClubsB1MinChanged(int value)
    {
        if (_session is null)
        {
            return;
        }

        _session.ClubsB1 = _session.ClubsB1 with { MinNumber = value };
        await PersistStateAsync(push: false);
    }

    private async Task OnClubsB1MaxChanged(int value)
    {
        if (_session is null)
        {
            return;
        }

        _session.ClubsB1 = _session.ClubsB1 with { MaxNumber = value };
        await PersistStateAsync(push: false);
    }

    private async Task OnClubsB2MinChanged(int value)
    {
        if (_session is null)
        {
            return;
        }

        _session.ClubsB2 = _session.ClubsB2 with { MinNumber = value };
        await PersistStateAsync(push: false);
    }

    private async Task OnClubsB2MaxChanged(int value)
    {
        if (_session is null)
        {
            return;
        }

        _session.ClubsB2 = _session.ClubsB2 with { MaxNumber = value };
        await PersistStateAsync(push: false);
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
        _workflowSnapshot = null;
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
            _ = SetPhaseAsync(FeedingPhase.SelectB1, push: false);
            _ = RestoreResultsFocusAsync();
            return;
        }

        _b2Locals = locals;
        if (locals.Count > 0)
        {
            _session.SelectSiteswap("B2", locals[0].Global);
        }

        _ = SetPhaseAsync(FeedingPhase.SelectB2, push: false);
        _ = RestoreResultsFocusAsync();
    }

    private async Task OnSelectFromResults(LocalFeedSiteswap local)
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

        await PersistStateAsync(push: false);
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

    private async Task MoveB1PassInterface(int beat)
    {
        if (_session?.TrySelectPassInterfaceBeat(PartnerB1, beat, _b1Results) != true)
        {
            return;
        }

        _b2Locals = [];
        RefreshInterfaceMoveTargets();
        await PersistStateAsync(push: false);
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
    public async Task OnBrowserPopState(string phase, string state)
    {
        RestoreState(state);
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
        await PersistStateAsync(push);
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
