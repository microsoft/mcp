// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Governance;

public sealed class GovernanceSoftDeleteOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.SoftDelete)]
    public required string SoftDelete { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.SoftDeleteRetentionDays)]
    public string? SoftDeleteRetentionDays { get; set; }
}
