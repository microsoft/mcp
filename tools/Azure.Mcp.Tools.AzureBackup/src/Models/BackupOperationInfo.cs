// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record BackupOperationInfo(
    string OperationId,
    string ResourceId,
    string Scope,
    string VaultType,
    string Status,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    BackupOperationError? Error,
    string? JobId,
    IReadOnlyList<string> JobIds,
    IReadOnlyDictionary<string, string> FailedJobsError);
