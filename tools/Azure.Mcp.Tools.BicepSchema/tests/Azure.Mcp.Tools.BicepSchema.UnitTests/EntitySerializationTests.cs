// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;
using Xunit;

namespace Azure.Mcp.Tools.BicepSchema.UnitTests;

public class EntitySerializationTests
{
    [Fact]
    public void ResourceTypeEntity_ShouldDeserializeAndSerialize_FromTypeLoader()
    {
        string expectedJson = """
        {
          "$type": "Resource",
          "bodyType": {
            "$type": "Object",
            "properties": [
              {
                "name": "apiVersion",
                "type": "2024-05-01",
                "description": "The resource api version",
                "flags": "ReadOnly, DeployTimeConstant"
              },
              {
                "name": "etag",
                "type": "string",
                "description": "ETag of the resource.",
                "flags": "ReadOnly"
              },
              {
                "name": "id",
                "type": "string",
                "description": "The resource id",
                "flags": "ReadOnly, DeployTimeConstant"
              },
              {
                "name": "location",
                "type": "string",
                "description": "Resource location.",
                "flags": "Required"
              },
              {
                "name": "name",
                "type": "string",
                "description": "The resource name",
                "flags": "Required, DeployTimeConstant",
                "modifiers": "minLength: 1, maxLength: 45, pattern: ^[a-zA-Z](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$"
              },
              {
                "name": "properties",
                "type": "ApiManagementGatewayProperties",
                "description": "Properties of the API Management gateway.",
                "flags": "Required"
              },
              {
                "name": "sku",
                "type": "ApiManagementGatewaySkuProperties",
                "description": "SKU properties of the API Management gateway.",
                "flags": "Required"
              },
              {
                "name": "systemData",
                "type": "SystemData",
                "description": "Metadata pertaining to creation and last modification of the resource.",
                "flags": "ReadOnly"
              },
              {
                "name": "tags",
                "type": "ApimResourceTags",
                "description": "Resource tags."
              },
              {
                "name": "type",
                "type": "Microsoft.ApiManagement/gateways",
                "description": "The resource type",
                "flags": "ReadOnly, DeployTimeConstant"
              }
            ],
            "name": "Microsoft.ApiManagement/gateways"
          },
          "writableScopes": "ResourceGroup",
          "readableScopes": "ResourceGroup",
          "name": "Microsoft.ApiManagement/gateways@2024-05-01"
        }
        """;

        // Deserialize
        ComplexType? result = JsonSerializer.Deserialize<ComplexType>(expectedJson);

        // Assert deserialized object
        Assert.NotNull(result);
        Assert.IsType<ResourceTypeEntity>(result);
        var resourceTypeEntity = (ResourceTypeEntity)result!;
        Assert.Equal("Microsoft.ApiManagement/gateways@2024-05-01", resourceTypeEntity.Name);
        Assert.IsType<ObjectTypeEntity>(resourceTypeEntity.BodyType);
        var bodyEntity = (ObjectTypeEntity)resourceTypeEntity.BodyType;
        Assert.Equal("Microsoft.ApiManagement/gateways", bodyEntity.Name);
        Assert.Equal(10, bodyEntity.Properties.Count);

        // Serialize
        string serialized = JsonSerializer.Serialize(result, new JsonSerializerOptions() { WriteIndented = true });
        Assert.Equal(expectedJson, serialized);
    }

