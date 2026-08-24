using Siteswaps.Generator.Components.State;
using Siteswaps.Generator.Components.State.FilterTrees;
using Siteswaps.Generator.Components.WizardPage;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.GenerationWorkflow;

/// <summary>
/// Editable surface for a generation workflow. Locked period/jugglers ignore direct writes
/// and always expose the locked values even if <see cref="Inner"/> is mutated;
/// clubs, throws, and filters remain mutable on the underlying <see cref="WizardState"/>.
/// </summary>
public sealed class GenerationWorkflowState
{
    private readonly GenerationWorkflowConfig _config;
    private readonly WizardState _inner;

    internal GenerationWorkflowState(GenerationWorkflowConfig config, WizardState inner)
    {
        _config = config;
        _inner = inner;
    }

    internal WizardState Inner => _inner;

    public Period Period
    {
        get => _config.HasLockedPeriod ? new Period(_config.Period!.Value) : _inner.Period;
        set
        {
            if (_config.HasLockedPeriod)
            {
                return;
            }

            _inner.Period = value;
        }
    }

    public int NumberOfJugglers
    {
        get =>
            _config.HasLockedJugglers ? _config.NumberOfJugglers!.Value : _inner.NumberOfJugglers;
        set
        {
            if (_config.HasLockedJugglers)
            {
                return;
            }

            _inner.NumberOfJugglers = value;
        }
    }

    public Between Clubs
    {
        get => _inner.Clubs;
        set => _inner.Clubs = value;
    }

    public List<Throw> AllowedThrows => _inner.AllowedThrows;

    public FilterTree FilterTree
    {
        get => _inner.FilterTree;
        set => _inner.FilterTree = value;
    }

    public bool ShowThrowNames
    {
        get => _inner.ShowThrowNames;
        set => _inner.ShowThrowNames = value;
    }

    public int MaxThrowHeight
    {
        get => _inner.MaxThrowHeight;
        set => _inner.MaxThrowHeight = value;
    }

    public WizardPhase Phase
    {
        get => _inner.Phase;
        set => _inner.Phase = value;
    }

    public List<Siteswap> Results => _inner.Results;

    public int NextFilterId() => _inner.NextFilterId();

    public void ApplyMaxThrowHeight(int maxHeight) => _inner.ApplyMaxThrowHeight(maxHeight);
}
