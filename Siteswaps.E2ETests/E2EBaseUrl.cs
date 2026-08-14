using PlaywrightTesting.Infrastructure;

namespace Siteswaps.E2ETests;

/// <summary>
/// Resolves the Blazor WASM base URL for E2E runs.
/// </summary>
public static class E2EBaseUrl
{
    public const string EnvironmentVariableName = "E2E_BASE_URL";

    public const string AspireDefault = "http://localhost:7021";

    /// <summary>
    /// Returns the self-hosted <see cref="BlazorWebassemblyFixture{TEntryPoint}"/> root URI.
    /// </summary>
    public static Uri FromFixture(BlazorWebassemblyFixture<Server.Program> fixture) =>
        EnsureTrailingSlash(fixture.RootUri);

    /// <summary>
    /// Returns <c>E2E_BASE_URL</c> when set; otherwise <see cref="AspireDefault"/>.
    /// </summary>
    public static Uri FromEnvironmentOrAspire()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return EnsureTrailingSlash(new Uri(fromEnvironment.Trim()));
        }

        return EnsureTrailingSlash(new Uri(AspireDefault));
    }

    public static Uri EnsureTrailingSlash(Uri uri)
    {
        var absolute = uri.AbsoluteUri;
        if (!absolute.EndsWith('/'))
        {
            absolute += "/";
        }

        return new Uri(absolute);
    }
}
