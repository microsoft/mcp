// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.EventGrid.Models;
using Azure.Mcp.Tools.EventGrid.Options;
using Azure.Mcp.Tools.EventGrid.Options.Subscription;
using Azure.Mcp.Tools.EventGrid.Services;

namespace Azure.Mcp.Tools.EventGrid.Commands.Subscription;

public sealed class SubscriptionListCommand(ILogger<SubscriptionListCommand> logger) : BaseEventGridCommand<SubscriptionListOptions>
{
    private const string CommandTitle = "List Event Grid Subscriptions";
    private readonly ILogger<SubscriptionListCommand> _logger = logger;
    private readonly Option<string> _topicNameOption = EventGridOptionDefinitions.TopicName;
    private readonly Option<string> _locationOption = EventGridOptionDefinitions.Location;

    public override string Name => "list";

    public override string Description =>
        """
        List event subscriptions for topics with filtering and endpoint configuration. This tool shows all active 
        subscriptions including webhook endpoints, event filters, and delivery retry policies. Returns subscription 
        details as JSON array. Requires topic-name or subscription.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        UseResourceGroup(); // Optional resource group filtering
        command.Options.Add(_topicNameOption);
        command.Options.Add(_locationOption);
    }

    protected override SubscriptionListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TopicName = parseResult.GetValue(_topicNameOption);
        options.Location = parseResult.GetValue(_locationOption);
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
            var eventGridService = context.GetService<IEventGridService>();
            var subscriptions = await eventGridService.GetSubscriptionsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.TopicName,
                options.Location,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create<SubscriptionListCommandResult>(
                new SubscriptionListCommandResult(subscriptions ?? []),
                EventGridJsonContext.Default.SubscriptionListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Event Grid subscriptions. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, TopicName: {TopicName}, Location: {Location}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options.TopicName, options.Location, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SubscriptionListCommandResult(List<EventGridSubscriptionInfo> Subscriptions);
}
