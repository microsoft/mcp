// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Skills;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Skills;

[CommandMetadata(
    Id = "21bb35ac-7301-495c-8193-57d482290d85",
    Name = "create",
    Title = "Create SRE Agent Skill",
    Description = "Creates or updates a custom skill on a targeted SRE Agent resource. Required: --subscription, --agent, --name, --content.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SkillsCreateCommand(ILogger<SkillsCreateCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SkillsCreateOptions, SkillsCreateCommand.SkillsCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<SkillsCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SkillsCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);

            var request = new SreSkillCreateRequest
            {
                Name = options.Name,
                Properties = new SreSkillProperties
                {
                    SkillContent = options.Content,
                    Description = options.Description
                }
            };

            var skill = await _sreAgentService.CreateSkillAsync(endpoint, request, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(skill), SreAgentJsonContext.Default.SkillsCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SRE Agent skill {Name} on agent resource {Agent}.", options.Name, options.Agent);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record SkillsCreateCommandResult(SreSkill Skill);
}
