// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Subscription.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Areas.Subscription.Commands;

public sealed class SubscriptionListCommand(ILogger<SubscriptionListCommand> logger) : GlobalCommand<SubscriptionListOptions>()
{
    private const string CommandTitle = "List Azure Subscriptions";
    private readonly ILogger<SubscriptionListCommand> _logger = logger;
    private readonly Option<int> _characterLimitOption = OptionDefinitions.Common.CharacterLimit;

    public override string Name => "list";

    public override string Description =>
    "List all or current subscriptions for an account in Azure; returns subscriptionId, displayName, state, tenantId, and isDefault. Use for scope selection in governance, policy, access, cost management, or deployment.";
    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_characterLimitOption);
    }

    protected override SubscriptionListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.CharacterLimit = parseResult.GetValue(_characterLimitOption);
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
            var subscriptionService = context.GetService<ISubscriptionService>();
            var subscriptions = await subscriptionService.GetSubscriptions(options.Tenant, options.RetryPolicy);

            if (subscriptions?.Count > 0)
            {
                var result = new SubscriptionListCommandResult(subscriptions);

                // Serialize to check character count
                var json = System.Text.Json.JsonSerializer.Serialize(result, SubscriptionJsonContext.Default.SubscriptionListCommandResult);

                if (json.Length <= options.CharacterLimit)
                {
                    // Response is within limit
                    context.Response.Results = ResponseResult.Create(result, SubscriptionJsonContext.Default.SubscriptionListCommandResult);
                    context.Response.Message = $"All {subscriptions.Count} subscriptions returned ({json.Length} characters).";
                }
                else
                {
                    // Response exceeds limit, truncate subscriptions
                    var truncatedSubscriptions = new List<SubscriptionData>();
                    var currentLength = 0;

                    foreach (var subscription in subscriptions)
                    {
                        var tempList = new List<SubscriptionData>(truncatedSubscriptions) { subscription };
                        var tempResult = new SubscriptionListCommandResult(tempList);
                        var tempJson = System.Text.Json.JsonSerializer.Serialize(tempResult, SubscriptionJsonContext.Default.SubscriptionListCommandResult);

                        if (tempJson.Length <= options.CharacterLimit)
                        {
                            truncatedSubscriptions.Add(subscription);
                            currentLength = tempJson.Length;
                        }
                        else
                        {
                            break;
                        }
                    }

                    var truncatedResult = new SubscriptionListCommandResult(truncatedSubscriptions);
                    context.Response.Results = ResponseResult.Create(truncatedResult, SubscriptionJsonContext.Default.SubscriptionListCommandResult);
                    context.Response.Message = $"Results truncated to {truncatedSubscriptions.Count} of {subscriptions.Count} subscriptions ({currentLength} characters). Increase --{OptionDefinitions.Common.CharacterLimitName} to see more results.";
                }
            }
            else
            {
                context.Response.Results = null;
                context.Response.Message = "No subscriptions found.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing subscriptions.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record SubscriptionListCommandResult(List<SubscriptionData> Subscriptions);
}
