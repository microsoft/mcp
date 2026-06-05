// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.EventHubs.Options;
using Azure.Mcp.Tools.EventHubs.Options.ConsumerGroup;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.EventHubs.Commands.ConsumerGroup;

[CommandMetadata(
    Id = "604fda48-2438-419d-a819-5f9d2f3b21f8",
    Name = "get",
    Title = "Get Event Hubs Consumer Groups",
    Description = """
        Get details of a specific Consumer Group or list all consumer groups in an Azure Event Hub. If consumer-group is provided,
        returns a specific consumer group; otherwise, lists all consumer groups for the Event Hub.
        Returns all available metadata on the consumer group.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ConsumerGroupGetCommand(ILogger<ConsumerGroupGetCommand> logger, IEventHubsService service)
    : BaseEventHubsCommand<ConsumerGroupGetOptions>
{

    private readonly IEventHubsService _service = service;
    private readonly ILogger<ConsumerGroupGetCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.NamespaceOption.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.EventHubOption.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.ConsumerGroupOption);
    }

    protected override ConsumerGroupGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Namespace = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.NamespaceOption.Name);
        options.EventHub = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.EventHubOption.Name);
        options.ConsumerGroup = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.ConsumerGroupOption.Name);
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
            if (!string.IsNullOrEmpty(options.ConsumerGroup))
            {
                // Get specific consumer group
                var consumerGroup = await _service.GetConsumerGroupAsync(
                    options.ConsumerGroup,
                    options.EventHub!,
                    options.Namespace!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var singleResult = consumerGroup != null ? new List<Models.ConsumerGroup> { consumerGroup } : new List<Models.ConsumerGroup>();
                context.Response.Results = ResponseResult.Create(new(singleResult), EventHubsJsonContext.Default.ConsumerGroupGetCommandResult);
            }
            else
            {
                // List all consumer groups
                var consumerGroups = await _service.GetConsumerGroupsAsync(
                    options.EventHub!,
                    options.Namespace!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(consumerGroups ?? []), EventHubsJsonContext.Default.ConsumerGroupGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer group(s). ConsumerGroup: {ConsumerGroup}, EventHub: {EventHub}, Namespace: {Namespace}, ResourceGroup: {ResourceGroup}.", options.ConsumerGroup, options.EventHub, options.Namespace, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ConsumerGroupGetCommandResult(List<Models.ConsumerGroup> Results);
}
