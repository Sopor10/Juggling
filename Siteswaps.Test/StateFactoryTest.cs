﻿using FluentAssertions;
using Siteswap.Details.StateDiagram;
using Siteswap = Siteswap.Details.Siteswap;

namespace Siteswaps.Test;

public class StateFactoryTest
{
    [Test]
    public void StateFactory_Works()
    {
        var sut = new StateFactory();
        var result = sut.Create(2, 3);
        result.Count().Should().Be(3);
    }

    [Test]
    public void StateFactory_Works_2()
    {
        var sut = new StateFactory();
        var result = sut.Create(3, 5);
        result.Count().Should().Be(10);
    }
}
