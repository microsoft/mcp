// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;
using Azure.Mcp.Tools.BicepSchema.Services.Support;

namespace Azure.Mcp.Tools.BicepSchema.Services
{
    public class BicepSchemaService(ResourceVisitor resourceVisitor) : IBicepSchemaService
    {
        public TypesDefinitionResult GetResourceTypeDefinitions(string resourceTypeName, string? apiVersion = null)
        {
            if (string.IsNullOrEmpty(apiVersion))
            {
                apiVersion = ApiVersionSelector.SelectLatestStable(resourceVisitor.GetResourceApiVersions(resourceTypeName));
            }

            return resourceVisitor.LoadSingleResource(resourceTypeName, apiVersion);
        }
    }
}
