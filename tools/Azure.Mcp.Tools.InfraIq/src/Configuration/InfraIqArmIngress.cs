// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Azure.Mcp.Tools.InfraIq.Configuration;

/// <summary>
/// Exact-destination rules for the InfraIQ ARM/RPaaS ingress. Local development accepts only
/// <see cref="DevelopmentOrigin"/>, an externally approved integration assumption.
/// </summary>
internal static class InfraIqArmIngress
{
    public const string ConfigurationKey = "InfraIq:ArmIngressOrigin";
    public const string DevelopmentOrigin = "https://eastus2euap.management.azure.com";
    public const string MissingOriginMessage =
        "The InfraIQ ARM ingress origin is not configured. Set 'InfraIq:ArmIngressOrigin' to an approved origin.";

    private const string DevelopmentHost = "eastus2euap.management.azure.com";

    /// <summary>
    /// Returns the configured origin, or the approved development origin when the value is absent or blank.
    /// Non-blank values are returned unchanged so unsafe values still fail validation.
    /// </summary>
    public static string ResolveConfiguredOrigin(string? configuredOrigin) =>
        string.IsNullOrWhiteSpace(configuredOrigin) ? DevelopmentOrigin : configuredOrigin;

    /// <summary>
    /// Parses and validates a configured ingress origin.
    /// </summary>
    /// <param name="origin">The configured origin text.</param>
    /// <param name="originUri">The canonical approved origin when valid.</param>
    /// <param name="error">A static, value-free failure message when invalid.</param>
    public static bool TryParseOrigin(
        string? origin,
        [NotNullWhen(true)] out Uri? originUri,
        [NotNullWhen(false)] out string? error)
    {
        originUri = null;

        if (string.IsNullOrWhiteSpace(origin))
        {
            error = MissingOriginMessage;
            return false;
        }

        if (!HasOnlySafeCharacters(origin)
            || !Uri.TryCreate(origin, UriKind.Absolute, out var parsed)
            || !IsApprovedAuthority(parsed)
            || parsed.AbsolutePath != "/"
            || parsed.Query.Length > 0
            || parsed.Fragment.Length > 0)
        {
            error = "The InfraIQ ARM ingress origin is not an approved origin.";
            return false;
        }

        originUri = new Uri(DevelopmentOrigin);
        error = null;
        return true;
    }

    /// <summary>
    /// Returns whether a completed request URI targets exactly the approved HTTPS authority.
    /// </summary>
    public static bool IsApprovedAuthority(Uri uri) =>
        uri.IsAbsoluteUri
        && uri.Scheme == Uri.UriSchemeHttps
        && uri.UserInfo.Length == 0
        && uri.IsDefaultPort
        && string.Equals(uri.Host, DevelopmentHost, StringComparison.OrdinalIgnoreCase)
        && string.Equals(uri.IdnHost, DevelopmentHost, StringComparison.OrdinalIgnoreCase);

    private static bool HasOnlySafeCharacters(string value)
    {
        foreach (var character in value)
        {
            if (char.IsWhiteSpace(character) || char.IsControl(character) || character is '%' or '\\' or '@')
            {
                return false;
            }
        }

        return true;
    }
}
