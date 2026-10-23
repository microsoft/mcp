// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Vault.PrivateEndpoint;

public sealed class PrivateEndpointApproveRejectOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.PrivateEndpointName)]
    public required string PrivateEndpointName { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.PrivateEndpointAction)]
    public required PrivateEndpointConnectionAction Action { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.PrivateEndpointDescription)]
    public string? Description { get; set; }
}
