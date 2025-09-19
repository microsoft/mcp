// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Bicep.Types;
using Azure.Mcp.Tools.BicepSchema.Services;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Azure.Mcp.Tools.BicepSchema.UnitTests;

public class GetSchemaTests
{
    private readonly ITestOutputHelper _output;

    public GetSchemaTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private void SummarizeSchema(TypesDefinitionResult result, string response)
    {
        _output.WriteLine($"Schema for {result.ResourceTypeEntities[0].Name}");
        _output.WriteLine($"String length: {response.Length.ToString("N0", CultureInfo.InvariantCulture)}");
        Assert.Single(result.ResourceTypeEntities);
        _output.WriteLine($"Resource: {result.ResourceTypeEntities[0].Name}");
        _output.WriteLine($"Functions: {string.Join(",", result.ResourceFunctionTypeEntities.Select(x => x.Name))}");
        _output.WriteLine($"Other complex type entities: {string.Join(",", result.OtherComplexTypeEntities.Select(x => x.Name))}");
    }

    [Fact]
    public void GetResourcePropertySchema_ShouldThrowOnNotFound()
    {
        var serviceCollection = new ServiceCollection();
        SchemaGenerator.ConfigureServices(serviceCollection);
        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        var exception = Assert.Throws<Exception>(() =>
        {
            TypesDefinitionResult result = SchemaGenerator.GetResourceTypeDefinitions(serviceProvider, "Microsoft.Unknown/virtualRandom");
            _ = SchemaGenerator.GetResponse(result);
        });
        Assert.Equal("Resource type Microsoft.Unknown/virtualRandom not found.", exception.Message);
    }

    [Fact]
    public void GetResourceTypeDefinition()
    {
        var serviceCollection = new ServiceCollection();
        SchemaGenerator.ConfigureServices(serviceCollection);
        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        TypesDefinitionResult result = SchemaGenerator.GetResourceTypeDefinitions(serviceProvider, "Microsoft.ApiManagement/service", "2024-05-01");
        Assert.Equal("2024-05-01", result.ApiVersion);
        Assert.Equal("Microsoft.ApiManagement", result.ResourceProvider);

        Assert.Single(result.ResourceTypeEntities);
        Assert.Contains("Microsoft.ApiManagement/service@2024-05-01", result.ResourceTypeEntities.Select(r => r.Name));

        var expectedFunctions = new[]
        {
            "Microsoft.ApiManagement/service/gateways@2024-05-01::listDebugCredentials",
            "Microsoft.ApiManagement/service/gateways@2024-05-01::listKeys",
            "Microsoft.ApiManagement/service/policyFragments@2024-05-01::listReferences",
            "Microsoft.ApiManagement/service/workspaces/policyFragments@2024-05-01::listReferences",
            "Microsoft.ApiManagement/service/authorizationServers@2024-05-01::listSecrets",
            "Microsoft.ApiManagement/service/identityProviders@2024-05-01::listSecrets",
            "Microsoft.ApiManagement/service/openidConnectProviders@2024-05-01::listSecrets",
            "Microsoft.ApiManagement/service/portalsettings@2024-05-01::listSecrets",
            "Microsoft.ApiManagement/service/subscriptions@2024-05-01::listSecrets",
            "Microsoft.ApiManagement/service/tenant@2024-05-01::listSecrets",
            "Microsoft.ApiManagement/service/workspaces/subscriptions@2024-05-01::listSecrets",
            "Microsoft.ApiManagement/service/gateways@2024-05-01::listTrace",
            "Microsoft.ApiManagement/service/namedValues@2024-05-01::listValue",
            "Microsoft.ApiManagement/service/workspaces/namedValues@2024-05-01::listValue",
        };
        var actualFunctions = result.ResourceFunctionTypeEntities.Select(f => $"{f.ResourceType}@{f.ApiVersion}::{f.Name}").ToArray();
        Assert.Equal(expectedFunctions.OrderBy(x => x), actualFunctions.OrderBy(x => x));

        var expectedComplexTypes = new[]
        {
            "AccessInformationSecretsContract",
            "AdditionalLocation",
            "ApiManagementServiceBasePropertiesCustomProperties",
            "ApiManagementServiceIdentity",
            "ApiManagementServiceIdentityUserAssignedIdentities",
            "ApiManagementServiceProperties",
            "ApiManagementServiceSkuProperties",
            "ApimResourceTags",
            "ApiVersionConstraint",
            "ArmIdWrapper",
            "AuthorizationServerSecretsContract",
            "CertificateConfiguration",
            "CertificateInformation",
            "ClientSecretContract",
            "ConfigurationApi",
            "GatewayDebugCredentialsContract",
            "GatewayKeysContract",
            "GatewayListDebugCredentialsContract",
            "GatewayListTraceContract",
            "GatewayTraceContract",
            "HostnameConfiguration",
            "NamedValueSecretContract",
            "PortalSettingValidationKeyContract",
            "PrivateEndpointConnectionWrapperProperties",
            "PrivateLinkServiceConnectionState",
            "RemotePrivateEndpointConnectionWrapper",
            "ResourceCollection",
            "ResourceCollectionValueItem",
            "SubscriptionKeysContract",
            "SystemData",
            "UserIdentityProperties",
            "VirtualNetworkConfiguration"
        };
        var actualComplexTypes = result.OtherComplexTypeEntities.Select(r => r.Name).ToArray();
        Assert.Equal(expectedComplexTypes.OrderBy(x => x), actualComplexTypes.OrderBy(x => x));
    }

