// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureAIBestPractices.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.AzureAIBestPractices;

public class AzureAIBestPracticesSetup : IAreaSetup
{
    public string Name => "azureaibestpractices";

    public string Title => "Azure AI Code Generation Best Practices";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<AzureAIBestPracticesGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Register Azure AI Best Practices command at the root level
        var azureAIBestPractices = new CommandGroup(
            Name,
            @"Azure AI best practices - Commands returns best practices and code generation guidance for building AI applications in Azure. 
            Use this tool when you need recommendations on how to write code for AI agents, chatbots, workflows, or other AI features. 
            This tool also provides guidance for code generation using the Azure resources (e.g. Azure AI Foundry) for application development only. 
            Call this tool first before creating any plans, todos or code.
            > Note: Understanding User Intent for Azure AI Foundry: 
            > (1) Resource Management - use the 'foundry' tool instead
            > e.g., 'create/deploy/provision agent/embedding/model/project in Foundry', 'set up agent/model resource'
            > (2) Application Development - use this 'azureaibestpractices' tool
            > e.g., 'build/write/implement agent', 'develop chatbot/assistant', 'agent code'
            > When ambiguous, clarify whether the user wants resource management (foundry tool) or application code generation (this tool).
            If this tool needs to be categorized, it belongs to the Azure Best Practices category.",
            Title
        );

        var practices = serviceProvider.GetRequiredService<AzureAIBestPracticesGetCommand>();
        azureAIBestPractices.AddCommand(practices.Name, practices);

        return azureAIBestPractices;
    }
}
