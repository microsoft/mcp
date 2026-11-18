// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
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

    /// <summary>
    /// Returns <c>true</c> when the request targets the root or the resource-specific protected resource metadata URL.
    /// </summary>
    public static bool IsMetadataRequest(HttpRequest request, McpHttpEndpointSettings settings)
    {
        if (!HttpMethods.IsGet(request.Method))
        {
            return false;
        }

        return request.Path == WellKnownPath
            || (settings.ResourcePath.Length > 0
                && request.Path == PathString.FromUriComponent(WellKnownPath + settings.ResourcePath));
    }

    /// <summary>
    /// Gets the protected resource identifier: the configured public resource, or one derived from the request.
    /// </summary>
    public static string GetResource(McpHttpEndpointSettings settings, string scheme, HostString host) =>
        settings.Resource ?? $"{scheme}://{host}{settings.EndpointPath}";

    /// <summary>
    /// Gets the protected resource metadata URL for the resource, following RFC 9728 section 3.1.
    /// </summary>
    public static string GetMetadataUrl(McpHttpEndpointSettings settings, string scheme, HostString host)
    {
        if (settings.Resource is null)
        {
            return $"{scheme}://{host}{WellKnownPath}{settings.EndpointPath}";
        }

        Uri resource = new(settings.Resource, UriKind.Absolute);
        return $"{resource.GetLeftPart(UriPartial.Authority)}{WellKnownPath}{settings.ResourcePath}";
    }

    /// <summary>
    /// Creates the protected resource metadata document.
    /// </summary>
    public static OAuthProtectedResourceMetadata CreateMetadata(
        McpHttpEndpointSettings settings,
        string resource,
        string? clientId,
        string? tenantId,
        string? instance)
    {
        string authorityInstance = instance?.TrimEnd('/') ?? DefaultInstance;

        return new OAuthProtectedResourceMetadata
        {
            Resource = resource,
            AuthorizationServers = settings.AuthorizationServers.Length > 0
                ? settings.AuthorizationServers
                : [$"{authorityInstance}/{tenantId}/v2.0"],

            // Only delegated permissions for user principal authorization are listed here.
            // Clients with users send these scopes to the identity platform to acquire an
            // access token. Service principals are expected to request the Entra-specific
            // `app-id/.default` scope and rely on app roles (Mcp.Tools.ReadWrite.All) instead.
            // Configured scopes must still resolve to the Mcp.Tools.ReadWrite delegated scope
            // on this server's app registration, because that is what the authorization policy requires.
            ScopesSupported = settings.ScopesSupported.Length > 0
                ? settings.ScopesSupported
                : [$"{clientId}/{DelegatedScope}"],
            BearerMethodsSupported = ["header"],
            ResourceDocumentation = ResourceDocumentation
        };
    }
}
