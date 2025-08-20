// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;
using Azure.Mcp.Tools.BicepSchema.Services.Support;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.BicepSchema.Services
{
    public class BicepSchemaService() : BaseAzureService, IBicepSchemaService
    {
        public TypesDefinitionResult GetResourceTypeDefinitions(IServiceProvider serviceProvider, string resourceTypeName, string? apiVersion = null)
        {
            ResourceVisitor resourceVisitor = serviceProvider.GetRequiredService<ResourceVisitor>();

            if (string.IsNullOrEmpty(apiVersion))
            {
                apiVersion = ApiVersionSelector.SelectLatestStable(resourceVisitor.GetResourceApiVersions(resourceTypeName));
            }

            return resourceVisitor.LoadSingleResource(resourceTypeName, apiVersion);
        }
    }
}
