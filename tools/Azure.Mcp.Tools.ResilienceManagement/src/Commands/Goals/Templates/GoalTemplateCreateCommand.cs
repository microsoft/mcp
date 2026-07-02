// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Templates;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Templates;

[CommandMetadata(
    Id = "f1d6b3a8-7c25-4e90-9b14-6a2d8f0c5e37",
    Name = "create",
    Title = "Create or Update Resilience Goal Template",
    Description = """
        Creates or updates a resilience goal template in the specified service group with the given high
        availability and disaster recovery requirements and regional recovery point and time objectives, and
        returns the goal template information including id, name, goal type, provisioning state, recovery point
        and time objectives, and high availability and disaster recovery requirements.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class GoalTemplateCreateCommand(ILogger<GoalTemplateCreateCommand> logger, IResilienceManagementService resilienceManagementService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<GoalTemplateCreateOptions, GoalTemplateCreateCommand.GoalTemplateCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<GoalTemplateCreateCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalTemplateCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var goalTemplate = await _resilienceManagementService.CreateGoalTemplateAsync(
                options.ServiceGroup,
                options.GoalTemplate,
                options.GoalType,
                options.RequireHighAvailability,
                options.RequireDisasterRecovery,
                options.RegionalRecoveryPointObjective,
                options.RegionalRecoveryTimeObjective,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new GoalTemplateCreateCommandResult(goalTemplate),
                ResilienceManagementJsonContext.Default.GoalTemplateCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating goal template. ServiceGroup: {ServiceGroup}, GoalTemplate: {GoalTemplate}, Subscription: {Subscription}.",
                options.ServiceGroup, options.GoalTemplate, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the goal template. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Service group not found. Verify the service group exists and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record GoalTemplateCreateCommandResult(GoalTemplateInfo GoalTemplate);
}
