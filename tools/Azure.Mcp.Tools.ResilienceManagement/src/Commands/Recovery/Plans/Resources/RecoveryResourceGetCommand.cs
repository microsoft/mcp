// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans.Resources;

[CommandMetadata(
    Id = "b9d2f74c-1a85-4e30-8c67-3f0b6a2d9e51",
    Name = "get",
    Title = "Get or List Resilience Recovery Resources",
    Description = """
        Gets the resources (members) of a resilience recovery plan. Provide a recovery resource name to get the
        full details of that resource. Omit the name to list all resources of the recovery plan, returning only
        their id and name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryResourceGetCommand(ILogger<RecoveryResourceGetCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryResourceGetOptions, RecoveryResourceGetCommand.RecoveryResourceGetCommandResult>
{
    private readonly ILogger<RecoveryResourceGetCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryResourceGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            RecoveryResourceGetCommandResult result;
            if (string.IsNullOrEmpty(options.Name))
            {
                var recoveryResources = await _resilienceManagementService.ListRecoveryResourcesAsync(
                    options.ServiceGroup,
                    options.RecoveryPlan,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new RecoveryResourceGetCommandResult(RecoveryResources: recoveryResources.ToList());
            }
            else
            {
                var recoveryResource = await _resilienceManagementService.GetRecoveryResourceAsync(
                    options.ServiceGroup,
                    options.RecoveryPlan,
                    options.Name,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new RecoveryResourceGetCommandResult(RecoveryResource: recoveryResource);
            }

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.RecoveryResourceGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting recovery resource(s). ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}, Name: {Name}.",
                options.ServiceGroup, options.RecoveryPlan, options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Recovery resource not found. Verify the recovery resource name, recovery plan, service group, subscription, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the recovery resource. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery resource not found. Verify the recovery resource, recovery plan, and service group exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record RecoveryResourceGetCommandResult(List<ResourceSummary>? RecoveryResources = null, JsonElement RecoveryResource = default);
}
