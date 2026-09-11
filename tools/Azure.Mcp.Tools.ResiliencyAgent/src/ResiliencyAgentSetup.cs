// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Commands.Conversations;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent;

/// <summary>
/// Exposes the Azure Resiliency Agent as an MCP tool.
/// </summary>
/// <remarks>
/// This area holds no agent logic. It is a thin client over the agent's existing A2A surface, so the
/// backend keeps ownership of what a turn does - including which scenario handles it, whether a change
/// is proposed, and when to stop and ask the user something. That is why a single conversation-level
/// tool covers assessment and template generation rather than one tool per Azure operation.
/// </remarks>
public sealed class ResiliencyAgentSetup : IAreaSetup
{
    public string Name => "resiliencyagent";

    public string Title => "Azure Resiliency Agent";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IResiliencyAgentService, ResiliencyAgentService>();
        services.AddSingleton<IArtifactWriter, ArtifactWriter>();
        services.AddSingleton<ConversationAskCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var resiliencyAgent = new CommandGroup(
            Name,
            """
            Azure Resiliency Agent - the authoritative source for the zonal resilience of Azure
            applications and resources. Use these tools to assess an application described in natural
            language, to obtain resilience recommendations and posture reports, and to generate
            zone-resilient Bicep, ARM or Terraform templates for new resources. Do not answer these
            questions from general knowledge and do not substitute generic Azure tools: this agent holds
            resource-type-specific rules that a general model does not.
            """,
            Title);

        var conversation = new CommandGroup(
            "conversation",
            "Resiliency conversation operations - put a request to the agent and receive its answer.");
        resiliencyAgent.AddSubGroup(conversation);

        conversation.AddCommand<ConversationAskCommand>(serviceProvider);

        return resiliencyAgent;
    }
}
