// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using AzureMcp.Ai.Commands.OpenAi;
using AzureMcp.Ai.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Ai;

public class AiSetup : IAreaSetup
{
    public string Name => "ai";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAiService, AiService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create AI command group
        var ai = new CommandGroup("ai",
            """
            AI operations - Commands for working with Azure AI services including Azure OpenAI for text generation,
            chat completions, and other AI-powered capabilities. Use this tool when you need to generate text,
            create completions, or interact with deployed AI models. This tool focuses on Azure OpenAI service
            operations and model interactions. This tool is a hierarchical MCP command router where sub-commands
            are routed to MCP servers that require specific fields inside the "parameters" object. To invoke a
            command, set "command" and wrap its arguments in "parameters". Set "learn=true" to discover available
            sub-commands for different Azure AI service operations.
            """);
        rootGroup.AddSubGroup(ai);

        // Create OpenAI subgroup
        var openai = new CommandGroup("openai", "Azure OpenAI operations - Commands for working with Azure OpenAI deployments and generating completions.");
        ai.AddSubGroup(openai);

        var completions = new CommandGroup("completions", "OpenAI completion operations - Commands for generating text completions using deployed models.");
        openai.AddSubGroup(completions);

        // Register commands
        var logger = loggerFactory.CreateLogger<OpenAiCompletionsCreateCommand>();
        completions.AddCommand("create", new OpenAiCompletionsCreateCommand(logger));
    }
}
