// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Services.Caching.Pagination;

/// <summary>
/// Represents the security context under which a cursor was created.
/// Used to prevent cross-user cursor reuse in remote HTTP mode.
/// </summary>
public sealed class CallerBinding
{
    /// <summary>
    /// The outgoing authentication strategy active when the cursor was created
    /// (e.g., "obo" for On-Behalf-Of, "hostIdentity" for hosting environment identity).
    /// </summary>
    [JsonPropertyName("mode")]
    public required string Mode { get; init; }

    /// <summary>
    /// The Entra tenant ID when available, otherwise null for stdio / unauthenticated flows.
    /// </summary>
    [JsonPropertyName("tenantId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TenantId { get; init; }

    /// <summary>
    /// A SHA-256 hash of the principal or service identity, stored as a hex string.
    /// Null for stdio / unauthenticated flows.
    /// </summary>
    [JsonPropertyName("principalIdHash")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PrincipalIdHash { get; init; }
}
