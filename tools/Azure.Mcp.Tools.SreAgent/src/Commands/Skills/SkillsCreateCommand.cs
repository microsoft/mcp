// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Skills;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

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
public sealed class SkillsCreateCommand(ILogger<SkillsCreateCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<SkillsCreateOptions>
{
    private readonly ILogger<SkillsCreateCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Name);
        command.Options.Add(SreAgentOptionDefinitions.Content);
        command.Options.Add(SreAgentOptionDefinitions.Description);
    }

    protected override SkillsCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Agent.Name);
        options.Name = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Name.Name) ?? string.Empty;
        options.Content = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Content.Name);
        options.Description = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Description.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options.Subscription!,
                options.ResourceGroup,
                options.Agent!,
                options.Tenant,
                options.RetryPolicy,
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

    internal record SkillsCreateCommandResult(SreSkill Skill);
}
