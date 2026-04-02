// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record RestoreTriggerResult(
    string Status,
    string? JobId,
    string? Message);
