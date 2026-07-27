// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;

[CommandMetadata(
    Id = "d4a8f1c6-3b75-4e29-9c08-2f6b5d0a7e31",
    Name = "get",
    Title = "Get or List Resilience Recovery Plans",
    Description = """
        Gets resilience recovery plans in the specified service group. Provide a recovery plan name to get the
        full details of that plan (including its properties and provisioning state). Omit the name to list all
        recovery plans in the service group, returning only their id and name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryPlanGetCommand(ILogger<RecoveryPlanGetCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryPlanGetOptions, RecoveryPlanGetCommand.RecoveryPlanGetCommandResult>
{
    private readonly ILogger<RecoveryPlanGetCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPlanGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            RecoveryPlanGetCommandResult result;
            if (string.IsNullOrEmpty(options.Name))
            {
                var recoveryPlans = await _resilienceManagementService.ListRecoveryPlansAsync(
                    options.ServiceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new RecoveryPlanGetCommandResult(RecoveryPlans: recoveryPlans.ToList());
            }
            else
            {
                var recoveryPlan = await _resilienceManagementService.GetRecoveryPlanAsync(
                    options.ServiceGroup,
                    options.Name,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new RecoveryPlanGetCommandResult(RecoveryPlan: recoveryPlan);
            }

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.RecoveryPlanGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting recovery plan(s). ServiceGroup: {ServiceGroup}, Name: {Name}.",
                options.ServiceGroup, options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Recovery plan not found. Verify the recovery plan name, service group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the recovery plan. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery plan not found. Verify the recovery plan and service group exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record RecoveryPlanGetCommandResult(List<ResourceSummary>? RecoveryPlans = null, JsonElement RecoveryPlan = default);
}
