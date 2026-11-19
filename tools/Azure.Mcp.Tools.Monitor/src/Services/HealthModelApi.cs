// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager;
using Azure.ResourceManager.CloudHealth;

namespace Azure.Mcp.Tools.Monitor.Services;

/// <summary>
/// The CloudHealth REST API version the inspect requests use, and the published contract of its payloads, which the
/// tools pass through unchanged. Requests are built from the SDK's models, so <see cref="Version"/> moves together
/// with the Azure.ResourceManager.CloudHealth package, never past the API version its models describe.
/// </summary>
internal static class HealthModelApi
{
    internal const string Version = "2026-11-01";

    /// <summary>The OpenAPI document that defines every CloudHealth payload the tools return.</summary>
    internal const string Contract =
        $"https://github.com/Azure/azure-rest-api-specs/blob/main/specification/cloudhealth/resource-manager/Microsoft.CloudHealth/CloudHealth/stable/{Version}/cloudhealth.json";

    /// <summary>Client options that send <see cref="Version"/> for every CloudHealth resource type.</summary>
    internal static ArmClientOptions ClientOptions()
    {
        var options = new ArmClientOptions();
        foreach (var resourceType in new[]
        {
            HealthModelResource.ResourceType,
            HealthModelEntityResource.ResourceType,
            HealthModelRelationshipResource.ResourceType,
            HealthModelSignalDefinitionResource.ResourceType,
            HealthModelDiscoveryRuleResource.ResourceType,
            HealthModelAuthenticationSettingResource.ResourceType,
        })
        {
            options.SetApiVersion(resourceType, Version);
        }
        return options;
    }
}
