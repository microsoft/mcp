// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ProtectResult(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("protectedItemName")] string? ProtectedItemName,
    [property: JsonPropertyName("jobId")] string? JobId,
    [property: JsonPropertyName("message")] string? Message);