    [Fact]
    public void ResourceTypeEntity_ShouldDeserializeAndSerialize()
    {
        string input = """
        {
          "$type": "Resource",
          "bodyType": {
            "$type": "Object",
            "properties": [
              {
                "name": "apiVersion",
                "type": "2024-05-01",
                "description": "The resource api version",
                "flags": "ReadOnly, DeployTimeConstant"
              },
              {
                "name": "etag",
                "type": "string",
                "description": "ETag of the resource.",
                "flags": "ReadOnly"
              },
              {
                "name": "name",
                "type": "string",
                "description": "The resource name",
                "flags": "Required, DeployTimeConstant",
                "modifiers": "minLength: 1, maxLength: 45, pattern: ^[a-zA-Z](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$"
              },
              {
                "name": "properties",
                "type": "ApiManagementGatewayProperties",
                "description": "",
                "flags": "None"
              }
            ],
            "name": "Microsoft.ApiManagement/gateways"
          },
          "writableScopes": "ResourceGroup",
          "readableScopes": "ResourceGroup",
          "name": "Microsoft.ApiManagement/gateways@2024-05-01"
        }
        """;

        string expected = """
        {
          "$type": "Resource",
          "bodyType": {
            "$type": "Object",
            "properties": [
              {
                "name": "apiVersion",
                "type": "2024-05-01",
                "description": "The resource api version",
                "flags": "ReadOnly, DeployTimeConstant"
              },
              {
                "name": "etag",
                "type": "string",
                "description": "ETag of the resource.",
                "flags": "ReadOnly"
              },
              {
                "name": "name",
                "type": "string",
                "description": "The resource name",
                "flags": "Required, DeployTimeConstant",
                "modifiers": "minLength: 1, maxLength: 45, pattern: ^[a-zA-Z](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$"
              },
              {
                "name": "properties",
                "type": "ApiManagementGatewayProperties"
              }
            ],
            "name": "Microsoft.ApiManagement/gateways"
          },
          "writableScopes": "ResourceGroup",
          "readableScopes": "ResourceGroup",
          "name": "Microsoft.ApiManagement/gateways@2024-05-01"
        }
        """;

        // Deserialize
        ComplexType? deserialized = JsonSerializer.Deserialize<ComplexType>(input);
        Assert.IsType<ResourceTypeEntity>(deserialized);

        // Assert deserialized object
        string serialized = JsonSerializer.Serialize(deserialized, new JsonSerializerOptions() { WriteIndented = true });

        Assert.Equal(expected, serialized);
    }

    [Fact]
    public void ObjectTypeEntity_ShouldDeserializeAndSerialize()
    {
        string input = """
          {
            "$type": "Object",
            "properties": [
              {
                "name": "linkedServiceName",
                "type": "LinkedServiceReference",
                "description": "Linked service reference."
              },
              {
                "name": "policy",
                "type": "ActivityPolicy",
                "description": "Activity policy."
              },
              {
                "name": "type",
                "type": "AzureDataExplorerCommand",
                "description": "Type of activity.",
                "flags": "Required"
              },
              {
                "name": "typeProperties",
                "type": "AzureDataExplorerCommandActivityTypeProperties",
                "description": "Azure Data Explorer command activity properties.",
                "flags": "Required"
              }
            ],
            "name": "AzureDataExplorerCommandActivity"
          }
          """;

        string expected = """
          {
            "$type": "Object",
            "properties": [
              {
                "name": "linkedServiceName",
                "type": "LinkedServiceReference",
                "description": "Linked service reference."
              },
              {
                "name": "policy",
                "type": "ActivityPolicy",
                "description": "Activity policy."
              },
              {
                "name": "type",
                "type": "AzureDataExplorerCommand",
                "description": "Type of activity.",
                "flags": "Required"
              },
              {
                "name": "typeProperties",
                "type": "AzureDataExplorerCommandActivityTypeProperties",
                "description": "Azure Data Explorer command activity properties.",
                "flags": "Required"
              }
            ],
            "name": "AzureDataExplorerCommandActivity"
          }
          """;

        // Deserialize
        ComplexType? deserialized = JsonSerializer.Deserialize<ComplexType>(input);
        Assert.IsType<ObjectTypeEntity>(deserialized);


        // Assert deserialized object
        string serialized = JsonSerializer.Serialize(deserialized, new JsonSerializerOptions() { WriteIndented = true });

        Assert.Equal(expected, serialized);
    }

    [Fact]
    public void ResourceFunctionTypeEntity_ShouldDeserializeAndSerialize()
    {
        string input = """
          [
            {
            "$type": "ResourceInstanceFunction",
            "resourceType": "Microsoft.DataFactory/factories/integrationRuntimes",
            "apiVersion": "2018-06-01",
            "inputType": null,
            "outputType": "IntegrationRuntimeAuthKeys",
            "name": "listAuthKeys"
            },
            {
            "$type": "ResourceInstanceFunction",
            "resourceType": "Microsoft.DataFactory/factories/integrationRuntimes",
            "apiVersion": "2018-06-01",
            "inputType": null,
            "outputType": "IntegrationRuntimeAuthKeys",
            "name": "listAuthKeys"
            }
          ]
          """;

        string expected = """
          [
            {
              "$type": "ResourceInstanceFunction",
              "resourceType": "Microsoft.DataFactory/factories/integrationRuntimes",
              "apiVersion": "2018-06-01",
              "inputType": null,
              "outputType": "IntegrationRuntimeAuthKeys",
              "name": "listAuthKeys"
            },
            {
              "$type": "ResourceInstanceFunction",
              "resourceType": "Microsoft.DataFactory/factories/integrationRuntimes",
              "apiVersion": "2018-06-01",
              "inputType": null,
              "outputType": "IntegrationRuntimeAuthKeys",
              "name": "listAuthKeys"
            }
          ]
          """;

        // Deserialize
        ComplexType[]? deserialized = JsonSerializer.Deserialize<ResourceFunctionTypeEntity[]>(input);
        Assert.IsType<ResourceFunctionTypeEntity[]>(deserialized);

        // Assert deserialized object
        string serialized = JsonSerializer.Serialize(deserialized, new JsonSerializerOptions() { WriteIndented = true });

        Assert.Equal(expected, serialized);
    }

