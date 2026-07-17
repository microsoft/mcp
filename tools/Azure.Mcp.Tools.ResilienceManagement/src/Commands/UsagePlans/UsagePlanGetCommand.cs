// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.UsagePlans;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.UsagePlans;

[CommandMetadata(
    Id = "a8d3f60b-5c27-4e19-8f74-0b6a2d9c5e83",
    Name = "get",
    Title = "Get or List Resilience Usage Plans",
    Description = """
        Gets resilience usage plans. Provide a usage plan name (with its resource group) to get the full details
        of that plan (id, name, resource type, location, tags, plan type, and provisioning state). Omit the name
        to list usage plans (id and name only): for the given resource group, or for the whole subscription when
        no resource group is provided.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class UsagePlanGetCommand(ILogger<UsagePlanGetCommand> logger, IResilienceManagementService resilienceManagementService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<UsagePlanGetOptions, UsagePlanGetCommand.UsagePlanGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<UsagePlanGetCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, UsagePlanGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            UsagePlanGetCommandResult result;
            if (!string.IsNullOrEmpty(options.Name))
            {
                if (string.IsNullOrEmpty(options.ResourceGroup))
                {
                    throw new ArgumentException("A resource group is required when getting a specific usage plan. Provide --resource-group.");
                }

                var usagePlan = await _resilienceManagementService.GetUsagePlanAsync(
                    options.ResourceGroup,
                    options.Name,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new UsagePlanGetCommandResult(UsagePlan: usagePlan);
            }
            else if (!string.IsNullOrEmpty(options.ResourceGroup))
            {
                var usagePlans = await _resilienceManagementService.ListUsagePlansAsync(
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new UsagePlanGetCommandResult(UsagePlans: usagePlans.ToList());
            }
            else
            {
                var usagePlans = await _resilienceManagementService.ListUsagePlansBySubscriptionAsync(
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new UsagePlanGetCommandResult(UsagePlans: usagePlans.ToList());
            }

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.UsagePlanGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting usage plan(s). ResourceGroup: {ResourceGroup}, Name: {Name}, Subscription: {Subscription}.",
                options.ResourceGroup, options.Name, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        KeyNotFoundException => "Usage plan not found. Verify the usage plan name, resource group, subscription, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the usage plan. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Usage plan not found. Verify the usage plan and resource group exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record UsagePlanGetCommandResult(List<ResourceSummary>? UsagePlans = null, UsagePlanInfo? UsagePlan = null);
}
