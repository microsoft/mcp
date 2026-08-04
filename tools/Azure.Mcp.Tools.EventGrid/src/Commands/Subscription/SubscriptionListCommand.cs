// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.EventGrid.Models;
using Azure.Mcp.Tools.EventGrid.Options.Subscription;
using Azure.Mcp.Tools.EventGrid.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.EventGrid.Commands.Subscription;

[CommandMetadata(
    Id = "716a33e5-755c-4168-87ed-8a4651476c6e",
    Name = "list",
    Title = "List Event Grid Subscriptions",
    Description = "Show all available Event Grid subscriptions with optional topic filtering. This tool displays active event subscriptions including webhook endpoints, event filters, and delivery retry policies. Use this when you need to show, list, or get Event Grid subscriptions for topics. Requires either topic name OR subscription. If only topic is provided, searches all accessible subscriptions for a topic with that name. Resource group and location filters can be applied, but only when used with a subscription or topic.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SubscriptionListCommand(
    ILogger<SubscriptionListCommand> logger,
    IEventGridService eventGridService,
    ISubscriptionService subscriptionService,
    ISubscriptionResolver subscriptionResolver)
    : AuthenticatedCommand<SubscriptionListOptions, SubscriptionListCommand.SubscriptionListCommandResult>
{
    private readonly ILogger<SubscriptionListCommand> _logger = logger;
    private readonly IEventGridService _eventGridService = eventGridService;
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly ISubscriptionResolver _subscriptionResolver = subscriptionResolver;

    public override void PostBindOptions(SubscriptionListOptions options)
    {
        base.PostBindOptions(options);
        options.Subscription = _subscriptionResolver.ResolveSubscription(options.Subscription);
    }

    public override void ValidateOptions(SubscriptionListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        var hasSubscription = !string.IsNullOrWhiteSpace(options.Subscription);
        var hasTopicOption = !string.IsNullOrWhiteSpace(options.Topic);

        // Either topic or subscription is mandatory
        if (!hasSubscription && !hasTopicOption)
        {
            validationResult.Errors.Add("Either --subscription or --topic is required.");
        }
        // Location and resource-group can only be used with subscription or topic
        else if ((!string.IsNullOrWhiteSpace(options.ResourceGroup) || !string.IsNullOrWhiteSpace(options.Location)) &&
            !hasSubscription &&
            !hasTopicOption)
        {
            // Can this case even be reached?
            validationResult.Errors.Add("Either --subscription or --topic is required when using --resource-group or --location.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SubscriptionListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Bare topic name without subscription triggers cross-subscription search
            if (string.IsNullOrWhiteSpace(options.Subscription) && !string.IsNullOrWhiteSpace(options.Topic))
            {
                // Iterate all subscriptions and aggregate
                // TODO (alzimmer): Listing all subscriptions should be done in the IEventGridService implementation.
                var allSubs = await _subscriptionService.GetSubscriptions(options.Tenant, options.RetryPolicy, cancellationToken);
                var aggregate = new List<EventGridSubscriptionInfo>();
                foreach (var sub in allSubs)
                {
                    try
                    {
                        var found = await _eventGridService.GetSubscriptionsAsync(
                            sub.SubscriptionId,
                            options.ResourceGroup,
                            options.Topic, // bare name
                            options.Location,
                            options.Tenant,
                            options.RetryPolicy,
                            cancellationToken);
                        if (found?.Count > 0)
                        {
                            aggregate.AddRange(found);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed searching topic '{Topic}' in subscription '{Sub}'. Continuing.", options.Topic, sub.SubscriptionId);
                        continue;
                    }
                }
                context.Response.Results = ResponseResult.Create(new(aggregate), EventGridJsonContext.Default.SubscriptionListCommandResult);
            }
            else
            {
                var subscriptions = await _eventGridService.GetSubscriptionsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Topic,
                    options.Location,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(subscriptions ?? []), EventGridJsonContext.Default.SubscriptionListCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Event Grid subscriptions. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, TopicName: {TopicName}, Location: {Location}.",
                options.Subscription, options.ResourceGroup, options.Topic, options.Location);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record SubscriptionListCommandResult(List<EventGridSubscriptionInfo> Subscriptions);
}
