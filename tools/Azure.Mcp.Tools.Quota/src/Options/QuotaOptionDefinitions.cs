// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Quota.Options;

public static class QuotaOptionDefinitions
{
    public static class QuotaCheck
    {
        public const string RegionName = "region";
        public const string ResourceTypesName = "resource-types";

        public static readonly Option<string> Region = new(
            $"--{RegionName}"
        )
        {
            Description = "The valid Azure region where the resources will be deployed. E.g. 'eastus', 'westus', etc.",
            Required = true
        };

        public static readonly Option<string> ResourceTypes = new(
            $"--{ResourceTypesName}"
        )
        {
            Description = "The valid Azure resource types that are going to be deployed(comma-separated). E.g. 'Microsoft.App/containerApps,Microsoft.Web/sites,Microsoft.CognitiveServices/accounts', etc.",
            Required = true,
            AllowMultipleArgumentsPerToken = true
        };
    }

    public static class RegionCheck
    {
        public const string ResourceTypesName = "resource-types";
        public const string CognitiveServiceModelNameName = "cognitive-service-model-name";
        public const string CognitiveServiceModelVersionName = "cognitive-service-model-version";
        public const string CognitiveServiceDeploymentSkuNameName = "cognitive-service-deployment-sku-name";

        public static readonly Option<string> ResourceTypes = new(
            $"--{ResourceTypesName}"
        )
        {
            Description = "Comma-separated list of Azure resource types to check available regions for. The valid Azure resource types. E.g. 'Microsoft.App/containerApps, Microsoft.Web/sites, Microsoft.CognitiveServices/accounts'.",
            Required = true,
            AllowMultipleArgumentsPerToken = true
        };

        public static readonly Option<string> CognitiveServiceModelName = new(
            $"--{CognitiveServiceModelNameName}"
        )
        {
            Description = "Optional model name for cognitive services. Only needed when Microsoft.CognitiveServices is included in resource types.",
            Required = false
        };

        public static readonly Option<string> CognitiveServiceModelVersion = new(
            $"--{CognitiveServiceModelVersionName}"
        )
        {
            Description = "Optional model version for cognitive services. Only needed when Microsoft.CognitiveServices is included in resource types.",
            Required = false
        };

        public static readonly Option<string> CognitiveServiceDeploymentSkuName = new(
            $"--{CognitiveServiceDeploymentSkuNameName}"
        )
        {
            Description = "Optional deployment SKU name for cognitive services. Only needed when Microsoft.CognitiveServices is included in resource types.",
            Required = false
        };
    }
}
