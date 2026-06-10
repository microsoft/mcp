// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.EventGrid.Options.Topic;
using Azure.Mcp.Tools.EventGrid.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.EventGrid.Commands.Topic;

[CommandMetadata(
    Id = "42390294-2856-4980-a057-095c91355650",
    Name = "list",
    Title = "List Event Grid Topics",
    Description = "List Event Grid topics in an Azure subscription or resource group. Returns topic names, endpoints, locations, and provisioning status.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TopicListCommand(ILogger<TopicListCommand> logger, IEventGridService eventGridService) : BaseEventGridCommand<TopicListOptions>
{
    private readonly ILogger<TopicListCommand> _logger = logger;
    private readonly IEventGridService _eventGridService = eventGridService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup);
    }

    protected override TopicListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
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
            var topics = await _eventGridService.GetTopicsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(topics ?? []), EventGridJsonContext.Default.TopicListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Event Grid topics. Subscription: {Subscription}.",
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record TopicListCommandResult(List<EventGridTopicInfo> Topics);
}
