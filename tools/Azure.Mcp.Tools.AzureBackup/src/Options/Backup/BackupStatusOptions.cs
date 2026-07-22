// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Backup;

public sealed class BackupStatusOptions : ISubscriptionOption
{
    [Option(Description = AzureBackupOptionDefinitions.DatasourceId)]
    public required string DatasourceId { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.LocationName)]
    public required string Location { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
