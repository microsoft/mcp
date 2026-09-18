// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public record NetAppFilesBackupPolicy(
    string Name,
    string Id,
    string Location,
    string? ProvisioningState,
    int? DailyBackupsToKeep,
    int? WeeklyBackupsToKeep,
    int? MonthlyBackupsToKeep,
    bool? IsEnabled);
