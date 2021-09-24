using System;
using System.Collections.Generic;
using System.Linq;
using Siteswaps.Graph;

namespace Siteswaps
{
    public class StateGraphGenerator
    {
        public StateGraph Generate(SiteswapGeneratorInput input)
        {
            var states = GenerateStates(input);
            var allTransitions = states.SelectMany(GenerateTransitions).ToHashSet();

            return new StateGraph(new Graph<State, int>(states, allTransitions));
        }

        private IEnumerable<Edge<State,int>> GenerateTransitions(State state)
        {
            return state.Transitions();
        }
        
        private HashSet<State> GenerateStates(SiteswapGeneratorInput input) => new StateFactory().Create(input.NumberOfObjects, input.Period).ToHashSet();
    }
}