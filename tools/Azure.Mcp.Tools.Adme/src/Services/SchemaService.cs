// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Mcp.Tools.Adme.Models.Schema;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Adme.Services;

/// <summary>
/// Retrieves schema definitions and descriptors from ADME.
/// </summary>
public sealed class SchemaService(
    IAzureTokenCredentialProvider credentialProvider,
    IHttpClientFactory httpClientFactory) : ISchemaService
{
    private const string BasePath = "/api/schema-service/v1/schema";

    private readonly IAzureTokenCredentialProvider _credentialProvider = credentialProvider;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    /// <summary>
    /// Gets the JSON definition for a schema kind.
    /// </summary>
    public Task<JsonElement> GetSchemaAsync(
        string endpoint,
        string dataPartition,
        string kind,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        return AdmeServiceHelper.SendAsync(
            _credentialProvider,
            _httpClientFactory,
            endpoint,
            dataPartition,
            tenant,
            $"{BasePath}/{Uri.EscapeDataString(kind)}",
            AdmeJsonContext.Default.JsonElement,
            cancellationToken);
    }

    /// <summary>
    /// Lists schema descriptors matching the requested filters.
    /// </summary>
    public Task<SchemaListResponse> ListSchemasAsync(
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
        int offset,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = new List<KeyValuePair<string, string>>();
        AdmeServiceHelper.Add(query, "authority", authority);
        AdmeServiceHelper.Add(query, "source", source);
        AdmeServiceHelper.Add(query, "entityType", entityType);
        AdmeServiceHelper.Add(query, "status", status?.ToString());
        AdmeServiceHelper.Add(query, "scope", scope?.ToString());
        AdmeServiceHelper.Add(query, "schemaVersionMajor", AdmeServiceHelper.Format(schemaVersionMajor));
        AdmeServiceHelper.Add(query, "schemaVersionMinor", AdmeServiceHelper.Format(schemaVersionMinor));
        AdmeServiceHelper.Add(query, "schemaVersionPatch", AdmeServiceHelper.Format(schemaVersionPatch));
        if (latestVersion)
        {
            AdmeServiceHelper.Add(query, "latestVersion", "true");
        }
        AdmeServiceHelper.Add(query, "offset", offset.ToString(CultureInfo.InvariantCulture));
        AdmeServiceHelper.Add(query, "limit", limit.ToString(CultureInfo.InvariantCulture));

        return AdmeServiceHelper.SendAsync(
            _credentialProvider,
            _httpClientFactory,
            endpoint,
            dataPartition,
            tenant,
            AdmeServiceHelper.AppendQuery(BasePath, query),
            AdmeJsonContext.Default.SchemaListResponse,
            cancellationToken);
    }
}
