// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Mcp.Core.Areas.Server.Models;

namespace Microsoft.Mcp.Core.Areas.Server;

/// <summary>
/// Builds OAuth 2.0 Protected Resource Metadata (RFC 9728) values for the remote HTTP MCP endpoint.
/// </summary>
internal static class OAuthProtectedResourceMetadataHelper
{
    public const string WellKnownPath = "/.well-known/oauth-protected-resource";
    public const string DelegatedScope = "Mcp.Tools.ReadWrite";
    public const string DefaultInstance = "https://login.microsoftonline.com";
    public const string ResourceDocumentation = "https://github.com/Microsoft/mcp";

    private const string ConfigurationSectionName = "McpServer";
    private const string ProtectedResourceSectionName = "ProtectedResource";
    private static readonly string[] s_reservedPathPrefixes = ["/.well-known", "/health"];

    /// <summary>
    /// Returns <c>true</c> when the request targets the root or the resource-specific protected resource metadata URL.
    /// </summary>
    public static bool IsMetadataRequest(HttpRequest request, string resourcePath)
    {
        if (!HttpMethods.IsGet(request.Method))
        {
            return false;
        }

        return request.Path == WellKnownPath
            || (resourcePath.Length > 0
                && request.Path == PathString.FromUriComponent(WellKnownPath + resourcePath));
    }

    /// <summary>
    /// Validates and normalizes the configured MCP endpoint path.
    /// </summary>
    public static string NormalizeEndpointPath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string path = value.Trim();
        if (!path.StartsWith('/')
            || path.IndexOfAny(['?', '#', '{', '}', '*', '\\']) >= 0
            || path.Contains("//", StringComparison.Ordinal)
            || path.Split('/').Any(segment => segment is "." or ".."))
        {
            throw new InvalidOperationException(
                $"{ConfigurationSectionName}:EndpointPath must be an absolute URL path such as '/mcp' without query, fragment, route parameters, or dot segments.");
        }

        path = path.TrimEnd('/');
        foreach (string reserved in s_reservedPathPrefixes)
        {
            if (path.Equals(reserved, StringComparison.OrdinalIgnoreCase)
                || path.StartsWith(reserved + "/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"{ConfigurationSectionName}:EndpointPath must not use the reserved path '{reserved}'.");
            }
        }

        return path;
    }

    /// <summary>
    /// Validates the configured public resource identifier.
    /// </summary>
    public static string? ValidateResource(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string resource = value.Trim();
        if (!Uri.TryCreate(resource, UriKind.Absolute, out Uri? uri)
            || !IsAllowedScheme(uri)
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment)
            || !string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new InvalidOperationException(
                $"{ConfigurationSectionName}:{ProtectedResourceSectionName}:Resource must be an absolute 'https' URI without query, fragment, or user info ('http' is only allowed for loopback hosts).");
        }

        return resource;
    }

    /// <summary>
    /// Gets the configured values in a scalar or indexed configuration section.
    /// </summary>
    public static string[] ReadValues(IConfigurationSection section)
    {
        List<string> values = [];
        if (!string.IsNullOrWhiteSpace(section.Value))
        {
            values.Add(section.Value.Trim());
        }

        foreach (IConfigurationSection child in section.GetChildren())
        {
            if (!string.IsNullOrWhiteSpace(child.Value))
            {
                values.Add(child.Value.Trim());
            }
        }

        return [.. values];
    }

    /// <summary>
    /// Validates configured authorization server issuer identifiers.
    /// </summary>
    public static string[] ValidateAuthorizationServers(string[] values)
    {
        foreach (string value in values)
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)
                || uri.Scheme != Uri.UriSchemeHttps
                || !string.IsNullOrEmpty(uri.Query)
                || !string.IsNullOrEmpty(uri.Fragment)
                || !string.IsNullOrEmpty(uri.UserInfo))
            {
                throw new InvalidOperationException(
                    $"Each {ConfigurationSectionName}:{ProtectedResourceSectionName}:AuthorizationServers entry must be an absolute 'https' issuer identifier without query, fragment, or user info.");
            }
        }

        return values;
    }

    /// <summary>
    /// Validates configured OAuth scopes.
    /// </summary>
    public static string[] ValidateScopes(string[] values)
    {
        foreach (string value in values)
        {
            if (value.Any(char.IsWhiteSpace) || value.Contains('"'))
            {
                throw new InvalidOperationException(
                    $"Each {ConfigurationSectionName}:{ProtectedResourceSectionName}:ScopesSupported entry must be a single scope without whitespace or quotes.");
            }
        }

        return values;
    }

    /// <summary>
    /// Gets the resource path used by RFC 9728 resource-specific metadata discovery.
    /// </summary>
    public static string GetResourcePath(string? resource, string endpointPath) =>
        resource is null ? endpointPath : GetResourcePath(new Uri(resource, UriKind.Absolute));

    /// <summary>
    /// Gets the protected resource identifier: the configured public resource, or one derived from the request.
    /// </summary>
    public static string GetResource(string? resource, string endpointPath, string scheme, HostString host) =>
        resource ?? $"{scheme}://{host}{endpointPath}";

    /// <summary>
    /// Gets the protected resource metadata URL for the resource, following RFC 9728 section 3.1.
    /// </summary>
    public static string GetMetadataUrl(string? resource, string resourcePath, string scheme, HostString host)
    {
        if (resource is null)
        {
            return $"{scheme}://{host}{WellKnownPath}{resourcePath}";
        }

        Uri resourceUri = new(resource, UriKind.Absolute);
        return $"{resourceUri.GetLeftPart(UriPartial.Authority)}{WellKnownPath}{resourcePath}";
    }

    /// <summary>
    /// Creates the protected resource metadata document.
    /// </summary>
    public static OAuthProtectedResourceMetadata CreateMetadata(
        string[] authorizationServers,
        string[] scopesSupported,
        string resource,
        string? clientId,
        string? tenantId,
        string? instance)
    {
        string authorityInstance = instance?.TrimEnd('/') ?? DefaultInstance;

        return new OAuthProtectedResourceMetadata
        {
            Resource = resource,
            AuthorizationServers = authorizationServers.Length > 0
                ? authorizationServers
                : [$"{authorityInstance}/{tenantId}/v2.0"],

            // Only delegated permissions for user principal authorization are listed here.
            // Clients with users send these scopes to the identity platform to acquire an
            // access token. Service principals are expected to request the Entra-specific
            // `app-id/.default` scope and rely on app roles (Mcp.Tools.ReadWrite.All) instead.
            // Configured scopes must still resolve to the Mcp.Tools.ReadWrite delegated scope
            // on this server's app registration, because that is what the authorization policy requires.
            ScopesSupported = scopesSupported.Length > 0
                ? scopesSupported
                : [$"{clientId}/{DelegatedScope}"],
            BearerMethodsSupported = ["header"],
            ResourceDocumentation = ResourceDocumentation
        };
    }

    private static bool IsAllowedScheme(Uri uri) =>
        uri.Scheme == Uri.UriSchemeHttps || (uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback);

    private static string GetResourcePath(Uri uri)
    {
        string path = uri.AbsolutePath;
        return path == "/" ? string.Empty : path;
    }
}
