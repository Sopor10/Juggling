using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.GenerationWorkflow;

/// <summary>
/// Hostable generation workflow session: locked inputs, editable clubs/throws/filters,
/// and list-only generation via <see cref="SiteswapListGeneration"/>. No feeding state.
/// </summary>
public sealed class GenerationWorkflowSession
{
    private GenerationWorkflowSession(GenerationWorkflowConfig config, WizardState inner)
    {
        Config = config;
        State = new GenerationWorkflowState(config, inner);
    }

    public GenerationWorkflowConfig Config { get; }

    public GenerationWorkflowState State { get; }

    public bool IsPeriodEditable => !Config.HasLockedPeriod;

    public bool IsJugglersEditable => !Config.HasLockedJugglers;

    public bool IsPeriodVisible => IsPeriodEditable;

    public bool IsJugglersVisible => IsJugglersEditable;

    /// <summary>Locked Pass/Self interface is never shown on the host filter surface.</summary>
    public bool IsInterfaceVisible => !HasLockedInterface;

    public bool HasLockedInterface => LockedInterfaceFilterId is not null;

    public int? LockedInterfaceFilterId { get; private set; }

    /// <summary>Filter tree without the locked interface leaf (for host UI).</summary>
    public FilterTree EditableFilterTree =>
        LockedInterfaceFilterId is { } lockedId
            ? WizardFilterTree.RemoveLeaf(State.FilterTree, lockedId)
            : State.FilterTree;

    public static GenerationWorkflowSession Create(GenerationWorkflowConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        // Snapshot PassSelfInterface so caller mutations cannot alter the locked filter.
        if (config.PassSelfInterface is { } pattern)
        {
            config = config with { PassSelfInterface = pattern.ToList() };
        }

        if (config.ThrowInterface is { } throwPattern)
        {
            config = config with { ThrowInterface = throwPattern.ToList() };
        }

        if (config.PassSelfInterface is { Count: > 0 } && config.Period is null)
        {
            throw new ArgumentException(
                "PassSelfInterface requires Period (and matching length).",
                nameof(config)
            );
        }

        if (
            config.PassSelfInterface is { Count: > 0 }
            && config.Period is { } period
            && config.PassSelfInterface.Count != period
        )
        {
            throw new ArgumentException(
                "PassSelfInterface length must match Period.",
                nameof(config)
            );
        }

        if (
            config.ThrowInterface is { Count: > 0 }
            && config.Period is { } throwPeriod
            && config.ThrowInterface.Count != throwPeriod
        )
        {
            throw new ArgumentException("ThrowInterface length must match Period.", nameof(config));
        }

        var inner = new WizardState();
        var session = new GenerationWorkflowSession(config, inner);
        session.ApplyConfig();
        return session;
    }

    public void SetPeriod(int value)
    {
        EnsurePeriodEditable();
        State.Inner.Period = new Period(value);
    }

    public void SetNumberOfJugglers(int value)
    {
        EnsureJugglersEditable();
        State.Inner.NumberOfJugglers = value;
    }

    public void SetClubs(Between clubs) => State.Clubs = clubs;

    public void RemoveFilter(int id)
    {
        if (LockedInterfaceFilterId is { } lockedId && lockedId == id)
        {
            throw new InvalidOperationException(
                "The locked Pass/Self interface filter cannot be removed."
            );
        }

        State.FilterTree = WizardFilterTree.RemoveLeaf(State.FilterTree, id);
    }

    /// <summary>
    /// Replaces the editable (non-locked) filter tree and re-injects the locked interface.
    /// </summary>
    public void ReplaceEditableFilterTree(FilterTree tree)
    {
        State.FilterTree = tree;
        EnforceLocks();
    }

    public Task<IReadOnlyList<Siteswap>> GenerateAsync(
        CancellationToken cancellationToken = default
    )
    {
        EnforceLocks();
        return SiteswapListGeneration.GenerateAsync(State.Inner, cancellationToken);
    }

    private void ApplyConfig()
    {
        if (Config.Period is { } period)
        {
            State.Inner.Period = new Period(period);
        }

        if (Config.NumberOfJugglers is { } jugglers)
        {
            State.Inner.NumberOfJugglers = jugglers;
        }

        if (Config.Clubs is { } clubs)
        {
            State.Inner.Clubs = clubs;
        }

        if (Config.PassSelfInterface is { Count: > 0 } pattern)
        {
            InjectOrReplaceLockedInterface(pattern, Config.ThrowInterface);
        }
    }

    private void EnforceLocks()
    {
        if (Config.Period is { } period)
        {
            State.Inner.Period = new Period(period);
        }

        if (Config.NumberOfJugglers is { } jugglers)
        {
            State.Inner.NumberOfJugglers = jugglers;
        }

        if (Config.PassSelfInterface is { Count: > 0 } pattern)
        {
            InjectOrReplaceLockedInterface(pattern, Config.ThrowInterface);
        }
    }

    private void InjectOrReplaceLockedInterface(
        IReadOnlyList<Throw> pattern,
        IReadOnlyList<Throw>? throwPattern
    )
    {
        var filter = new InterfaceFilterInformation(
            pattern.ToList(),
            AllowRotation: true,
            throwPattern?.ToList()
        );

        if (LockedInterfaceFilterId is { } lockedId)
        {
            if (WizardFilterTree.FindLeaf(State.FilterTree, lockedId) is null)
            {
                var leaf = new FilterLeaf(new WizardIdentifiedFilter(lockedId, filter));
                State.FilterTree = WizardFilterTree.AddLeaf(
                    State.FilterTree,
                    parentGroup: null,
                    leaf
                );
            }
            else
            {
                State.FilterTree = WizardFilterTree.ReplaceLeaf(State.FilterTree, lockedId, filter);
            }

            return;
        }

        var id = State.NextFilterId();
        LockedInterfaceFilterId = id;
        var newLeaf = new FilterLeaf(new WizardIdentifiedFilter(id, filter));
        State.FilterTree = WizardFilterTree.AddLeaf(State.FilterTree, parentGroup: null, newLeaf);
    }

    private void EnsurePeriodEditable()
    {
        if (!IsPeriodEditable)
        {
            throw new InvalidOperationException("Period is locked for this generation workflow.");
        }
    }

    private void EnsureJugglersEditable()
    {
        if (!IsJugglersEditable)
        {
            throw new InvalidOperationException(
                "Number of jugglers is locked for this generation workflow."
            );
        }
    }
}
