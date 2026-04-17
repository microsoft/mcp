// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Options.Governance;

public class GovernanceImmutabilityOptions : BaseAzureBackupOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.ImmutabilityStateName)]
    public string? ImmutabilityState { get; set; }
}
