using System.Collections.Generic;
using System.Linq;
using Siteswaps.StateDiagram.Graph;

namespace Siteswaps.StateDiagram
{
    public class StateGraphGenerator
    {
        public StateGraph Generate(StateGraphGeneratorInput input)
        {
            var states = GenerateStates(input);
            var allTransitions = states.SelectMany(GenerateTransitions).ToHashSet();

            return new StateGraph(new Graph<State, int>(states, allTransitions));
        }

        private IEnumerable<Edge<State,int>> GenerateTransitions(State state)
        {
            return state.Transitions();
        }
        
        private HashSet<State> GenerateStates(StateGraphGeneratorInput input) => new StateFactory().Create(input.NumberOfObjects, input.Period).ToHashSet();
    }
}