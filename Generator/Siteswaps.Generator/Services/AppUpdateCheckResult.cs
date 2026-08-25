namespace Siteswaps.Generator.Services;

public sealed record AppUpdateCheckResult(AppUpdateStatus Status, string? Message = null);
