using Microsoft.AspNetCore.Components;

namespace Siteswaps.Jif.Components.Navigation;

/// <summary>
/// Centralizes URL-backed state navigation and whether an update creates a browser-history entry.
/// State encoding remains owned by each feature; navigating to its encoded URL is shared here.
/// </summary>
public sealed class UrlStateNavigation(NavigationManager navigation)
{
    public void SetQueryParameter(string name, string? value, bool replace = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var uri = WithQueryParameter(navigation.Uri, name, value);
        if (string.Equals(uri, navigation.Uri, StringComparison.Ordinal))
            return;

        navigation.NavigateTo(uri, replace: replace);
    }

    public string GetUriWithQueryParameter(string name, string? value) =>
        WithQueryParameter(navigation.Uri, name, value);

    public string WithQueryParameter(string uri, string name, string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var builder = new UriBuilder(navigation.ToAbsoluteUri(uri));
        var parameters = builder
            .Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(pair => pair.Split('=', 2))
            .Select(parts =>
                (
                    Name: Uri.UnescapeDataString(parts[0].Replace('+', ' ')),
                    Value: parts.Length > 1
                        ? Uri.UnescapeDataString(parts[1].Replace('+', ' '))
                        : string.Empty
                )
            )
            .Where(parameter => !parameter.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (value is not null)
            parameters.Add((name, value));

        builder.Query = string.Join(
            "&",
            parameters.Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Name)}={Uri.EscapeDataString(parameter.Value)}"
            )
        );
        return builder.Uri.ToString();
    }

    public void NavigateTo(string uri, bool replace = false) =>
        navigation.NavigateTo(uri, replace: replace);
}
