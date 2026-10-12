// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Azure.Mcp.Tools.InfraIq.Configuration;

/// <summary>
/// Exact-destination rules for the InfraIQ ARM ingress. Only the global Azure public-cloud ARM endpoint
/// (<see cref="DefaultOrigin"/>) and the East US 2 EUAP Canary ARM endpoint (<see cref="CanaryOrigin"/>) are approved.
/// </summary>
internal static class InfraIqArmIngress
{
    public const string ConfigurationKey = "InfraIq:ArmIngressOrigin";
    public const string DefaultOrigin = "https://" + DefaultHost;
    public const string CanaryOrigin = "https://" + CanaryHost;
    public const string MissingOriginMessage =
        "The InfraIQ ARM ingress origin is not configured. Set 'InfraIq:ArmIngressOrigin' to an approved origin.";

    private const string DefaultHost = "management.azure.com";
    private const string CanaryHost = "eastus2euap.management.azure.com";

    /// <summary>
    /// Returns <see langword="null"/> when the value is absent or blank, which selects automatic routing: global ARM
    /// first, with Canary fallback while the action is unavailable there. Non-blank values are returned unchanged so
    /// unsafe values still fail validation and an approved value pins that single origin.
    /// </summary>
    public static string? NormalizeConfiguredOrigin(string? configuredOrigin) =>
        string.IsNullOrWhiteSpace(configuredOrigin) ? null : configuredOrigin;

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
            || !TryGetApprovedHost(parsed, out var approvedHost)
            || parsed.AbsolutePath != "/"
            || parsed.Query.Length > 0
            || parsed.Fragment.Length > 0)
        {
            error = "The InfraIQ ARM ingress origin is not an approved origin.";
            return false;
        }

        originUri = new Uri("https://" + approvedHost);
        error = null;
        return true;
    }

    /// <summary>
    /// Returns whether a completed request URI targets exactly an approved HTTPS authority.
    /// </summary>
    public static bool IsApprovedAuthority(Uri uri) => TryGetApprovedHost(uri, out _);

    private static bool TryGetApprovedHost(Uri uri, [NotNullWhen(true)] out string? approvedHost)
    {
        approvedHost = null;
        if (!uri.IsAbsoluteUri
            || uri.Scheme != Uri.UriSchemeHttps
            || uri.UserInfo.Length != 0
            || !uri.IsDefaultPort)
        {
            return false;
        }

        approvedHost = IsHost(uri, DefaultHost) ? DefaultHost : IsHost(uri, CanaryHost) ? CanaryHost : null;
        return approvedHost is not null;
    }

    private static bool IsHost(Uri uri, string host) =>
        string.Equals(uri.Host, host, StringComparison.OrdinalIgnoreCase)
        && string.Equals(uri.IdnHost, host, StringComparison.OrdinalIgnoreCase);

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