    [Fact]
    public void GetAllResourceTypesAndVersionsByProvider()
    {
        var serviceCollection = new ServiceCollection();
        SchemaGenerator.ConfigureServices(serviceCollection);
        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        ResourceVisitor visitor = new(serviceProvider.GetRequiredService<ITypeLoader>());
        var result = visitor.GetAllResourceTypesAndVersionsByProvider();

        Assert.True(result.Count >= 262);
        Assert.True(result.ContainsKey("microsoft.compute"));
        Assert.Equal("Microsoft.Compute", result["microsoft.compute"].Provider);
        Assert.True(result["microsoft.compute"].ResourceTypes.Count >= 43);
        Assert.True(result["microsoft.compute"].ResourceTypes["microsoft.compute/virtualmachines"].ApiVersions.Count >= 25);
    }

    [Fact]
    public void LoadSingleVersion()
    {
        const string resourceProvider = "Microsoft.ApiManagement";
        const string resourceType = "gateways";
        const string apiVersion = "2024-05-01";

        var serviceCollection = new ServiceCollection();
        SchemaGenerator.ConfigureServices(serviceCollection);
        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        ResourceVisitor visitor = new(serviceProvider.GetRequiredService<ITypeLoader>());
        TypesDefinitionResult result = visitor.LoadSingleResource($"{resourceProvider}/{resourceType}", apiVersion);

        Assert.Equal(resourceProvider, result.ResourceProvider);
        Assert.Equal(apiVersion, result.ApiVersion);
        Assert.Single(result.ResourceTypeEntities);
        Assert.Equal(14, result.ResourceFunctionTypeEntities.Count);
        Assert.Equal(21, result.OtherComplexTypeEntities.Count);

        Assert.IsType<ResourceTypeEntity>(result.ResourceTypeEntities[0]);
        Assert.IsType<ObjectTypeEntity>(result.ResourceTypeEntities[0].BodyType);
        Assert.Equal("Microsoft.ApiManagement/gateways", result.ResourceTypeEntities[0].BodyType.Name);

        string actualApiVersion = ((ObjectTypeEntity)result.ResourceTypeEntities[0].BodyType).Properties.Single(x => x.Name == "apiVersion").Type;
        Assert.Equal(apiVersion, actualApiVersion);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetResponse_HasCorrectTypes(bool compactFormat)
    {
        var serviceCollection = new ServiceCollection();
        SchemaGenerator.ConfigureServices(serviceCollection);
        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        TypesDefinitionResult result = SchemaGenerator.GetResourceTypeDefinitions(serviceProvider, "Microsoft.ApiManagement/service", "2024-05-01");
        List<ComplexType> allComplexTypesResult = SchemaGenerator.GetResponse(result);
        string response = GetSerializedResponse(allComplexTypesResult, compactFormat);
        Assert.False(string.IsNullOrEmpty(response));

        Assert.Contains("Microsoft.ApiManagement/service@2024-05-01", response);
        Assert.Contains("listDebugCredentials", response);
        Assert.Contains("listKeys", response);
        Assert.Contains("listSecrets", response);
        Assert.Contains("listReferences", response);
        Assert.Contains("listTrace", response);
        Assert.Contains("listValue", response);
        Assert.Contains("AccessInformationSecretsContract", response);
        Assert.Contains("AdditionalLocation", response);
        Assert.Contains("ApiManagementServiceBasePropertiesCustomProperties", response);
        Assert.Contains("ApiManagementServiceIdentity", response);
        Assert.Contains("ApiManagementServiceIdentityUserAssignedIdentities", response);
        Assert.Contains("ApiManagementServiceProperties", response);
        Assert.Contains("ApiManagementServiceSkuProperties", response);
        Assert.Contains("ApimResourceTags", response);
        Assert.Contains("ApiVersionConstraint", response);
        Assert.Contains("ArmIdWrapper", response);
        Assert.Contains("AuthorizationServerSecretsContract", response);
        Assert.Contains("CertificateConfiguration", response);
        Assert.Contains("CertificateInformation", response);
        Assert.Contains("ClientSecretContract", response);
        Assert.Contains("ConfigurationApi", response);
        Assert.Contains("GatewayDebugCredentialsContract", response);
        Assert.Contains("GatewayKeysContract", response);
        Assert.Contains("GatewayListDebugCredentialsContract", response);
        Assert.Contains("GatewayListTraceContract", response);
        Assert.Contains("GatewayTraceContract", response);
        Assert.Contains("HostnameConfiguration", response);
        Assert.Contains("NamedValueSecretContract", response);
        Assert.Contains("PortalSettingValidationKeyContract", response);
        Assert.Contains("PrivateEndpointConnectionWrapperProperties", response);
        Assert.Contains("PrivateLinkServiceConnectionState", response);
        Assert.Contains("RemotePrivateEndpointConnectionWrapper", response);
        Assert.Contains("ResourceCollection", response);
        Assert.Contains("ResourceCollectionValueItem", response);
        Assert.Contains("SubscriptionKeysContract", response);
        Assert.Contains("SystemData", response);
        Assert.Contains("UserIdentityProperties", response);
        Assert.Contains("VirtualNetworkConfiguration", response);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetResponse_Format(bool compactFormat)
    {
        const string resourceType = "Microsoft.Compute/virtualMachineScaleSets";
        const string apiVersion = "2024-07-01";

        var serviceCollection = new ServiceCollection();
        SchemaGenerator.ConfigureServices(serviceCollection);
        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        TypesDefinitionResult result = SchemaGenerator.GetResourceTypeDefinitions(serviceProvider, $"{resourceType}", apiVersion);
        List<ComplexType> allComplexTypesResult = SchemaGenerator.GetResponse(result);
        string response = GetSerializedResponse(allComplexTypesResult, compactFormat);

        // Verify
        Assert.Contains($"{resourceType}@{apiVersion}", response);

        using JsonDocument document = JsonDocument.Parse(response);
        JsonElement root = document.RootElement;
        Assert.Equal("Resource", root[0].GetProperty("$type").GetString());
        Assert.Equal($"{resourceType}@{apiVersion}", root[0].GetProperty("name").GetString());

        //TODO: Consider more checks after optimizing output
    }

    [Fact]
    public void GetResponse_CheckExactFormat()
    {
        const string resourceType = "Microsoft.ApiManagement/service/diagnostics/loggers";
        const string apiVersion = "2018-01-01";

        const string expected = """
          [
            {
              "$type": "Resource",
              "bodyType": {
                "$type": "Object",
                "properties": [
                  {
                    "name": "apiVersion",
                    "type": "2018-01-01",
                    "description": "The resource api version",
                    "flags": "ReadOnly, DeployTimeConstant"
                  },
                  {
                    "name": "id",
                    "type": "string",
                    "description": "The resource id",
                    "flags": "ReadOnly, DeployTimeConstant"
                  },
                  {
                    "name": "name",
                    "type": "string",
                    "description": "The resource name",
                    "flags": "Required, DeployTimeConstant",
                    "modifiers": "maxLength: 80, pattern: (^[\\w]\u002B$)|(^[\\w][\\w\\-]\u002B[\\w]$)"
                  },
                  {
                    "name": "type",
                    "type": "Microsoft.ApiManagement/service/diagnostics/loggers",
                    "description": "The resource type",
                    "flags": "ReadOnly, DeployTimeConstant"
                  }
                ],
                "name": "Microsoft.ApiManagement/service/diagnostics/loggers"
              },
              "writableScopes": "ResourceGroup",
              "readableScopes": "None",
              "name": "Microsoft.ApiManagement/service/diagnostics/loggers@2018-01-01"
            }
          ]
          """;

        var serviceCollection = new ServiceCollection();
        SchemaGenerator.ConfigureServices(serviceCollection);
        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        TypesDefinitionResult result = SchemaGenerator.GetResourceTypeDefinitions(serviceProvider, $"{resourceType}", apiVersion);
        List<ComplexType> allComplexTypesResult = SchemaGenerator.GetResponse(result);
        string response = GetSerializedResponse(allComplexTypesResult, compactFormat: false);

        Assert.Equal(expected, response);
    }

    [Fact]
    public void GetResponse_HugeSchema()
    {
        const string resourceType = "Microsoft.DataFactory/factories/pipelines";
        const string apiVersion = "2018-06-01";

        var serviceCollection = new ServiceCollection();
        SchemaGenerator.ConfigureServices(serviceCollection);
        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        TypesDefinitionResult result = SchemaGenerator.GetResourceTypeDefinitions(serviceProvider, $"{resourceType}", apiVersion);
        List<ComplexType> allComplexTypesResult = SchemaGenerator.GetResponse(result);
        string response = GetSerializedResponse(allComplexTypesResult, compactFormat: false);
        string responseCompact = GetSerializedResponse(allComplexTypesResult, compactFormat: true);

        _output.WriteLine($"Schema size raw = {response.Length}");
        _output.WriteLine($"Schema size compact = {responseCompact.Length}");
        SummarizeSchema(result, responseCompact);

        _output.WriteLine($"\n{response}");

        Assert.True(response.Length > responseCompact.Length);

        Assert.True(responseCompact.Length > 5000);
        Assert.True(responseCompact.Length <= 317668);
    }

    private string GetSerializedResponse(List<ComplexType> allComplexTypesResult, bool compactFormat)
    {
        return JsonSerializer.Serialize(
                allComplexTypesResult,
                new JsonSerializerOptions
                {
                    WriteIndented = !compactFormat
                });
    }

#if false
    // This currently takes about 3 minutes on my machine and there are almost 3000 resources -> 0.06s per resource
    //
    // Currently smallest schemas:
    //   Microsoft.Compute/cloudServices/updateDomains
    //   Microsoft.Sql/servers/databases/schemas
    // Currently largest:
    //   Microsoft.DataFactory/factories/pipelines
    //   Microsoft.DataFactory/factories/linkedservices
    //   Microsoft.DataMigration/services/projects/tasks
    [Fact]
    public void InspectAllSchemas()
    {
        const int TAKE = int.MaxValue;

        var serviceCollection = new ServiceCollection();
        SchemaGenerator.ConfigureServices(serviceCollection);
        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        ResourceVisitor visitor = serviceProvider.GetRequiredService<ResourceVisitor>();
        Dictionary<string, (TypesDefinitionResult Result, string Response)> results = [];

        string[] resourceTypeNames = [..
            visitor.GetAllResourceTypesAndVersionsByProvider().SelectMany(p => p.Value.ResourceTypes.Values).Select(r => r.ResourceType).Take(TAKE)];

        _output.WriteLine($"===== Count of resources: {resourceTypeNames.Length}");

        foreach (string resourceTypeName in resourceTypeNames)
        {
            TypesDefinitionResult result = SchemaGenerator.GetResourceTypeDefinitions(serviceProvider, resourceTypeName);
            string response = SchemaGenerator.GetResponse(result, compactFormat: true);

            results[resourceTypeName] = (result, response);
        }

        _output.WriteLine($"Minimum response string length: {results.Min(x => x.Value.Response.Length)}");
        _output.WriteLine($"Maximum response string length: {results.Max(x => x.Value.Response.Length)}");
        _output.WriteLine($"Average response string length: {Math.Round(results.Average(x => x.Value.Response.Length))}");

        _output.WriteLine($"\nLargest to smallest:\n");

        foreach ((string key, (TypesDefinitionResult Result, string Response)) in results.OrderBy(x => x.Value.Response.Length))
        {
            _output.WriteLine($"Resource: {key}");
            SummarizeSchema(Result, Response);
            _output.WriteLine("");
        }

        _output.WriteLine(string.Join("\nDone."));
    }
#endif
}
