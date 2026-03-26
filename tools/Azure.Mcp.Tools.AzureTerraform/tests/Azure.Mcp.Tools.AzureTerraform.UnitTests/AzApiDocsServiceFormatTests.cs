// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Services;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.UnitTests;

public class AzApiDocsServiceFormatTests
{
    [Fact]
    public void FormatAsHcl_BasicResourceType_ContainsExpectedHeaders()
    {
        var complexTypes = new List<ComplexType>();

        string result = AzApiDocsService.FormatAsHcl(
            "Microsoft.Storage/storageAccounts",
            "2023-05-01",
            "Microsoft.Resources/resourceGroups",
            "ResourceGroup",
            complexTypes);

        Assert.Contains("# Resource Type: Microsoft.Storage/storageAccounts@2023-05-01", result);
        Assert.Contains("API Version: 2023-05-01", result);
        Assert.Contains("Parent resource type: Microsoft.Resources/resourceGroups", result);
        Assert.Contains("azapi_resource", result);
        Assert.Contains("```hcl", result);
    }

    [Fact]
    public void FormatAsHcl_ResourceGroupScope_IncludesLocation()
    {
        var complexTypes = new List<ComplexType>();

        string result = AzApiDocsService.FormatAsHcl(
            "Microsoft.Storage/storageAccounts",
            "2023-05-01",
            "Microsoft.Resources/resourceGroups",
            "ResourceGroup",
            complexTypes);

        Assert.Contains("location", result);
    }

    [Fact]
    public void FormatAsHcl_TenantScope_OmitsLocation()
    {
        var complexTypes = new List<ComplexType>();

        string result = AzApiDocsService.FormatAsHcl(
            "Microsoft.Management/managementGroups",
            "2021-04-01",
            "",
            "Tenant",
            complexTypes);

        Assert.DoesNotContain("location", result);
    }

    [Fact]
    public void FormatAsHcl_GeneratesLabelFromLastPart()
    {
        var complexTypes = new List<ComplexType>();

        string result = AzApiDocsService.FormatAsHcl(
            "Microsoft.Compute/virtualMachines",
            "2024-03-01",
            "Microsoft.Resources/resourceGroups",
            "ResourceGroup",
            complexTypes);

        // "virtualMachines" => "virtualMachine" (strips trailing 's')
        Assert.Contains("\"virtualMachine\"", result);
    }

    [Fact]
    public void FormatAsHcl_ChildResource_HasCorrectParentId()
    {
        var complexTypes = new List<ComplexType>();

        string result = AzApiDocsService.FormatAsHcl(
            "Microsoft.Compute/virtualMachines/extensions",
            "2024-03-01",
            "Microsoft.Compute/virtualMachines",
            "ResourceGroup",
            complexTypes);

        Assert.Contains("Microsoft.Compute/virtualMachines", result);
    }

    [Fact]
    public void FormatAsHcl_WithObjectTypeEntity_FormatsProperties()
    {
        var bodyProperties = new List<PropertyInfo>
        {
            new("properties", "StorageAccountProperties", null, null, null),
            new("sku", "Sku", null, null, null)
        };

        var bodyType = new ObjectTypeEntity
        {
            Name = "StorageAccountBody",
            Properties = bodyProperties
        };

        var resourceEntity = new ResourceTypeEntity
        {
            Name = "Microsoft.Storage/storageAccounts@2023-05-01",
            BodyType = bodyType,
            WritableScopes = "ResourceGroup"
        };

        var propertiesObj = new ObjectTypeEntity
        {
            Name = "StorageAccountProperties",
            Properties =
            [
                new("kind", "string", "The kind of storage account", "Required", null),
                new("accessTier", "string", "The access tier", "Optional", null)
            ]
        };

        var skuObj = new ObjectTypeEntity
        {
            Name = "Sku",
            Properties =
            [
                new("name", "string", "SKU name", "Required", null)
            ]
        };

        var complexTypes = new List<ComplexType>
        {
            resourceEntity,
            propertiesObj,
            skuObj
        };

        string result = AzApiDocsService.FormatAsHcl(
            "Microsoft.Storage/storageAccounts",
            "2023-05-01",
            "Microsoft.Resources/resourceGroups",
            "ResourceGroup",
            complexTypes);

        Assert.Contains("body = {", result);
        Assert.Contains("properties = {", result);
        Assert.Contains("kind", result);
        Assert.Contains("(Required)", result);
        Assert.Contains("sku = {", result);
    }

    [Fact]
    public void FormatAsHcl_SkipsReadOnlyProperties()
    {
        var bodyProperties = new List<PropertyInfo>
        {
            new("properties", "TestProperties", null, null, null)
        };

        var bodyType = new ObjectTypeEntity
        {
            Name = "TestBody",
            Properties = bodyProperties
        };

        var resourceEntity = new ResourceTypeEntity
        {
            Name = "Microsoft.Test/resources@2023-01-01",
            BodyType = bodyType,
            WritableScopes = "ResourceGroup"
        };

        var propertiesObj = new ObjectTypeEntity
        {
            Name = "TestProperties",
            Properties =
            [
                new("writable", "string", "A writable property", "Required", null),
                new("readOnly", "string", "A read-only property", "ReadOnly", null)
            ]
        };

        var complexTypes = new List<ComplexType> { resourceEntity, propertiesObj };

        string result = AzApiDocsService.FormatAsHcl(
            "Microsoft.Test/resources",
            "2023-01-01",
            "Microsoft.Resources/resourceGroups",
            "ResourceGroup",
            complexTypes);

        Assert.Contains("writable", result);
        Assert.DoesNotContain("readOnly", result);
    }
}
