// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Models;

public record SqlDatabaseExportResult(
    [property: JsonPropertyName("operationId")] string? OperationId,
    [property: JsonPropertyName("requestId")] string? RequestId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("queuedTime")] DateTimeOffset? QueuedTime,
    [property: JsonPropertyName("lastModifiedTime")] DateTimeOffset? LastModifiedTime,
    [property: JsonPropertyName("serverName")] string? ServerName,
    [property: JsonPropertyName("databaseName")] string? DatabaseName,
    [property: JsonPropertyName("storageUri")] string? StorageUri,
    [property: JsonPropertyName("message")] string? Message
);
