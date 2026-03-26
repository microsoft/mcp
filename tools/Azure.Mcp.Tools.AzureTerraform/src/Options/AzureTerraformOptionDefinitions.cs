// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public static class AzureTerraformOptionDefinitions
{
    // AzureRM option names
    public const string ResourceTypeName = "resource-type";
    public const string DocTypeName = "doc-type";
    public const string ArgumentNameOption = "argument";
    public const string AttributeNameOption = "attribute";

    // AzAPI option names
    public const string AzApiResourceTypeName = "resource-type";
    public const string ApiVersionName = "api-version";

    // AVM option names
    public const string AvmModuleNameOption = "module-name";
    public const string AvmModuleVersionOption = "module-version";

    public static readonly Option<string> ResourceType = new(
        $"--{ResourceTypeName}"
    )
    {
        Description = "The AzureRM Terraform resource type name (e.g., azurerm_resource_group, azurerm_storage_account). The 'azurerm_' prefix is optional.",
        Required = true
    };

    public static readonly Option<string> DocType = new(
        $"--{DocTypeName}"
    )
    {
        Description = "The documentation type to retrieve. Options: 'resource' (default), 'data-source'.",
        Required = false
    };

    public static readonly Option<string> ArgumentName = new(
        $"--{ArgumentNameOption}"
    )
    {
        Description = "Filter results to a specific argument name.",
        Required = false
    };

    public static readonly Option<string> AttributeName = new(
        $"--{AttributeNameOption}"
    )
    {
        Description = "Filter results to a specific attribute name.",
        Required = false
    };

    public static readonly Option<string> AzApiResourceType = new(
        $"--{AzApiResourceTypeName}"
    )
    {
        Description = "The Azure resource type in ARM format (e.g., Microsoft.Compute/virtualMachines, Microsoft.Storage/storageAccounts).",
        Required = true
    };

    public static readonly Option<string> ApiVersion = new(
        $"--{ApiVersionName}"
    )
    {
        Description = "The API version to use for the resource schema. If omitted, the latest stable version is used.",
        Required = false
    };

    public static readonly Option<string> AvmModuleName = new(
        $"--{AvmModuleNameOption}"
    )
    {
        Description = "The name of the Azure Verified Module (e.g., avm-res-storage-storageaccount).",
        Required = true
    };

    public static readonly Option<string> AvmModuleVersion = new(
        $"--{AvmModuleVersionOption}"
    )
    {
        Description = "The version of the Azure Verified Module (e.g., 0.4.0).",
        Required = true
    };
}