    [Fact]
    public void DiscriminatedObjectTypeEntity_ShouldDeserializeAndSerialize()
    {
        string input = """
          {
            "$type": "DiscriminatedObject",
            "baseProperties": [
              {
                "name": "dependsOn",
                "type": "ActivityDependency[]",
                "description": "Activity depends on condition."
              },
              {
                "name": "description",
                "type": "string",
                "description": "Activity description.",
                "flags": "None",
                "modifiers": ""
              },
              {
                "name": "name",
                "type": "string",
                "description": null,
                "flags": "Required",
                "modifiers": null
              }
            ],
            "elements": [
              {
                "$type": "Object",
                "properties": [
                  {
                    "name": "type",
                    "type": "AppendVariable",
                    "description": "Type of activity.",
                    "flags": "Required"
                  },
                  {
                    "name": "typeProperties",
                    "type": "AppendVariableActivityTypeProperties",
                    "description": "Append Variable activity properties.",
                    "flags": "Required"
                  }
                ],
                "name": "AppendVariableActivity"
              },
              {
                "$type": "Object",
                "properties": [
                  {
                    "name": "linkedServiceName",
                    "type": "LinkedServiceReference",
                    "description": "Linked service reference."
                  },
                  {
                    "name": "policy",
                    "type": "ActivityPolicy",
                    "description": ""
                  },
                  {
                    "name": "type",
                    "type": "AzureDataExplorerCommand",
                    "description": "Type of activity.",
                    "flags": "None"
                  },
                  {
                    "name": "typeProperties",
                    "type": "AzureDataExplorerCommandActivityTypeProperties",
                    "description": "Azure Data Explorer command activity properties.",
                    "flags": "Required"
                  }
                ],
                "name": "AzureDataExplorerCommandActivity"
              }
            ],
            "discriminator": "type",
            "name": "Activity"
          }
          """;

        string expected = """
          {
            "$type": "DiscriminatedObject",
            "baseProperties": [
              {
                "name": "dependsOn",
                "type": "ActivityDependency[]",
                "description": "Activity depends on condition."
              },
              {
                "name": "description",
                "type": "string",
                "description": "Activity description."
              },
              {
                "name": "name",
                "type": "string",
                "flags": "Required"
              }
            ],
            "elements": [
              {
                "$type": "Object",
                "properties": [
                  {
                    "name": "type",
                    "type": "AppendVariable",
                    "description": "Type of activity.",
                    "flags": "Required"
                  },
                  {
                    "name": "typeProperties",
                    "type": "AppendVariableActivityTypeProperties",
                    "description": "Append Variable activity properties.",
                    "flags": "Required"
                  }
                ],
                "name": "AppendVariableActivity"
              },
              {
                "$type": "Object",
                "properties": [
                  {
                    "name": "linkedServiceName",
                    "type": "LinkedServiceReference",
                    "description": "Linked service reference."
                  },
                  {
                    "name": "policy",
                    "type": "ActivityPolicy"
                  },
                  {
                    "name": "type",
                    "type": "AzureDataExplorerCommand",
                    "description": "Type of activity."
                  },
                  {
                    "name": "typeProperties",
                    "type": "AzureDataExplorerCommandActivityTypeProperties",
                    "description": "Azure Data Explorer command activity properties.",
                    "flags": "Required"
                  }
                ],
                "name": "AzureDataExplorerCommandActivity"
              }
            ],
            "discriminator": "type",
            "name": "Activity"
          }
          """;

        // Deserialize
        ComplexType? deserialized = JsonSerializer.Deserialize<ComplexType>(input);
        Assert.IsType<DiscriminatedObjectTypeEntity>(deserialized);

        // Assert deserialized object
        string serialized = JsonSerializer.Serialize(deserialized, new JsonSerializerOptions() { WriteIndented = true });

        Assert.Equal(expected, serialized);
    }
}
