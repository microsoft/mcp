// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Governance;

public sealed class GovernanceSoftDeleteOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.SoftDelete)]
    public required AzureBackupSoftDeleteState SoftDelete { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.SoftDeleteRetentionDays)]
    public required int SoftDeleteRetentionDays { get; set; }
}