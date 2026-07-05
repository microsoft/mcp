// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Resources;

[CommandMetadata(
    Id = "a3e7f29c-5b81-4d06-8f53-2c9b1e7a4d68",
    Name = "get",
    Title = "Get or List Resilience Goal Resources",
    Description = """
        Gets the resources (members) of a resilience goal assignment. Provide a goal resource name to get the
        full details of that resource (id, name, disaster recovery and high availability attestation status and
        goal participation, exclusion reasons, provisioning state, the resource ARM id, and service group
        memberships). Omit the name to list all resources of the goal assignment, returning only their id and name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class GoalResourceGetCommand(ILogger<GoalResourceGetCommand> logger, IResilienceManagementService resilienceManagementService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<GoalResourceGetOptions, GoalResourceGetCommand.GoalResourceGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<GoalResourceGetCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalResourceGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            GoalResourceGetCommandResult result;
            if (string.IsNullOrEmpty(options.Name))
            {
                var goalResources = await _resilienceManagementService.ListGoalResourcesAsync(
                    options.ServiceGroup,
                    options.GoalAssignment,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new GoalResourceGetCommandResult(GoalResources: goalResources.ToList());
            }
            else
            {
                var goalResource = await _resilienceManagementService.GetGoalResourceAsync(
                    options.ServiceGroup,
                    options.GoalAssignment,
                    options.Name,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new GoalResourceGetCommandResult(GoalResource: goalResource);
            }

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.GoalResourceGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting goal resource(s). ServiceGroup: {ServiceGroup}, GoalAssignment: {GoalAssignment}, Name: {Name}, Subscription: {Subscription}.",
                options.ServiceGroup, options.GoalAssignment, options.Name, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Goal resource not found. Verify the goal resource name, goal assignment, service group, subscription, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the goal resource. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Goal resource not found. Verify the goal resource, goal assignment, and service group exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record GoalResourceGetCommandResult(List<ResourceSummary>? GoalResources = null, GoalResourceInfo? GoalResource = null);
}
