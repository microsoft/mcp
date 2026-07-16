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
    Id = "e5b71c93-4d8a-42f6-9b0c-3a7e1d6f8025",
    Name = "get",
    Title = "Get or List Resilience Goal Templates",
    Description = """
        Gets resilience goal templates in the specified service group. Provide a goal template name to get the
        full details of that template (id, name, goal type, provisioning state, recovery point and time
        objectives, and high availability and disaster recovery requirements). Omit the name to list all goal
        templates in the service group, returning only their id and name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class GoalTemplateGetCommand(ILogger<GoalTemplateGetCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<GoalTemplateGetOptions, GoalTemplateGetCommand.GoalTemplateGetCommandResult>
{
    private readonly ILogger<GoalTemplateGetCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GoalTemplateGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            GoalTemplateGetCommandResult result;
            if (string.IsNullOrEmpty(options.Name))
            {
                var goalTemplates = await _resilienceManagementService.ListGoalTemplatesAsync(
                    options.ServiceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new GoalTemplateGetCommandResult(GoalTemplates: goalTemplates.ToList());
            }
            else
            {
                var goalTemplate = await _resilienceManagementService.GetGoalTemplateAsync(
                    options.ServiceGroup,
                    options.Name,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new GoalTemplateGetCommandResult(GoalTemplate: goalTemplate);
            }

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.GoalTemplateGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting goal template(s). ServiceGroup: {ServiceGroup}, Name: {Name}.",
                options.ServiceGroup, options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Goal template not found. Verify the goal template name, service group, subscription, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the goal template. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Goal template not found. Verify the goal template and service group exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record GoalTemplateGetCommandResult(List<ResourceSummary>? GoalTemplates = null, GoalTemplateInfo? GoalTemplate = null);
}
