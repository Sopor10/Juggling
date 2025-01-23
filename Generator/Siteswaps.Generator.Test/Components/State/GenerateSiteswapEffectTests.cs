using System.Collections.Immutable;
using FluentAssertions;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Moq;
using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Test.Components.State;

[TestFixture]
public class GenerateSiteswapEffectTests
{
    [Test]
    public async Task Should_Generate_7566()
    {
        var generatorState = new GeneratorState()
        {
            Objects = new ExactNumber { Number = 6 },
            Throws = new List<Throw> { Throw.Self, Throw.Zap, Throw.SinglePass }.ToImmutableList(),
            Period = new(4),
            MinThrow = 5,
            MaxThrow = 8,
            NumberOfJugglers = 2,
            CreateFilterFromThrowList = true,
        };
        var action = new GenerateSiteswapsAction(generatorState, new CancellationTokenSource());

        var sut = new GenerateSiteswapEffect(Mock.Of<INavigation>());

        var dispatcherMock = new DispatcherMock();
        await sut.HandleAsync(action, dispatcherMock);

        dispatcherMock
            .Actions.OfType<SiteswapGeneratedAction>()
            .Should()
            .ContainSingle()
            .Which.Siteswaps.Single()
            .Items.Should()
            .BeSameAs([7, 5, 6, 6]);
    }
}

public class DispatcherMock : IDispatcher
{
    public List<object> Actions { get; } = [];

    public void Dispatch(object action)
    {
        Actions.Add(action);
    }

#pragma warning disable CS0067
    public event EventHandler<ActionDispatchedEventArgs>? ActionDispatched;
#pragma warning restore CS0067
}
