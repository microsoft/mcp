// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Bicep.Types;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;
using Azure.Mcp.Tools.BicepSchema.Services.Support;
using Microsoft.Azure.Mcp.AzTypes.Internal.Compact;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.BicepSchema.Services;

public static class SchemaGenerator
{
    public static List<ComplexType> GetResponse(TypesDefinitionResult typesDefinitionResult)
    {
        var allComplexTypes = new List<ComplexType>();
        allComplexTypes.AddRange(typesDefinitionResult.ResourceTypeEntities);
        allComplexTypes.AddRange(typesDefinitionResult.ResourceFunctionTypeEntities);
        allComplexTypes.AddRange(typesDefinitionResult.OtherComplexTypeEntities);
        return allComplexTypes;
    }

    public static TypesDefinitionResult GetResourceTypeDefinitions(
        IServiceProvider serviceProvider,
        string resourceTypeName,
        string? apiVersion = null)
    {
        ResourceVisitor resourceVisitor = serviceProvider.GetRequiredService<ResourceVisitor>();

        if (string.IsNullOrEmpty(apiVersion))
        {
            apiVersion = ApiVersionSelector.SelectLatestStable(resourceVisitor.GetResourceApiVersions(resourceTypeName));
        }

        return resourceVisitor.LoadSingleResource(resourceTypeName, apiVersion);
    }

    public static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<ITypeLoader, AzTypeLoader>();
        services.AddSingleton<ResourceVisitor>();
    }
}
