// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record RecoveryPointInfo(
    string? Id,
    string Name,
    string VaultType,
    DateTimeOffset? RecoveryPointTime,
    string? RecoveryPointType);
