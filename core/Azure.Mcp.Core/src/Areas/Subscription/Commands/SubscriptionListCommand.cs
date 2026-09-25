// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Subscription.Models;
using Azure.Mcp.Core.Areas.Subscription.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Core.Areas.Subscription.Commands;

[CommandMetadata(
    Id = "72bbe80e-ca42-4a43-8f02-45495bca1179",
    Name = "list",
    Title = "List Azure Subscriptions",
    Description = "List all Azure subscriptions for the current account. Returns subscriptionId, displayName, state, tenantId, and isDefault for each subscription. The isDefault field indicates the user's default subscription as resolved from the Azure CLI profile (configured via 'az account set') or, if not set there, from the AZURE_SUBSCRIPTION_ID environment variable. When the user has not specified a subscription, prefer the subscription where isDefault is true. If no default can be determined from either source and multiple subscriptions exist, ask the user which subscription to use.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class SubscriptionListCommand(ILogger<SubscriptionListCommand> logger, IAzureService azureService)
    : AuthenticatedCommand<SubscriptionListOptions, SubscriptionListCommand.SubscriptionListCommandResult>()
{
    private readonly ILogger<SubscriptionListCommand> _logger = logger;
    private readonly IAzureService _azureService = azureService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SubscriptionListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var subscriptions = await _azureService.GetSubscriptions(options.Tenant, cancellationToken);

            var defaultSubscriptionId = _azureService.GetDefaultSubscriptionId();
            var subscriptionInfos = MapToSubscriptionInfos(subscriptions, defaultSubscriptionId);

            context.Response.Results = ResponseResult.Create(
                new(subscriptionInfos),
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
            hasDefault && s.SubscriptionId.Equals(defaultSubscriptionId, StringComparisons.SubscriptionId)
        )).ToList();

        // Sort so the default subscription appears first
        if (hasDefault)
        {
            infos = [.. infos.OrderByDescending(s => s.IsDefault)];
        }

        return infos;
    }

    public sealed record SubscriptionListCommandResult(List<SubscriptionInfo> Subscriptions);
}
