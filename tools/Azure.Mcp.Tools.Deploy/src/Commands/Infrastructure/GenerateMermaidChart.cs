// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using Azure.Mcp.Tools.Deploy.Models;
using Azure.Mcp.Tools.Deploy.Options;

namespace Azure.Mcp.Tools.Deploy.Commands.Infrastructure;

public static class GenerateMermaidChart
{
    // used to create a subgraph for AKS cluster in the chart
    private const string aksClusterInternalName = "akscluster";
    private const string aksClusterName = "Azure Kubernetes Service (AKS) Cluster";

    private const string acaEnvInternalName = "acaenvironment";
    private const string acaEnvName = "Azure Container Apps Environment";

    public static string GenerateChart(string workspaceFolder, AppTopology appTopology)
    {
        var chartComponents = new List<string>();

        chartComponents.Add("graph TD");

        if (appTopology.Services.Any(s => s.AzureComputeHost == "aks"))
        {
            chartComponents.Add("classDef cluster fill:#ffffd0,stroke:#333,stroke-width:2px,color:#000");
        }

        var services = new List<string> { "%% Services" };
        var resources = new List<string> { "%% Compute Resources" };
        var dependencyResources = new List<string> { "%% Binding Resources" };
        var relationships = new List<string> { "%% Relationships" };

        foreach (var service in appTopology.Services)
        {
            var serviceName = new List<string> { $"Name: {service.Name}" };

            if (!string.IsNullOrWhiteSpace(workspaceFolder))
            {
                var projectRelativePath = Path.GetRelativePath(workspaceFolder, string.IsNullOrWhiteSpace(service.Path) ? workspaceFolder : service.Path);
                serviceName.Add($"Path: {projectRelativePath}");
            }
            serviceName.Add($"Language: {service.Language}");
            serviceName.Add($"Port: {service.Port}");

            if (service.DockerSettings != null &&
                string.Equals(service.AzureComputeHost, "azurecontainerapp", StringComparison.OrdinalIgnoreCase))
            {
                serviceName.Add($"DockerFile: {service.DockerSettings.DockerFilePath}");
                serviceName.Add($"Docker Context: {service.DockerSettings.DockerContext}");
            }

            var serviceInternalName = $"svc-{service.Name}";

            services.Add(CreateComponentName(serviceInternalName, string.Join("\n", serviceName), NodeShape.Rectangle));

            relationships.Add(CreateRelationshipString(serviceInternalName, $"{FlattenServiceType(service.AzureComputeHost)}_{service.Name}", "hosted on", ArrowType.Solid));
        }

        var aksClusterExists = false;
        var containerAppEnvExists = false;

        foreach (var service in appTopology.Services)
        {
            string serviceResourceInternalName = $"{FlattenServiceType(service.AzureComputeHost)}_{service.Name}";
            if (service.AzureComputeHost == "aks")
            {
                if (!aksClusterExists)
                {
                    // Add AKS cluster as a subgraph
                    resources.Add($"subgraph {aksClusterInternalName} [\"{aksClusterName}\"]");
                    // containerized services share the same AKS cluster
                    foreach (var aksservice in appTopology.Services.Where(s => s.AzureComputeHost == "aks"))
                    {
                        resources.Add(CreateComponentName($"{aksservice.AzureComputeHost}_{aksservice.Name}", $"{aksservice.Name} (Containerized Service)", NodeShape.RoundedRectangle));
                    }
                    resources.Add("end");
                    resources.Add($"{aksClusterInternalName}:::cluster");
                    aksClusterExists = true;
                }
            }
            else if (service.AzureComputeHost == "containerapp")
            {
                if (!containerAppEnvExists)
                {
                    // Add Container App Environment as a subgraph
                    resources.Add($"subgraph {acaEnvInternalName} [\"{acaEnvName}\"]");
                    // containerized services share the same Container App Environment
                    foreach (var containerAppService in appTopology.Services.Where(s => s.AzureComputeHost == "containerapp"))
                    {
                        resources.Add(CreateComponentName($"{containerAppService.AzureComputeHost}_{containerAppService.Name}", $"{containerAppService.Name} (Container App)", NodeShape.RoundedRectangle));
                    }
                    resources.Add("end");
                    containerAppEnvExists = true;
                }
            }
            // each service should have a compute resource type
            else if (!resources.Any(r => r.Contains(serviceResourceInternalName)))
            {
                resources.Add(CreateComponentName(serviceResourceInternalName, $"{service.Name} ({GetFormalName(service.AzureComputeHost)})", NodeShape.RoundedRectangle));
            }

            foreach (var dependency in service.Dependencies)
            {
                var instanceInternalName = $"{FlattenServiceType(dependency.ServiceType)}.{dependency.Name}";
                var instanceName = $"{dependency.Name} ({GetFormalName(dependency.ServiceType)})";

                if (IsComputeResourceType(dependency.ServiceType))
                {
                    if (!resources.Any(r => r.Contains(EnsureUrlFriendlyName(instanceInternalName))))
                    {
                        resources.Add(CreateComponentName(instanceInternalName, instanceName, NodeShape.RoundedRectangle));
                    }
                }
                else
                {
                    dependencyResources.Add(CreateComponentName(instanceInternalName, instanceName, NodeShape.Rectangle));
                }

                relationships.Add(CreateRelationshipString(serviceResourceInternalName, instanceInternalName, dependency.ConnectionType, ArrowType.Dotted));
            }
        }

        chartComponents.AddRange(services);
        chartComponents.AddRange(resources);


        chartComponents.Add("subgraph \"Compute Resources\"");
        chartComponents.AddRange(resources);
        chartComponents.Add("end");

        chartComponents.Add("subgraph \"Dependency Resources\"");
        chartComponents.AddRange(dependencyResources);
        chartComponents.Add("end");

        chartComponents.AddRange(relationships);

        return string.Join("\n", chartComponents);
    }

