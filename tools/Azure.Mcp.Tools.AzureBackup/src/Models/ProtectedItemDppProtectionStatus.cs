// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ProtectedItemDppProtectionStatus(
    string? Status,
    IReadOnlyList<ProtectedItemDppError>? ErrorDetails,
    IReadOnlyList<ProtectedItemDppError>? ProtectionStatusErrorDetails);
