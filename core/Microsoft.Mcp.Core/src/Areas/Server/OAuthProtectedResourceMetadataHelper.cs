// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Abstractions;
using Microsoft.Mcp.Core.Areas.Server.Options;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Authentication;

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
    /// Validates and normalizes the configured MCP endpoint path.
    /// </summary>
    /// <param name="value">The configured endpoint path.</param>
    /// <returns>The normalized endpoint path, or an empty string when no path is configured.</returns>
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
    /// <param name="value">The configured protected resource identifier.</param>
    /// <returns>The validated resource identifier, or <see langword="null"/> when it is not configured.</returns>
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
    /// Validates configured authorization server issuer identifiers.
    /// </summary>
    /// <param name="values">The configured issuer identifiers.</param>
    /// <returns>The validated issuer identifiers.</returns>
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
    /// <param name="values">The configured OAuth scopes.</param>
    /// <returns>The validated scopes.</returns>
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
    /// Validates the configured HTTP endpoint options.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <returns><see langword="true"/> when the options are valid.</returns>
    public static bool ValidateOptions(McpHttpEndpointOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _ = NormalizeEndpointPath(options.EndpointPath);

        ProtectedResourceOptions protectedResource = options.ProtectedResource
            ?? throw new InvalidOperationException($"{ConfigurationSectionName}:{ProtectedResourceSectionName} must be an object.");
        _ = ValidateResource(protectedResource.Resource);
        _ = ValidateAuthorizationServers(protectedResource.AuthorizationServers ?? []);
        _ = ValidateScopes(protectedResource.ScopesSupported ?? []);
        return true;
    }

    /// <summary>
    /// Configures the MCP SDK authentication handler with protected resource metadata.
    /// </summary>
    /// <param name="authenticationOptions">The MCP SDK authentication options to configure.</param>
    /// <param name="endpointOptions">The HTTP endpoint configuration.</param>
    /// <param name="azureAdOptions">The Microsoft Entra application configuration.</param>
    public static void ConfigureAuthenticationOptions(
        McpAuthenticationOptions authenticationOptions,
        McpHttpEndpointOptions endpointOptions,
        MicrosoftIdentityApplicationOptions azureAdOptions)
    {
        ProtectedResourceOptions protectedResource = endpointOptions.ProtectedResource;
        authenticationOptions.ResourceMetadata = CreateMetadata(
            protectedResource.AuthorizationServers ?? [],
            protectedResource.ScopesSupported ?? [],
            protectedResource.Resource,
            azureAdOptions.ClientId,
            azureAdOptions.TenantId,
            azureAdOptions.Instance);

        if (GetConfiguredMetadataUri(endpointOptions) is Uri metadataUri)
        {
            authenticationOptions.ResourceMetadataUri = metadataUri;
        }
    }

    /// <summary>
    /// Gets the absolute metadata URL for an explicitly configured protected resource.
    /// </summary>
    /// <param name="endpointOptions">The HTTP endpoint configuration.</param>
    /// <returns>The configured metadata URL, or <see langword="null"/> when no resource is configured.</returns>
    public static Uri? GetConfiguredMetadataUri(McpHttpEndpointOptions endpointOptions)
    {
        if (ValidateResource(endpointOptions.ProtectedResource?.Resource) is not string resource)
        {
            return null;
        }

        string endpointPath = NormalizeEndpointPath(endpointOptions.EndpointPath);
        return new Uri(GetConfiguredMetadataUrl(resource, GetResourcePath(resource, endpointPath)));
    }

    /// <summary>
    /// Applies the public origin of the configured metadata URL to a request for that metadata document.
    /// </summary>
    /// <remarks>
    /// The MCP SDK only serves an absolute configured metadata URL when the request's scheme and host match it.
    /// When TLS terminates in front of the server, the server can receive a different internal scheme and host.
    /// The metadata document is static and anonymous, so only requests for its configured path are updated.
    /// </remarks>
    /// <param name="request">The incoming HTTP request.</param>
    /// <param name="metadataUri">The configured absolute metadata URL.</param>
    /// <returns><see langword="true"/> when the request targets the configured metadata path and was updated.</returns>
    public static bool ApplyConfiguredMetadataOrigin(HttpRequest request, Uri metadataUri)
    {
        if (!string.Equals(request.Path.Value, metadataUri.AbsolutePath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        request.Scheme = metadataUri.Scheme;
        request.Host = HostString.FromUriComponent(metadataUri);
        return true;
    }

    /// <summary>
    /// Gets the metadata path for a protected resource, removing its terminating slash for discovery.
    /// </summary>
    /// <param name="resource">The protected resource identifier.</param>
    /// <param name="endpointPath">The configured MCP endpoint path.</param>
    /// <returns>The path appended to the well-known metadata path.</returns>
    public static string GetResourcePath(string? resource, string endpointPath)
    {
        if (resource is null)
        {
            return endpointPath;
        }

        string path = new Uri(resource, UriKind.Absolute).AbsolutePath;
        return path == "/" ? string.Empty : path.TrimEnd('/');
    }

    /// <summary>
    /// Creates the protected resource metadata document.
    /// </summary>
    /// <param name="authorizationServers">Configured authorization server issuer identifiers.</param>
    /// <param name="scopesSupported">Configured scopes to advertise.</param>
    /// <param name="resource">The configured protected resource identifier, if any.</param>
    /// <param name="clientId">The protected resource application's client ID.</param>
    /// <param name="tenantId">The tenant ID used by the protected resource.</param>
    /// <param name="instance">The identity platform instance used by the protected resource.</param>
    /// <returns>The metadata document configured for the MCP SDK handler.</returns>
    public static ProtectedResourceMetadata CreateMetadata(
        string[] authorizationServers,
        string[] scopesSupported,
        string? resource,
        string? clientId,
        string? tenantId,
        string? instance)
    {
        string authorityInstance = instance?.TrimEnd('/') ?? DefaultInstance;

        return new ProtectedResourceMetadata
        {
            Resource = resource,
            AuthorizationServers = authorizationServers.Length > 0
                ? [.. authorizationServers]
                : [$"{authorityInstance}/{tenantId}/v2.0"],

            // Only delegated permissions for user principal authorization are listed here.
            // Clients with users send these scopes to the identity platform to acquire an
            // access token. Service principals are expected to request the Entra-specific
            // `app-id/.default` scope and rely on app roles (Mcp.Tools.ReadWrite.All) instead.
            // Configured scopes must still resolve to the Mcp.Tools.ReadWrite delegated scope
            // on this server's app registration, because that is what the authorization policy requires.
            ScopesSupported = scopesSupported.Length > 0
                ? [.. scopesSupported]
                : [$"{clientId}/{DelegatedScope}"],
            BearerMethodsSupported = ["header"],
            ResourceDocumentation = ResourceDocumentation
        };
    }

    private static bool IsAllowedScheme(Uri uri) =>
        uri.Scheme == Uri.UriSchemeHttps || (uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback);

    private static string GetConfiguredMetadataUrl(string resource, string resourcePath)
    {
        Uri resourceUri = new(resource, UriKind.Absolute);
        return $"{resourceUri.GetLeftPart(UriPartial.Authority)}{WellKnownPath}{resourcePath}";
    }
}
