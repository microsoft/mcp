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
    /// </summary>
    [JsonPropertyName("clientId")]
    public required string ClientId { get; set; }

    /// <summary>
    /// The expected audience (resource identifier) for token validation.
    /// </summary>
    [JsonPropertyName("audience")]
    public required string Audience { get; set; }

    /// <summary>
    /// The client secret for the Azure AD application.
    /// Required for On-Behalf-Of flow, optional otherwise.
    /// </summary>
    [JsonPropertyName("clientSecret")]
    public string? ClientSecret { get; set; }
}
