// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Deploy.Models;

public static class AzureServiceConstants
{
    public enum AzureComputeServiceType
    {
        AppService,
        FunctionApp,
        ContainerApp,
        StaticWebApp,
        AKS
    }

    public enum AzureServiceType
    {
        AzureAISearch,
        AzureAIServices,
        AppService,
        AzureApplicationInsights,
        AzureBotService,
        AzureContainerApp,
        AzureCosmosDB,
        AzureFunctionApp,
        AzureKeyVault,
        AKS,
        AzureDatabaseForMySQL,
        AzureOpenAI,
        AzureDatabaseForPostgreSQL,
        AzurePrivateEndpoint,
        AzureCacheForRedis,
        AzureSQLDatabase,
        AzureStorageAccount,
        StaticWebApp,
        AzureServiceBus,
        AzureSignalRService,
        AzureVirtualNetwork,
        AzureWebPubSub
    }
}

public static class DeployTelemetryTags
{
    private static string AddPrefix(string tagName) => $"deploy/{tagName}";

    public static readonly string ComputeHostResources = AddPrefix("ComputeHostResources");
    public static readonly string DeploymentTool = AddPrefix("DeploymentTool");
    public static readonly string IacType = AddPrefix("IacType");
    public static readonly string ProjectName = AddPrefix("ProjectName");
    public static readonly string ServiceCount = AddPrefix("ServiceCount");
    public static readonly string BackingServiceResources = AddPrefix("BackingServiceResources");
}
