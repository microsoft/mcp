// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Governance;

public sealed class GovernanceImmutabilityOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.ImmutabilityState)]
    public required AzureBackupImmutabilityState ImmutabilityState { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.ImmutabilityType)]
    public required AzureBackupImmutabilityType ImmutabilityType { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.ImmutabilityDurationDays)]
    public int? ImmutabilityDurationDays { get; set; }
}
