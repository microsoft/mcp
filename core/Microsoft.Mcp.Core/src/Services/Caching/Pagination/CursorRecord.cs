// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Services.Caching.Pagination;

/// <summary>
/// Server-side record stored in the cursor registry for each active cursor.
/// Maps the opaque public cursor ID to provider-native continuation state
/// and the validation context needed to enforce request and caller isolation.
/// </summary>
public sealed class CursorRecord
{
    /// <summary>
    /// Schema version for forward compatibility during rollout and upgrades.
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; init; } = 1;

    /// <summary>
    /// The opaque public cursor ID returned to the client (e.g., "cur_01JV7K6Q4M0R0H7S1EJ0M8YV7A").
    /// </summary>
    [JsonPropertyName("cursorId")]
    public required string CursorId { get; init; }

    /// <summary>
    /// The provider adapter family responsible for resuming this cursor (e.g., "arm", "cosmos", "kql").
    /// </summary>
    [JsonPropertyName("provider")]
    public required string Provider { get; init; }

    /// <summary>
    /// The logical MCP tool or operation name (e.g., "resourcegroup.list").
    /// </summary>
    [JsonPropertyName("operation")]
    public required string Operation { get; init; }

    /// <summary>
    /// Hash of the semantic request shape. Used to reject mismatched resume attempts.
    /// </summary>
    [JsonPropertyName("requestFingerprint")]
    public required string RequestFingerprint { get; init; }

    /// <summary>
    /// Caller security context binding. Used to prevent cross-user cursor reuse.
    /// </summary>
    [JsonPropertyName("callerBinding")]
    public required CallerBinding CallerBinding { get; init; }

    /// <summary>
    /// Provider-native continuation state (e.g., ARM nextLink, Cosmos continuation token).
    /// Opaque to everything outside the owning provider adapter.
    /// </summary>
    [JsonPropertyName("nativeState")]
    public required string NativeState { get; init; }

    /// <summary>
    /// Optional metadata about the resource scope, useful for resume validation
    /// (e.g., subscription, cloud, API version).
    /// </summary>
    [JsonPropertyName("resourceMetadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, string>? ResourceMetadata { get; init; }

    /// <summary>
    /// When the cursor record was created (UTC).
    /// </summary>
    [JsonPropertyName("createdAtUtc")]
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// When the cursor record expires (UTC). Enforced by the registry via cache TTL.
    /// </summary>
    [JsonPropertyName("expiresAtUtc")]
    public DateTimeOffset ExpiresAtUtc { get; init; }
}
