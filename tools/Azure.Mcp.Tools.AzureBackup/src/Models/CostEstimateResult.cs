// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record CostEstimateResult(
    string? VaultName,
    string? VaultType,
    double? EstimatedMonthlyCostUsd,
    int? ProtectedItemCount,
    double? StorageUsedGb,
    string? Message);
