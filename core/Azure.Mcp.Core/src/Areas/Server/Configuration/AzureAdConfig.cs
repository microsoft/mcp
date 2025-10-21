// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Configuration;

/// <summary>
/// Azure Active Directory (Entra ID) configuration for authentication.
/// </summary>
public class AzureAdConfig
{
    /// <summary>
    /// The Azure AD instance URL (e.g., "https://login.microsoftonline.com/").
    /// </summary>
    [JsonPropertyName("instance")]
    public required string Instance { get; set; }

    /// <summary>
    /// The Azure AD tenant ID (GUID).
    /// </summary>
    [JsonPropertyName("tenantId")]
    public required string TenantId { get; set; }

    /// <summary>
    /// The Azure AD application (client) ID.
    /// Required for OBO flow, optional for JWT validation only.
    /// </summary>
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }

    /// <summary>
    /// The expected audience (resource identifier) for token validation.
    /// </summary>
    [JsonPropertyName("audience")]
    public required string Audience { get; set; }

    /// <summary>
    /// Optional array of required app roles for authorization.
    /// If specified, users must have at least one of these roles to access MCP endpoints.
    /// If not specified or empty, only authentication is required (no role validation).
    /// </summary>
    [JsonPropertyName("requiredRoles")]
    public string[]? RequiredRoles { get; set; }
}
