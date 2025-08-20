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

    public override string Name => "list";

    public override string Description =>
        $"""
        List all Azure subscriptions accessible to your account. Optionally specify {OptionDefinitions.Common.TenantName}
        and {OptionDefinitions.Common.AuthMethodName}. Results include subscription names and IDs, returned as a JSON array.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var subscriptionService = context.GetService<ISubscriptionService>();
            var subscriptions = await subscriptionService.GetSubscriptions(options.Tenant, options.RetryPolicy);

            context.Response.Results = subscriptions?.Count > 0
                ? ResponseResult.Create(
                    new SubscriptionListCommandResult(subscriptions),
                    SubscriptionJsonContext.Default.SubscriptionListCommandResult)
                : null;
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
