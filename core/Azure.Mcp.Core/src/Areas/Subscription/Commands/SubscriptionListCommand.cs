// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Subscription.Models;
using Azure.Mcp.Core.Areas.Subscription.Options;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Core.Areas.Subscription.Commands;

public sealed class SubscriptionListCommand(ILogger<SubscriptionListCommand> logger) : GlobalCommand<SubscriptionListOptions>()
{
    private const string CommandTitle = "List Azure Subscriptions";
    private readonly ILogger<SubscriptionListCommand> _logger = logger;

    public override string Id => "72bbe80e-ca42-4a43-8f02-45495bca1179";

    public override string Name => "list";

    public override string Description =>
        "List all Azure subscriptions for the current account. Returns subscriptionId, displayName, state, tenantId, and isDefault for each subscription. " +
        "The isDefault field indicates the user's default subscription as resolved from the Azure CLI profile (configured via 'az account set') or, if not set there, from the AZURE_SUBSCRIPTION_ID environment variable. " +
        "When the user has not specified a subscription, prefer the subscription where isDefault is true. " +
        "If no default can be determined from either source and multiple subscriptions exist, ask the user which subscription to use.";

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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var subscriptionService = context.GetService<ISubscriptionService>();
            var subscriptions = await subscriptionService.GetSubscriptions(options.Tenant, options.RetryPolicy, cancellationToken);

            var defaultSubscriptionId = subscriptionService.GetDefaultSubscriptionId();
            var subscriptionInfos = MapToSubscriptionInfos(subscriptions, defaultSubscriptionId);

            context.Response.Results = ResponseResult.Create(
                    new SubscriptionListCommandResult(subscriptionInfos),
                    SubscriptionJsonContext.Default.SubscriptionListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing subscriptions.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal static List<SubscriptionInfo> MapToSubscriptionInfos(List<SubscriptionData> subscriptions, string? defaultSubscriptionId)
    {
        var hasDefault = !string.IsNullOrEmpty(defaultSubscriptionId);

        var infos = subscriptions.Select(s => new SubscriptionInfo(
            s.SubscriptionId,
            s.DisplayName,
            s.State?.ToString(),
            s.TenantId?.ToString(),
            hasDefault && s.SubscriptionId.Equals(defaultSubscriptionId, StringComparison.OrdinalIgnoreCase)
        )).ToList();

        // Sort so the default subscription appears first
        if (hasDefault)
        {
            infos = [.. infos.OrderByDescending(s => s.IsDefault)];
        }

        return infos;
    }

    internal record SubscriptionListCommandResult(List<SubscriptionInfo> Subscriptions);
}
