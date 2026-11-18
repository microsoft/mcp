// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;

namespace Microsoft.Mcp.Core.Areas.Server.Models;

/// <summary>
/// Optional settings for the remote HTTP MCP endpoint and its OAuth 2.0 Protected Resource Metadata (RFC 9728).
/// All settings are optional. When none are configured, the server keeps its default behavior: the MCP endpoint
/// is mapped at the root path and the protected resource is derived from the incoming request.
/// </summary>
/// <remarks>
/// Settings are read from the <c>McpServer</c> configuration section, for example through environment variables:
/// <list type="bullet">
/// <item><description><c>McpServer__EndpointPath</c>: path at which the MCP endpoint is mapped, such as <c>/mcp</c>.</description></item>
/// <item><description><c>McpServer__ProtectedResource__Resource</c>: public, canonical URI of the MCP endpoint advertised as the protected resource.</description></item>
/// <item><description><c>McpServer__ProtectedResource__AuthorizationServers__0</c>: authorization server issuer identifier(s) to advertise.</description></item>
/// <item><description><c>McpServer__ProtectedResource__ScopesSupported__0</c>: delegated scope(s) to advertise.</description></item>
/// </list>
/// These settings only affect what is advertised to clients and where the endpoint is mapped. They do not change how
/// incoming access tokens are validated, which remains governed by the <c>AzureAd</c> configuration section.
/// </remarks>
internal sealed class McpHttpEndpointSettings(
    string endpointPath,
    string? resource,
    string[] authorizationServers,
    string[] scopesSupported)
{
    public const string SectionName = "McpServer";
    public const string EndpointPathKey = "EndpointPath";
    public const string ProtectedResourceSectionName = "ProtectedResource";
    public const string ResourceKey = "Resource";
    public const string AuthorizationServersKey = "AuthorizationServers";
    public const string ScopesSupportedKey = "ScopesSupported";

    private static readonly string[] s_reservedPathPrefixes = ["/.well-known", "/health"];

    public static McpHttpEndpointSettings Default { get; } = new(string.Empty, null, [], []);

    /// <summary>
    /// The normalized path at which the MCP endpoint is mapped. Empty means the root path.
    /// </summary>
    public string EndpointPath { get; } = endpointPath;

    /// <summary>
    /// The configured public protected resource URI, exactly as configured, or <c>null</c> to derive it from the request.
    /// </summary>
    public string? Resource { get; } = resource;

    /// <summary>
    /// Configured authorization server issuer identifiers. Empty means use the default derived from <c>AzureAd</c>.
    /// </summary>
    public string[] AuthorizationServers { get; } = authorizationServers;

    /// <summary>
    /// Configured scopes to advertise. Empty means use the default derived from <c>AzureAd:ClientId</c>.
    /// </summary>
    public string[] ScopesSupported { get; } = scopesSupported;

    /// <summary>
    /// The path component used for the resource-specific protected resource metadata URL (RFC 9728 section 3.1),
    /// or an empty string when the resource has no path.
    /// </summary>
    public string ResourcePath { get; } = resource is null ? endpointPath : GetResourcePath(new Uri(resource, UriKind.Absolute));

    /// <summary>
    /// Reads and validates the settings from configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when a configured value is invalid.</exception>
    public static McpHttpEndpointSettings FromConfiguration(IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection(SectionName);
        IConfigurationSection protectedResourceSection = section.GetSection(ProtectedResourceSectionName);

        string endpointPath = NormalizeEndpointPath(section[EndpointPathKey]);
        string? resource = ValidateResource(protectedResourceSection[ResourceKey]);
        string[] authorizationServers = ValidateAuthorizationServers(ReadValues(protectedResourceSection.GetSection(AuthorizationServersKey)));
        string[] scopesSupported = ValidateScopes(ReadValues(protectedResourceSection.GetSection(ScopesSupportedKey)));

        return new(endpointPath, resource, authorizationServers, scopesSupported);
    }

    internal static string NormalizeEndpointPath(string? value)
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
                $"{SectionName}:{EndpointPathKey} must be an absolute URL path such as '/mcp' without query, fragment, route parameters, or dot segments.");
        }

        path = path.TrimEnd('/');
        foreach (string reserved in s_reservedPathPrefixes)
        {
            if (path.Equals(reserved, StringComparison.OrdinalIgnoreCase)
                || path.StartsWith(reserved + "/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"{SectionName}:{EndpointPathKey} must not use the reserved path '{reserved}'.");
            }
        }

        return path;
    }

    private static string? ValidateResource(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string resource = value.Trim();
        const string key = $"{SectionName}:{ProtectedResourceSectionName}:{ResourceKey}";
        if (!Uri.TryCreate(resource, UriKind.Absolute, out Uri? uri)
            || !IsAllowedScheme(uri)
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment)
            || !string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new InvalidOperationException(
                $"{key} must be an absolute 'https' URI without query, fragment, or user info ('http' is only allowed for loopback hosts).");
        }

        return resource;
    }

    private static string[] ValidateAuthorizationServers(string[] values)
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
                    $"Each {SectionName}:{ProtectedResourceSectionName}:{AuthorizationServersKey} entry must be an absolute 'https' issuer identifier without query, fragment, or user info.");
            }
        }

        return values;
    }

    private static string[] ValidateScopes(string[] values)
    {
        foreach (string value in values)
        {
            if (value.Any(char.IsWhiteSpace) || value.Contains('"'))
            {
                throw new InvalidOperationException(
                    $"Each {SectionName}:{ProtectedResourceSectionName}:{ScopesSupportedKey} entry must be a single scope without whitespace or quotes.");
            }
        }

        return values;
    }

    private static string[] ReadValues(IConfigurationSection section)
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

    private static bool IsAllowedScheme(Uri uri) =>
        uri.Scheme == Uri.UriSchemeHttps || (uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback);

    private static string GetResourcePath(Uri uri)
    {
        string path = uri.AbsolutePath;
        return path == "/" ? string.Empty : path;
    }
}
