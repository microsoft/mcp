// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.EventGrid.Options;
using Azure.Mcp.Tools.EventGrid.Options.Subscription;
using Azure.Mcp.Tools.EventGrid.Services;

namespace Azure.Mcp.Tools.EventGrid.Commands.Subscription;

public sealed class SubscriptionListCommand(ILogger<SubscriptionListCommand> logger) : BaseEventGridCommand<SubscriptionListOptions>
{
    private const string CommandTitle = "List Event Grid Subscriptions";
    private readonly ILogger<SubscriptionListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        """
        List event subscriptions for topics with filtering and endpoint configuration. This tool shows all active 
        subscriptions including webhook endpoints, event filters, and delivery retry policies. Returns subscription 
        details as JSON array. Requires either --topic (bare topic name) OR --subscription. If only --topic is provided
        the tool searches all accessible subscriptions for a topic with that name. Optional --resource-group/--location
        may only be used alongside --subscription or --topic.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup);
        command.Options.Add(EventGridOptionDefinitions.TopicName);
        command.Options.Add(EventGridOptionDefinitions.Location);
    }

    protected override SubscriptionListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.TopicName = parseResult.GetValueOrDefault<string>(EventGridOptionDefinitions.TopicName.Name);
        options.Location = parseResult.GetValueOrDefault<string>(EventGridOptionDefinitions.Location.Name);
        return options;
    }

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        // Skip the base validation that requires subscription and implement custom validation
        var result = new ValidationResult { IsValid = true };

        var hasSubscription = HasSubscriptionAvailable(commandResult);
        var hasTopicOption = commandResult.HasOptionResult(EventGridOptionDefinitions.TopicName);
        var hasRg = commandResult.HasOptionResult(OptionDefinitions.Common.ResourceGroup);
        var hasLocation = commandResult.HasOptionResult(EventGridOptionDefinitions.Location);

        // Either topic or subscription is mandatory
        if (!hasSubscription && !hasTopicOption)
        {
            result.IsValid = false;
            result.ErrorMessage = "Either --subscription or --topic is required.";

            if (commandResponse != null)
            {
                commandResponse.Status = 400;
                commandResponse.Message = result.ErrorMessage;
            }
        }
        // Location and resource-group can only be used with subscription or topic
        else if ((hasRg || hasLocation) && !hasSubscription && !hasTopicOption)
        {
            result.IsValid = false;
            result.ErrorMessage = "Either --subscription or --topic is required when using --resource-group or --location.";

            if (commandResponse != null)
            {
                commandResponse.Status = 400;
                commandResponse.Message = result.ErrorMessage;
            }
        }

        return result;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        var hasSubscription = !string.IsNullOrWhiteSpace(options.Subscription);
        var hasTopic = !string.IsNullOrWhiteSpace(options.TopicName);

        // Bare topic name without subscription triggers cross-subscription search
        bool crossSubscriptionSearch = !hasSubscription && hasTopic;

        try
        {
            var eventGridService = context.GetService<IEventGridService>();

            if (crossSubscriptionSearch)
            {
                // Iterate all subscriptions and aggregate
                var subscriptionService = context.GetService<ISubscriptionService>();
                var allSubs = await subscriptionService.GetSubscriptions(null, options.RetryPolicy);
                var aggregate = new List<EventGridSubscriptionInfo>();
                foreach (var sub in allSubs)
                {
                    try
                    {
                        var found = await eventGridService.GetSubscriptionsAsync(
                            sub.SubscriptionId,
                            options.ResourceGroup,
                            options.TopicName, // bare name
                            options.Location,
                            options.Tenant,
                            options.RetryPolicy);
                        if (found?.Count > 0)
                        {
                            aggregate.AddRange(found);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed searching topic '{Topic}' in subscription '{Sub}'. Continuing.", options.TopicName, sub.SubscriptionId);
                        continue;
                    }
                }
                context.Response.Results = ResponseResult.Create(new(aggregate), EventGridJsonContext.Default.SubscriptionListCommandResult);
            }
            else
            {
                var subscriptions = await eventGridService.GetSubscriptionsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.TopicName,
                    options.Location,
                    options.Tenant,
                    options.RetryPolicy);

                context.Response.Results = ResponseResult.Create(new(subscriptions ?? []), EventGridJsonContext.Default.SubscriptionListCommandResult);
            }
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
