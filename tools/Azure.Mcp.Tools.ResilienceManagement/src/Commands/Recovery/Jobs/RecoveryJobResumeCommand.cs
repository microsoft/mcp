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
    Id = "260f87af-e70d-45fb-9458-3ecbc9543458",
    Name = "resume",
    Title = "Resume Resilience Recovery Job",
    Description = "Resumes or continues a paused resilience recovery job for a recoveryplan in an Azure service group. Use when asked to resume a paused recovery job, optionally with a description containing user input for the paused action. This destructive operation returns after the resume is accepted with an operation ID. Use recoveryjob get to monitor the existing job. The recovery job must currently be Paused.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryJobResumeCommand(
    ILogger<RecoveryJobResumeCommand> logger,
    IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryJobResumeOptions, RecoveryJobResumeResult>
{
    private readonly ILogger<RecoveryJobResumeCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(RecoveryJobResumeOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        RecoveryPlanValidation.ValidateServiceGroup(options.ServiceGroup, validationResult);
        RecoveryPlanValidation.ValidateName(options.RecoveryPlan, validationResult);
        RecoveryJobValidation.ValidateName(options.RecoveryJob, validationResult);
        if (options.Description?.Length > 100)
        {
            validationResult.Errors.Add("--description must not exceed 100 characters.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryJobResumeOptions options, CancellationToken cancellationToken)
    {
        try
        {
            RecoveryJobResumeResult result = await _resilienceManagementService.ResumeRecoveryJobAsync(options.ServiceGroup, options.RecoveryPlan, options.RecoveryJob, options.Description, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, ResilienceManagementJsonContext.Default.RecoveryJobResumeResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming recovery job. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}, RecoveryJob: {RecoveryJob}.", options.ServiceGroup, options.RecoveryPlan, options.RecoveryJob);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TimeoutException => "The recovery job resume request timed out. Check the recovery job before trying again.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.PreconditionFailed => "The recovery job cannot be resumed because it is not in the Paused state.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict => "The recovery job resume conflicts with its current state or another active operation.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden => "Authorization failed resuming the recovery job. Verify you have permission to run recovery actions in the service group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound => "Recovery job not found. Verify the recovery job, recoveryplan, and service group exist and you have access.",
        RequestFailedException => "The recovery job resume request failed. Verify the job is Paused and try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex is TimeoutException ? HttpStatusCode.GatewayTimeout : base.GetStatusCode(ex);
}
