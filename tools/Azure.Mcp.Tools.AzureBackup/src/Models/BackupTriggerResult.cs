// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record BackupTriggerResult(
    string Status,
    string? JobId,
    string? Message);
