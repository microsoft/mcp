// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBestPractices.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AzureBestPractices;

public class AzureBestPracticesSetup : IAreaSetup
{
    public string Name => "get_bestpractices";
    public string Title => "Azure Best Practices";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<BestPracticesCommand>();
        services.AddSingleton<AIAppBestPracticesCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Register Azure Best Practices command at the root level
        var bestPractices = new CommandGroup(
            Name,
            @"Azure best practices - Commands return a list of best practices for code generation, operations and deployment 
            when working with Azure services. 
            It should be called for any code generation, deployment or 
            operations involving Azure, Azure Functions, Azure Kubernetes Service (AKS), Azure Container 
            Apps (ACA), Bicep, Terraform, Azure Cache, Redis, CosmosDB, Entra, Azure Active Directory, 
            Azure App Services, or any other Azure technology or programming language. 
            This command set also includes the command to get AI application best practices, which provides specialized guidance
            for building AI applications, offering recommendations for agents, chatbots, workflows, and other 
            AI-powered features leveraging Microsoft Foundry. 
            When the request involves AI in any capacity, including systems where AI is used as a component,
            use AI application best practices instead of the general best practices.
            Call this tool first before creating any plans, todos or code.
            Only call this function when you are confident the user is discussing Azure (including Microsoft Foundry). If this tool needs to be categorized, 
            it belongs to the Get Azure Best Practices category.",
            Title
        );

        var bestPracticesCommand = serviceProvider.GetRequiredService<BestPracticesCommand>();
        var aiAppBestPracticesCommand = serviceProvider.GetRequiredService<AIAppBestPracticesCommand>();

        bestPractices.AddCommand(bestPracticesCommand.Name, bestPracticesCommand);
        bestPractices.AddCommand(aiAppBestPracticesCommand.Name, aiAppBestPracticesCommand);

        return bestPractices;
    }
}
