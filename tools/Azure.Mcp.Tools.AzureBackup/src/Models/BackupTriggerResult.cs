// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record BackupTriggerResult(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("jobId")] string? JobId,
    [property: JsonPropertyName("message")] string? Message);
