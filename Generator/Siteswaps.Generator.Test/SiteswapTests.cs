﻿using FluentAssertions;
using NUnit.Framework;
using Siteswaps.Generator.Generator;

namespace Siteswaps.Generator.Test;

public class SiteswapTests
{
    [Test]
    [TestCase(new sbyte[]{5,3,1}, 0, 2, "513")]
    [TestCase(new sbyte[]{5,3,1}, 1, 2, "351")]
    [TestCase(new sbyte[]{5,1}, 0, 2, "5")]
    [TestCase(new sbyte[]{5,1}, 1, 2, "1")]
    [TestCase(new sbyte[]{10,10,7,5,2,2}, 0, 2, "a72")]
    [TestCase(new sbyte[]{10,10,7,5,2,2}, 1, 2, "a52")]
    public void Local_Siteswaps(sbyte[] siteswap, int juggler, int maxJugglers, string expected)
    {
        Siteswap.CreateFromCorrect(siteswap).GetLocalSiteswap(juggler, maxJugglers).GlobalNotation.Should().Be(expected);
    }
}
