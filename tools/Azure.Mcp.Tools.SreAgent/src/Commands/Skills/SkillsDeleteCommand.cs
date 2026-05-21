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
    Id = "a052cd8d-05a5-44ed-8ee1-ef131c8b0321",
    Name = "delete",
    Title = "Delete SRE Agent Tool",
    Description = "Deletes a custom tool from a targeted SRE Agent resource. Required: --subscription, --agent, --name, --confirm true.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SkillsDeleteCommand(ILogger<SkillsDeleteCommand> logger, ISreAgentService sreAgentService)
    : BaseSreAgentCommand<SkillsDeleteOptions>
{
    private readonly ILogger<SkillsDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.Name);
        command.Options.Add(SreAgentOptionDefinitions.Confirm);
    }

    protected override SkillsDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Agent.Name);
        options.Name = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Name.Name) ?? string.Empty;
        options.Confirm = parseResult.GetValueOrDefault<bool>(SreAgentOptionDefinitions.Confirm.Name);
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
            if (!options.Confirm)
            {
                throw new InvalidOperationException($"Refusing to delete skill '{options.Name}': destructive operation requires --confirm true.");
            }

            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options.Subscription!,
                options.ResourceGroup,
                options.Agent!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var result = await _sreAgentService.DeleteSkillAsync(endpoint, options.Name, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(result), SreAgentJsonContext.Default.SkillsDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SRE Agent skill {Name} from agent resource {Agent}.", options.Name, options.Agent);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SkillsDeleteCommandResult(SreAgentDeleteResult Tool);
}
