// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Resources;

[CommandMetadata(
    Id = "7a13b11a-3809-4d83-9490-2d2dd0a33508",
    Name = "get",
    Title = "Get or List Resilience Drill Resources and Targets",
    Description = """
        Lists or gets the drill resources — also called drill targets — for a resilience drill in a service
        group. List all drill resources or drill targets for a resilience drill, or get the complete details
        of a single drill resource or drill target by name, including its ARM properties. Requires the parent
        drill name.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillResourceGetCommand(ILogger<DrillResourceGetCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillResourceGetOptions, DrillResourceGetCommand.DrillResourceGetCommandResult>
{
    private readonly ILogger<DrillResourceGetCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillResourceGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            DrillResourceGetCommandResult result;
            if (string.IsNullOrEmpty(options.Name))
            {
                var drillResources = await _resilienceManagementService.ListDrillResourcesAsync(
                    options.ServiceGroup,
                    options.Drill,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new DrillResourceGetCommandResult(DrillResources: drillResources.ToList());
            }
            else
            {
                var drillResource = await _resilienceManagementService.GetDrillResourceAsync(
                    options.ServiceGroup,
                    options.Drill,
                    options.Name,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new DrillResourceGetCommandResult(DrillResource: drillResource);
            }

            context.Response.Results = ResponseResult.Create(
                result,
                ResilienceManagementJsonContext.Default.DrillResourceGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting drill resource(s). ServiceGroup: {ServiceGroup}, Drill: {Drill}, Name: {Name}.",
                options.ServiceGroup, options.Drill, options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Drill resource not found. Verify the drill resource name, drill, service group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the drill resource. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill resource not found. Verify the drill resource, drill, and service group exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillResourceGetCommandResult(List<ResourceSummary>? DrillResources = null, DrillResourceInfo? DrillResource = null);
}
