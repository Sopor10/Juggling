using Microsoft.AspNetCore.Components;
using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Components.WizardPage.Filters;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.GenerationWorkflow;

/// <summary>
/// Hostable configure-and-generate workflow. Delivers the full siteswap list to the host
/// via <see cref="OnResultsReady"/>; does not render a results list or siteswap selection.
/// When <see cref="ChildContent"/> is set (e.g. Wizard embed), only that content is rendered.
/// </summary>
public partial class ConfiguredGenerationWorkflow : ComponentBase, IDisposable
{
    private GenerationWorkflowSession _session = GenerationWorkflowSession.Create(
        new GenerationWorkflowConfig()
    );

    private bool _isGenerating;
    private int _generationEpoch;
    private CancellationTokenSource? _generationCts;
    private FilterBottomSheet? _filterSheet;
    private FilterNode? _insertParent;
    private string? _appliedSnapshot;

    [Parameter]
    public GenerationWorkflowConfig Config { get; set; } = new();

    /// <summary>Optional serialized editable state supplied by the host.</summary>
    [Parameter]
    public string? StateSnapshot { get; set; }

    /// <summary>Raised whenever the host-editable workflow state changes.</summary>
    [Parameter]
    public EventCallback<string?> StateChanged { get; set; }

    /// <summary>Fired with the complete generated list. Host owns display/selection.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<Siteswap>> OnResultsReady { get; set; }

    [Parameter]
    public EventCallback OnCancelled { get; set; }

    /// <summary>Optional custom UI (e.g. Wizard steps). Suppresses the default control surface.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public GenerationWorkflowState State => _session.State;

    public GenerationWorkflowSession Session => _session;

    public bool IsPeriodVisible => _session.IsPeriodVisible;

    public bool IsJugglersVisible => _session.IsJugglersVisible;

    public bool IsInterfaceVisible => _session.IsInterfaceVisible;

    public bool IsGenerating => _isGenerating;

    protected override void OnParametersSet()
    {
        if (!_session.Config.Equals(Config))
        {
            CancelInFlightGeneration();
            _generationEpoch++;
            _session = GenerationWorkflowSession.Create(Config);
            _appliedSnapshot = null;
        }

        if (!string.Equals(StateSnapshot, _appliedSnapshot, StringComparison.Ordinal))
        {
            _session.ApplySnapshot(StateSnapshot);
            _appliedSnapshot = StateSnapshot;
        }
    }

    public async Task GenerateAsync(CancellationToken cancellationToken = default)
    {
        if (_isGenerating)
        {
            return;
        }

        _isGenerating = true;
        var epoch = _generationEpoch;
        var session = _session;
        _generationCts?.Cancel();
        _generationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _generationCts.Token;

        try
        {
            // Yield so a concurrent Config swap can cancel before work starts.
            await Task.Yield();
            if (epoch != _generationEpoch || token.IsCancellationRequested)
            {
                await OnCancelled.InvokeAsync();
                return;
            }

            var results = await session.GenerateAsync(token);
            if (epoch != _generationEpoch || token.IsCancellationRequested || session != _session)
            {
                await OnCancelled.InvokeAsync();
                return;
            }

            await OnResultsReady.InvokeAsync(results);
        }
        catch (OperationCanceledException)
        {
            await OnCancelled.InvokeAsync();
        }
        finally
        {
            _isGenerating = false;
        }
    }

    public void CancelGeneration() => CancelInFlightGeneration();

    public void Dispose()
    {
        CancelInFlightGeneration();
        GC.SuppressFinalize(this);
    }

    private void CancelInFlightGeneration()
    {
        if (_generationCts is null)
        {
            return;
        }

        var cts = _generationCts;
        _generationCts = null;
        try
        {
            cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Generation already finished.
        }
        finally
        {
            cts.Dispose();
        }
    }

    private void OnPeriodChanged(int value) => _session.SetPeriod(value);

    private void OnJugglersChanged(int value) => _session.SetNumberOfJugglers(value);

    private async Task OnClubsMinChanged(int value)
    {
        _session.SetClubs(State.Clubs with { MinNumber = value });
        await NotifyStateChangedAsync();
    }

    private async Task OnClubsMaxChanged(int value)
    {
        _session.SetClubs(State.Clubs with { MaxNumber = value });
        await NotifyStateChangedAsync();
    }

    private async Task OnRemoveFilter(int id)
    {
        _session.RemoveFilter(id);
        await NotifyStateChangedAsync();
    }

    private async Task OnFilterTreeChanged(FilterTree tree)
    {
        _session.ReplaceEditableFilterTree(tree);
        await NotifyStateChangedAsync();
    }

    private async Task OnThrowsChanged() => await NotifyStateChangedAsync();

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

    private async Task OnFilterAdded(IFilterInformation filter)
    {
        var id = State.NextFilterId();
        var leaf = new FilterLeaf(new WizardIdentifiedFilter(id, filter));
        var editable = WizardFilterTree.AddLeaf(_session.EditableFilterTree, _insertParent, leaf);
        _session.ReplaceEditableFilterTree(editable);
        _insertParent = null;
        await NotifyStateChangedAsync();
    }

    private async Task OnFilterEdited((int Id, IFilterInformation Filter) args)
    {
        if (_session.LockedInterfaceFilterId is { } lockedId && lockedId == args.Id)
        {
            return;
        }

        State.FilterTree = WizardFilterTree.ReplaceLeaf(State.FilterTree, args.Id, args.Filter);
        await NotifyStateChangedAsync();
    }

    private Task NotifyStateChangedAsync() => StateChanged.InvokeAsync(_session.EncodeSnapshot());

    private async Task OnGenerateClicked() => await GenerateAsync(CancellationToken.None);
}
