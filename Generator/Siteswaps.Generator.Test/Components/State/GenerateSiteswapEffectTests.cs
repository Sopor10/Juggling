using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Components.State;

namespace Siteswaps.Generator.Test.Components.State;

[TestFixture]
public class GenerateSiteswapEffectTests
{
    [Test]
    public async Task Should_Generate_7566()
    {

        var result = await GenerateSiteswapEffect.CreateSiteswaps(new GenerateSiteswapsAction(new GeneratorState()
        {
            Objects = new ExactNumber{Number = 6},
            Throws = new List<Throw>{ Throw.Self , Throw.Zap, Throw.SinglePass}.ToImmutableList(),
            Period = new(4),
            MinThrow = 5,
            MaxThrow = 8,
            NumberOfJugglers = 2,
            CreateFilterFromThrowList = true
            
        }));
        result.Should().HaveCount(1);
    }
}