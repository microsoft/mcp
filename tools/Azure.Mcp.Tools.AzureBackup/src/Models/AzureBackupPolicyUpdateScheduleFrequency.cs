// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>Schedule frequencies supported when updating an RSV backup policy.</summary>
public enum AzureBackupPolicyUpdateScheduleFrequency
{
    /// <summary>Run the backup every day.</summary>
    Daily,

    /// <summary>Run the backup on selected days each week.</summary>
    Weekly
}
