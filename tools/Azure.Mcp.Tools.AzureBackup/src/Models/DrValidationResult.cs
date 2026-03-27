// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record DrValidationResult(
    string? VaultName,
    bool CrrEnabled,
    string? StorageRedundancy,
    string? PrimaryRegion,
    string? SecondaryRegion,
    int TotalProtectedItems,
    int ItemsWithSecondaryRp,
    string? Message);
