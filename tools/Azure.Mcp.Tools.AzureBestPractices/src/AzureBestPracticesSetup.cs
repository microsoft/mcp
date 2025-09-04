// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureBestPractices.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureBestPractices;

public class AzureBestPracticesSetup : IAreaSetup
{
    public string Name => "bestpractices";

    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Register Azure Best Practices command at the root level
        var bestPractices = new CommandGroup(
            Name,
            @"Azure best practices – Commands return secure, production-grade guidance and recommendations 
            for cloud architecture, security, performance, and cost optimization when working with Azure services. 
            Use this tool for Azure resource configuration, security hardening, deployment patterns, monitoring setup, 
            and before generating Azure SDK code or infrastructure templates (Bicep, Terraform, ARM). 
            It should be called for code generation, deployment, or operations involving Azure technologies such as 
            Azure Functions, Azure Kubernetes Service (AKS), Azure Container Apps (ACA), Azure Cache for Redis, 
            Cosmos DB, Entra ID (Azure Active Directory), Azure App Services, and other Azure services. 
            Do not use this tool for specific troubleshooting, real-time monitoring, or direct resource management 
            operations – it focuses on architectural and security best practices rather than executing actions. 
            If this tool needs to be categorized, it belongs to the Azure Best Practices category."
        );
        rootGroup.AddSubGroup(bestPractices);

        bestPractices.AddCommand(
            "get",
            new BestPracticesCommand(loggerFactory.CreateLogger<BestPracticesCommand>())
        );
    }
}
