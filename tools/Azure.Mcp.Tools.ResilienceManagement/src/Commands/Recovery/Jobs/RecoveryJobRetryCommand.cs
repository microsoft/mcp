// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Jobs;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Jobs;

[CommandMetadata(
    Id = "e84ec6e8-94ad-4e09-af25-0fd323238f29",
    Name = "retry",
    Title = "Retry Resilience Recovery Job",
    Description = "Retries a failed resilience recovery job. This destructive operation re-executes the original recovery workflow using the existing recovery job and returns an operation ID for tracking. The job must currently be Failed.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryJobRetryCommand(
    ILogger<RecoveryJobRetryCommand> logger,
    IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryJobRetryOptions, RecoveryJobRetryResult>
{
    private readonly ILogger<RecoveryJobRetryCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(RecoveryJobRetryOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        RecoveryPlanValidation.ValidateServiceGroup(options.ServiceGroup, validationResult);
        RecoveryPlanValidation.ValidateName(options.RecoveryPlan, validationResult);
        RecoveryJobValidation.ValidateName(options.RecoveryJob, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryJobRetryOptions options, CancellationToken cancellationToken)
    {
        try
        {
            RecoveryJobRetryResult result = await _resilienceManagementService.RetryRecoveryJobAsync(options.ServiceGroup, options.RecoveryPlan, options.RecoveryJob, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, ResilienceManagementJsonContext.Default.RecoveryJobRetryResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying recovery job. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}, RecoveryJob: {RecoveryJob}.", options.ServiceGroup, options.RecoveryPlan, options.RecoveryJob);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException => "The recovery job retry request timed out. Check the recovery job before retrying again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.PreconditionFailed => "The recovery job cannot be retried because it is not in the Failed state.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict => "The recovery job retry conflicts with its current state or another active operation.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden => "Authorization failed retrying the recovery job. Verify you have permission to run recovery actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound => "Recovery job not found. Verify the recovery job, recoveryplan, and service group exist and you have access.",
        RequestFailedException => "The recovery job retry request failed. Verify the job is Failed and try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex is TimeoutException ? HttpStatusCode.GatewayTimeout : base.GetStatusCode(ex);
}
