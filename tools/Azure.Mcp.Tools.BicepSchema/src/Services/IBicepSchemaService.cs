// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;

namespace Azure.Mcp.Tools.BicepSchema.Services
{
    public interface IBicepSchemaService
    {
        TypesDefinitionResult GetResourceTypeDefinitions(
        string resourceTypeName,
        string? apiVersion = null);
    }
}
