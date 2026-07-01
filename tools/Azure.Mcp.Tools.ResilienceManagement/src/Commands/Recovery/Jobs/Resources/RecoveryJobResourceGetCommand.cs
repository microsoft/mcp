// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Jobs.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Jobs.Resources;

[CommandMetadata(
    Id = "a1d6f3c8-7b94-4e25-9c08-2f5b7d0a3e69",
    Name = "get",
    Title = "Get or List Resilience Recovery Job Resources",
    Description = """
        Gets the resources (targets) of a resilience recovery job. Provide a recovery job resource name to get
        the full details of that resource. Omit the name to list all resources of the recovery job, returning
        only their id and name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryJobResourceGetCommand(ILogger<RecoveryJobResourceGetCommand> logger, IResilienceManagementService resilienceManagementService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RecoveryJobResourceGetOptions, RecoveryJobResourceGetCommand.RecoveryJobResourceGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<RecoveryJobResourceGetCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryJobResourceGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            RecoveryJobResourceGetCommandResult result;
            if (string.IsNullOrEmpty(options.Name))
            {
                var recoveryJobResources = await _resilienceManagementService.ListRecoveryJobResourcesAsync(
                    options.ServiceGroup,
                    options.RecoveryPlan,
                    options.RecoveryJob,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new RecoveryJobResourceGetCommandResult(RecoveryJobResources: recoveryJobResources.ToList());
            }
            else
            {
                var recoveryJobResource = await _resilienceManagementService.GetRecoveryJobResourceAsync(
                    options.ServiceGroup,
                    options.RecoveryPlan,
                    options.RecoveryJob,
                    options.Name,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new RecoveryJobResourceGetCommandResult(RecoveryJobResource: recoveryJobResource);
            }

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.RecoveryJobResourceGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting recovery job resource(s). ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}, RecoveryJob: {RecoveryJob}, Name: {Name}, Subscription: {Subscription}.",
                options.ServiceGroup, options.RecoveryPlan, options.RecoveryJob, options.Name, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Recovery job resource not found. Verify the recovery job resource name, recovery job, recovery plan, service group, subscription, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the recovery job resource. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery job resource not found. Verify the recovery job resource, recovery job, recovery plan, and service group exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record RecoveryJobResourceGetCommandResult(List<ResourceSummary>? RecoveryJobResources = null, JsonElement RecoveryJobResource = default);
}
