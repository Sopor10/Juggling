using System.Text.Json.Serialization;

namespace Siteswaps.Generator.Services;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppUpdateStatus
{
    Unsupported,
    UpToDate,
    UpdateAvailable,
    CheckFailed,
}
