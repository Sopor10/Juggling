using System.Collections.Generic;
using System.Linq;
using Siteswap.Details.StateDiagram.Graph;

namespace Siteswap.Details.StateDiagram;

public class StateGraphGenerator
{
    public StateGraph Generate(StateGraphGeneratorInput input)
    {
        var states = GenerateStates(input);
        var allTransitions = states.SelectMany(state => GenerateTransitions(state, input.Period)).ToHashSet();

        return new StateGraph(new Graph<State, int>(states, allTransitions));
    }

    private IEnumerable<Edge<State, int>> GenerateTransitions(State state, int maxHeight)
    {
        return state.Transitions(maxHeight);
    }
        
    private HashSet<State> GenerateStates(StateGraphGeneratorInput input) => StateGenerator.Create(input.NumberOfObjects, input.Period).ToHashSet();
}