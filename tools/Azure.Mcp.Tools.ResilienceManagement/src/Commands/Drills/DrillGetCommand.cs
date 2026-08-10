// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;

[CommandMetadata(
    Id = "f64f7d2b-1d8c-4c26-a8d2-2bcf83d768d8",
    Name = "get",
    Title = "Get or List Resilience Drills",
    Description = """
        Gets resilience drills in the specified service group. Provide a drill name to get the full
        details of that drill. Omit the name to list all drills in the service group, returning only
        their id and name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillGetCommand(ILogger<DrillGetCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillGetOptions, DrillGetCommand.DrillGetCommandResult>
{
    private readonly ILogger<DrillGetCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            DrillGetCommandResult result;
            if (string.IsNullOrEmpty(options.Name))
            {
                var drills = await _resilienceManagementService.ListDrillsAsync(
                    options.ServiceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new DrillGetCommandResult(Drills: drills.ToList());
            }
            else
            {
                var drill = await _resilienceManagementService.GetDrillAsync(
                    options.ServiceGroup,
                    options.Name,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new DrillGetCommandResult(Drill: drill);
            }

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.DrillGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting drill(s). ServiceGroup: {ServiceGroup}, Name: {Name}.",
                options.ServiceGroup, options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Drill not found. Verify the drill name, service group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the drill. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill not found. Verify the drill and service group exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillGetCommandResult(List<ResourceSummary>? Drills = null, DrillInfo? Drill = null);
}