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
    public string Name => "get_bestpractices";

    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Register Azure Best Practices command at the root level
        var bestPractices = new CommandGroup(
            Name,
            @"Azure best practices - Commands return a list of best practices for code generation, operations and deployment 
            when working with Azure services. It should be called for any code generation, deployment or 
            operations involving Azure, Azure Functions, Azure Kubernetes Service (AKS), Azure Container 
            Apps (ACA), Bicep, Terraform, Azure Cache, Redis, CosmosDB, Entra, Azure Active Directory, 
            Azure App Services, or any other Azure technology or programming language. Only call this function 
            when you are confident the user is discussing Azure. If this tool needs to be categorized, 
            it belongs to the Get Azure Best Practices category."
        );
        rootGroup.AddSubGroup(bestPractices);

        bestPractices.AddCommand(
            "get",
            new BestPracticesCommand(loggerFactory.CreateLogger<BestPracticesCommand>())
        );
    }
}
