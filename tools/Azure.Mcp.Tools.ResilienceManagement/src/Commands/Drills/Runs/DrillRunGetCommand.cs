// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;

[CommandMetadata(
    Id = "a75f5e6d-f60c-44d7-9dc3-96e49b27a432",
    Name = "get",
    Title = "Get or List Resilience Drill Runs",
    Description = """
        Lists all runs of a drill in an Azure service group, or gets the drill run by drill run name
        for that drill and service group.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillRunGetCommand(ILogger<DrillRunGetCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillRunGetOptions, DrillRunGetCommand.DrillRunGetCommandResult>
{
    private readonly ILogger<DrillRunGetCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillRunGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            DrillRunGetCommandResult result;
            if (string.IsNullOrEmpty(options.Name))
            {
                var drillRuns = await _resilienceManagementService.ListDrillRunsAsync(
                    options.ServiceGroup,
                    options.Drill,
                    options.Tenant,
                    cancellationToken);
                result = new DrillRunGetCommandResult(DrillRuns: drillRuns.ToList());
            }
            else
            {
                var drillRun = await _resilienceManagementService.GetDrillRunAsync(
                    options.ServiceGroup,
                    options.Drill,
                    options.Name,
                    options.Tenant,
                    cancellationToken);
                result = new DrillRunGetCommandResult(DrillRun: drillRun);
            }

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.DrillRunGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting drill run(s). ServiceGroup: {ServiceGroup}, Drill: {Drill}, Name: {Name}.",
                options.ServiceGroup, options.Drill, options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the drill run. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill run not found. Verify the drill run, drill, and service group exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillRunGetCommandResult(List<ResourceSummary>? DrillRuns = null, JsonElement DrillRun = default);
}
