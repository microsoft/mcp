// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Adme.Models;
using Azure.Mcp.Tools.Adme.Models.Schema;

namespace Azure.Mcp.Tools.Adme.Services;

/// <summary>
/// Provides access to ADME schema definitions and descriptors.
/// </summary>
public interface ISchemaService
{
    /// <summary>
    /// Gets the JSON definition for a schema kind.
    /// </summary>
    Task<AdmeResponse<JsonElement>> GetSchemaAsync(
        string endpoint,
        string dataPartition,
        string kind,
        string? tenant,
        CancellationToken cancellationToken,
        string? authAppId = null);

    /// <summary>
    /// Lists schema descriptors matching the requested filters.
    /// </summary>
    Task<AdmeResponse<SchemaListResponse>> ListSchemasAsync(
        string endpoint,
        string dataPartition,
        string? tenant,
        string? authority,
        string? source,
        string? entityType,
        SchemaStatus? status,
        SchemaScope? scope,
        int? schemaVersionMajor,
        int? schemaVersionMinor,
        int? schemaVersionPatch,
        bool latestVersion,
        int? offset,
        int? limit,
        CancellationToken cancellationToken,
        string? authAppId = null);
}
