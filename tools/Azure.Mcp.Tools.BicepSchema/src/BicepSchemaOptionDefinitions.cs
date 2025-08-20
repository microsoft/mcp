// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.BicepSchema;

public static class BicepSchemaOptionDefinitions
{
    public const string ResourceType = "resource-type";

    public static readonly Option<string> ResourceTypeName = new(
        $"--{ResourceType}",
        "The name of the Bicep Resource Type and must be in the full Azure Resource Manager format '{ResourceProvider}/{ResourceType}'. " +
        "(e.g., 'Microsoft.KeyVault/vaults', 'Microsoft.Storage/storageAccounts', 'Microsoft.Compute/virtualMachines')(e.g., Microsoft.Storage/storageAccounts)."
    )
    {
        IsRequired = true
    };
}
