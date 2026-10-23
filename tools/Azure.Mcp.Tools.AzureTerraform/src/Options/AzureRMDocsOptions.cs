// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public sealed class AzureRMDocsOptions
{
    [Option(Description = "The AzureRM Terraform resource type name (e.g., azurerm_resource_group, azurerm_storage_account). The 'azurerm_' prefix is optional.")]
    public required string ResourceType { get; set; }

    [Option(Description = "The documentation type to retrieve. Options: 'resource' (default), 'data-source'.")]
    public string? DocType { get; set; }

    [Option(Description = "Filter results to a specific argument name.")]
    public string? Argument { get; set; }

    [Option(Description = "Filter results to a specific attribute name.")]
    public string? Attribute { get; set; }
}
