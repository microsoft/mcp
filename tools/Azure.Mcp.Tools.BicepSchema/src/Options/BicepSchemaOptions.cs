// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.BicepSchema.Options;

public sealed class BicepSchemaOptions
{
    [Option(Description = "The name of the Bicep Resource Type and must be in the full Azure Resource Manager format '{ResourceProvider}/{ResourceType}'. " +
        "(e.g., 'Microsoft.KeyVault/vaults', 'Microsoft.Storage/storageAccounts', 'Microsoft.Compute/virtualMachines')(e.g., Microsoft.Storage/storageAccounts).")]
    public required string ResourceType { get; set; }
}
