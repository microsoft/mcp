// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs.Resources;

[CommandMetadata(
    Id = "7884c2be-01f9-47e2-b497-81b900b7b1eb",
    Name = "get",
    Title = "Get or List Resilience Drill Run Resources",
    Description = """
        Lists all drill run resources (targets) for a named drill run of a drill in an Azure service
        group. Gets a drill run resource by resource name from that named drill run.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillRunResourceGetCommand(ILogger<DrillRunResourceGetCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillRunResourceGetOptions, DrillRunResourceGetCommand.DrillRunResourceGetCommandResult>
{
    private readonly ILogger<DrillRunResourceGetCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillRunResourceGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            DrillRunResourceGetCommandResult result;
            if (string.IsNullOrEmpty(options.Name))
            {
                var drillRunResources = await _resilienceManagementService.ListDrillRunResourcesAsync(
                    options.ServiceGroup,
                    options.Drill,
                    options.DrillRun,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new DrillRunResourceGetCommandResult(DrillRunResources: drillRunResources.ToList());
            }
            else
            {
                var drillRunResource = await _resilienceManagementService.GetDrillRunResourceAsync(
                    options.ServiceGroup,
                    options.Drill,
                    options.DrillRun,
                    options.Name,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new DrillRunResourceGetCommandResult(DrillRunResource: drillRunResource);
            }

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.DrillRunResourceGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting drill run resource(s). ServiceGroup: {ServiceGroup}, Drill: {Drill}, DrillRun: {DrillRun}, Name: {Name}.",
                options.ServiceGroup, options.Drill, options.DrillRun, options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Drill run resource not found. Verify the resource name, drill run, drill, service group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed getting the drill run resource. Verify that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill run resource not found. Verify the resource, drill run, drill, and service group exist and you have access.",
        RequestFailedException => "Failed to get the drill run resource. Verify the resource, drill run, drill, and service group, then retry.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillRunResourceGetCommandResult(List<ResourceSummary>? DrillRunResources = null, JsonElement DrillRunResource = default);
}
