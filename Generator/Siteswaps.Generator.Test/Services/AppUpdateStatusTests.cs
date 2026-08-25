using FluentAssertions;
using Siteswaps.Generator.Services;

namespace Siteswaps.Generator.Test.Services;

[TestFixture]
public sealed class AppUpdateStatusTests
{
    [TestCase(AppUpdateStatus.UpToDate, false)]
    [TestCase(AppUpdateStatus.UpdateAvailable, true)]
    [TestCase(AppUpdateStatus.Unsupported, false)]
    [TestCase(AppUpdateStatus.CheckFailed, false)]
    public void UpdateAvailable_Only_When_Status_Is_UpdateAvailable(
        AppUpdateStatus status,
        bool expectsUpdate
    )
    {
        var result = new AppUpdateCheckResult(status);

        (result.Status == AppUpdateStatus.UpdateAvailable).Should().Be(expectsUpdate);
    }
}