    private static string CreateComponentName(string internalName, string name, NodeShape nodeShape)
    {
        var nodeShapeBrackets = GetNodeShapeBrackets(nodeShape);
        return $"{EnsureUrlFriendlyName(internalName)}{nodeShapeBrackets[0]}\"`{name}`\"{nodeShapeBrackets[1]}";
    }

    private static string CreateRelationshipString(string sourceName, string targetName, string connectionDescription, ArrowType arrowType)
    {
        var arrowSymbol = GetArrowSymbol(arrowType);
        return $"{EnsureUrlFriendlyName(sourceName)} {arrowSymbol} |\"{connectionDescription}\"| {EnsureUrlFriendlyName(targetName)}";
    }

    private static string EnsureUrlFriendlyName(string name)
    {
        return name.Replace('.', '_')
                  .Replace(" ", "_")
                  .Trim()
                  .ToLowerInvariant();
    }

    private static string[] GetNodeShapeBrackets(NodeShape nodeShape)
    {
        return nodeShape switch
        {
            NodeShape.Rectangle => ["[", "]"],
            NodeShape.Circle => ["((", "))"],
            NodeShape.RoundedRectangle => ["(", ")"],
            NodeShape.Cylinder => ["[(", ")]"],
            NodeShape.Hexagon => ["{{", "}}"],
            _ => ["[", "]"]
        };
    }

    private static string GetArrowSymbol(ArrowType arrowType)
    {
        return arrowType switch
        {
            ArrowType.Solid => "-->",
            ArrowType.Open => "->",
            ArrowType.Dotted => "-.->",
            _ => "-->"
        };
    }

    private static string GetFormalName(string name)
    {
        if (ResourceTypeConverter.TryGetValue(name, out var formalName))
        {
            return formalName;
        }
        return name;

    }

    private static string FlattenServiceType(string serviceType)
    {
        return serviceType.ToLowerInvariant().Replace("azure", "");
    }

    private static bool IsComputeResourceType(string serviceType)
    {
        return Enum.GetNames<AzureServiceConstants.AzureComputeServiceType>().Contains(serviceType, StringComparer.OrdinalIgnoreCase);
    }

    private static IDictionary<string, string> ResourceTypeConverter = new Dictionary<string, string>
    {
        { "appservice", "Azure App Service" },
        { "containerapp", "Azure Container Apps" },
        { "functionapp", "Azure Functions" },
        { "staticwebapp", "Azure Static Web Apps" },
        { "aks", "Azure Kubernetes Services" },
        { "azureaisearch", "Azure AI Search" },
        { "azureaiservices", "Azure AI Services" },
        { "azureapplicationinsights", "Azure Application Insights" },
        { "azurebotservice", "Azure Bot Service" },
        { "azurecosmosdb", "Azure Cosmos DB" },
        { "azurekeyvault", "Azure Key Vault" },
        { "azuredatabaseformysql", "Azure Database for MySQL" },
        { "azureopenai", "Azure OpenAI" },
        { "azuredatabaseforpostgresql", "Azure Database for PostgreSQL" },
        { "azuresqldatabase", "Azure SQL Database" },
        { "azurecacheforredis", "Azure Cache For Redis"},
        { "azurestorageaccount", "Azure Storage Account" },
        { "azureservicebus", "Azure Service Bus" },
        { "azurewebpubsub", "Azure Web PubSub"}
    };
}

public enum NodeShape
{
    Rectangle,
    Circle,
    RoundedRectangle,
    Cylinder,
    Hexagon
}

public enum ArrowType
{
    Solid,
    Open,
    Dotted
}
