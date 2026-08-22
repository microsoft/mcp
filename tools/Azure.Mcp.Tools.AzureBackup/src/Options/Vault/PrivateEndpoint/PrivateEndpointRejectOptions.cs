// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Vault.PrivateEndpoint;

public sealed class PrivateEndpointRejectOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.PrivateEndpointName)]
    public required string PrivateEndpointName { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.PrivateEndpointDescription)]
    public string? Description { get; set; }
}
