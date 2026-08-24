using FluentAssertions;
using Siteswaps.Generator.Components.Internal;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Test.Components.Internal;

/// <summary>
/// Detail links must be app-relative so &lt;base href&gt; / PathBase (e.g. /pr-preview/pr-N/) is respected.
/// Root-absolute paths like /details jump to production origin root on GitHub Pages previews.
/// </summary>
[TestFixture]
public class SiteswapCardNavigationTests
{
    [Test]
    public void InternalDetailView_Is_App_Relative_Not_Root_Absolute()
    {
        var siteswap = Siteswap.CreateFromCorrect(7, 5, 6);
        var view = new SiteswapCard.SiteswapView(siteswap, 2);

        view.InternalDetailView.Should().Be("details?s=756&n=2");
        view.InternalDetailView.Should().NotStartWith("/");
        view.InternalDetailView.Should().NotContain("passing.zone");
        view.InternalDetailView.Should().NotContain("://");
    }
}
