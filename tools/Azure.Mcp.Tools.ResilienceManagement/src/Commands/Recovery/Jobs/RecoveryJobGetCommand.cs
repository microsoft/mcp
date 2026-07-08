// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Jobs;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Jobs;

[CommandMetadata(
    Id = "c7e1b4a9-2d63-4f08-8b95-1a6c3f0d7e42",
    Name = "get",
    Title = "Get or List Resilience Recovery Jobs",
    Description = """
        Gets the recovery jobs of a resilience recovery plan. Provide a recovery job name to get the full
        details of that job. Omit the name to list all recovery jobs of the recovery plan, returning only their
        id and name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryJobGetCommand(ILogger<RecoveryJobGetCommand> logger, IResilienceManagementService resilienceManagementService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RecoveryJobGetOptions, RecoveryJobGetCommand.RecoveryJobGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<RecoveryJobGetCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryJobGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            RecoveryJobGetCommandResult result;
            if (string.IsNullOrEmpty(options.Name))
            {
                var recoveryJobs = await _resilienceManagementService.ListRecoveryJobsAsync(
                    options.ServiceGroup,
                    options.RecoveryPlan,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new RecoveryJobGetCommandResult(RecoveryJobs: recoveryJobs.ToList());
            }
            else
            {
                var recoveryJob = await _resilienceManagementService.GetRecoveryJobAsync(
                    options.ServiceGroup,
                    options.RecoveryPlan,
                    options.Name,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new RecoveryJobGetCommandResult(RecoveryJob: recoveryJob);
            }

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.RecoveryJobGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting recovery job(s). ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}, Name: {Name}, Subscription: {Subscription}.",
                options.ServiceGroup, options.RecoveryPlan, options.Name, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Recovery job not found. Verify the recovery job name, recovery plan, service group, subscription, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the recovery job. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery job not found. Verify the recovery job, recovery plan, and service group exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record RecoveryJobGetCommandResult(List<ResourceSummary>? RecoveryJobs = null, JsonElement RecoveryJob = default);
}
