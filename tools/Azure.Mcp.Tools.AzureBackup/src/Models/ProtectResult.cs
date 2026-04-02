// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ProtectResult(
    string Status,
    string? ProtectedItemName,
    string? JobId,
    string? Message);
