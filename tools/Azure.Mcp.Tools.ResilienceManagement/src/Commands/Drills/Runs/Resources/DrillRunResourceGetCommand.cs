// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs.Resources;

[CommandMetadata(
    Id = "f3b7d2a8-1c64-4e90-9b25-6a0f8c3d5e74",
    Name = "get",
    Title = "Get or List Resilience Drill Run Resources",
    Description = """
        Gets the resources (targets) of a resilience drill run. Provide a drill run resource name to get the
        full details of that resource. Omit the name to list all resources of the drill run, returning only
        their id and name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillRunResourceGetCommand(ILogger<DrillRunResourceGetCommand> logger, IResilienceManagementService resilienceManagementService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DrillRunResourceGetOptions, DrillRunResourceGetCommand.DrillRunResourceGetCommandResult>(subscriptionResolver)
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
                    options.Subscription!,
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
                    options.Subscription!,
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
                "Error getting drill run resource(s). ServiceGroup: {ServiceGroup}, Drill: {Drill}, DrillRun: {DrillRun}, Name: {Name}, Subscription: {Subscription}.",
                options.ServiceGroup, options.Drill, options.DrillRun, options.Name, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Drill run resource not found. Verify the drill run resource name, drill run, drill, service group, subscription, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the drill run resource. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill run resource not found. Verify the drill run resource, drill run, and drill exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record DrillRunResourceGetCommandResult(List<ResourceSummary>? DrillRunResources = null, JsonElement DrillRunResource = default);
}
