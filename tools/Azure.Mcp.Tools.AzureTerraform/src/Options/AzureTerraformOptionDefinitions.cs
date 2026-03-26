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
}
