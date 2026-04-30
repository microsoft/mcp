// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record OperationResult(
    string Status,
    string? JobId,
    string? Message);
