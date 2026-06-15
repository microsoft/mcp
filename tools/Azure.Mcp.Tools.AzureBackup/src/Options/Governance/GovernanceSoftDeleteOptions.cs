// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Governance;

public class GovernanceSoftDeleteOptions : BaseAzureBackupOptions
{
    [Option(AzureBackupOptionDefinitions.SoftDelete)]
    public required string SoftDelete { get; set; }

    [Option(AzureBackupOptionDefinitions.SoftDeleteRetentionDays)]
    public string? SoftDeleteRetentionDays { get; set; }
}
