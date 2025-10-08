// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.EventHubs.Commands;
using Azure.Mcp.Tools.EventHubs.Options;
using Azure.Mcp.Tools.EventHubs.Options.ConsumerGroup;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.EventHubs.Commands.ConsumerGroup;

public sealed class ConsumerGroupGetCommand(ILogger<ConsumerGroupGetCommand> logger)
    : BaseEventHubsCommand<ConsumerGroupGetOptions>
{
    private const string CommandTitle = "Get Event Hubs Consumer Groups";

    private readonly ILogger<ConsumerGroupGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        """
        Get consumer groups from Azure event hub. This command can either:
        
        List all consumer groups in an event hub (using --eventhub with --namespace with --resource-group)
        Get a single consumer group by name (using --consumer-group with --eventhub with --namespace with --resource-group)
        
        The eventhub, namespace, and resource-group parameters are required (for both get and list)
        The consumer-group parameter is only required for getting a specific consumer-group
        
        When retrieving a single consumer group and when listing all available consumer groups, return all available metadata on the consumer group.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        OpenWorld = true,       // Queries Azure resources
        Destructive = false,    // Read-only operation
        Idempotent = true,      // Same parameters produce same results
        ReadOnly = true,        // Only reads data
        Secret = false,         // Returns non-sensitive information
        LocalRequired = false   // Pure cloud API calls
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.NamespaceOption.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.EventHubNameOption.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.ConsumerGroupOption.AsOptional());
    }

    protected override ConsumerGroupGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Namespace = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.Namespace);
        options.EventHub = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.EventHubName);
        options.ConsumerGroup = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.ConsumerGroup);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var eventHubsService = context.GetService<IEventHubsService>();

            if (!string.IsNullOrEmpty(options.ConsumerGroup))
            {
                // Get specific consumer group
                var consumerGroup = await eventHubsService.GetConsumerGroupAsync(
                    options.ConsumerGroup,
                    options.EventHub!,
                    options.Namespace!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy);

                var singleResult = consumerGroup != null ? new List<Models.ConsumerGroup> { consumerGroup } : new List<Models.ConsumerGroup>();
                context.Response.Results = ResponseResult.Create(new ConsumerGroupGetCommandResult(singleResult), EventHubsJsonContext.Default.ConsumerGroupGetCommandResult);
            }
            else
            {
                // List all consumer groups
                var consumerGroups = await eventHubsService.ListConsumerGroupsAsync(
                    options.EventHub!,
                    options.Namespace!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy);

                context.Response.Results = ResponseResult.Create(new ConsumerGroupGetCommandResult(consumerGroups ?? []), EventHubsJsonContext.Default.ConsumerGroupGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer group(s). Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ConsumerGroupGetCommandResult(List<Models.ConsumerGroup> Results);
}