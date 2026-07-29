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
    Id = "a052cd8d-05a5-44ed-8ee1-ef131c8b0321",
    Name = "delete",
    Title = "Delete SRE Agent Tool",
    Description = "Deletes a custom skill from a targeted SRE Agent resource. Required: --subscription, --agent, --name, --confirm true.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SkillsDeleteCommand(ILogger<SkillsDeleteCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SkillsDeleteOptions, SkillsDeleteCommand.SkillsDeleteCommandResult>(subscriptionResolver)
{
    private readonly ILogger<SkillsDeleteCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SkillsDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!options.Confirm)
            {
                throw new InvalidOperationException($"Refusing to delete skill '{options.Name}': destructive operation requires --confirm true.");
            }

            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
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

    public sealed record SkillsDeleteCommandResult(SreAgentDeleteResult Tool);
}
