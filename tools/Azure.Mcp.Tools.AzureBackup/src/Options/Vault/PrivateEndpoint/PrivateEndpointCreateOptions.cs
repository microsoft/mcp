// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Vault.PrivateEndpoint;

public sealed class PrivateEndpointCreateOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.PrivateEndpointName)]
    public required string PrivateEndpointName { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.PrivateEndpointVnetSubnetId)]
    public required string VnetSubnetId { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.PrivateEndpointGroupId)]
    public string? GroupId { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.PrivateEndpointLocation)]
    public string? Location { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.PrivateEndpointAutoApprove)]
    public bool? AutoApprove { get; set; }
}
