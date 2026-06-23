// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Governance;

public sealed class GovernanceImmutabilityOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.ImmutabilityState)]
    public required string ImmutabilityState { get; set; }
}
