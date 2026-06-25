// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;

[CommandMetadata(
    Id = "d5f8b1c3-6a27-4e94-8c50-2b7d9f0a3e16",
    Name = "get",
    Title = "Get or List Resilience Drill Runs",
    Description = """
        Gets the runs of a resilience drill. Provide a drill run name to get the full details of that run. Omit
        the name to list all runs of the drill, returning only their id and name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillRunGetCommand(ILogger<DrillRunGetCommand> logger, IResilienceManagementService resilienceManagementService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DrillRunGetOptions, DrillRunGetCommand.DrillRunGetCommandResult>(subscriptionResolver)
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
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                result = new DrillRunGetCommandResult(DrillRuns: drillRuns.ToList());
            }
            else
            {
                var drillRun = await _resilienceManagementService.GetDrillRunAsync(
                    options.ServiceGroup,
                    options.Drill,
                    options.Name,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
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
                "Error getting drill run(s). ServiceGroup: {ServiceGroup}, Drill: {Drill}, Name: {Name}, Subscription: {Subscription}.",
                options.ServiceGroup, options.Drill, options.Name, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Drill run not found. Verify the drill run name, drill, service group, subscription, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the drill run. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill run not found. Verify the drill run and drill exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record DrillRunGetCommandResult(List<ResourceSummary>? DrillRuns = null, JsonElement DrillRun = default);
}
