// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;

[CommandMetadata(
    Id = "f0b6d31a-2c84-4e79-9a51-7d3f8c0b2e65",
    Name = "get",
    Title = "Get or List Resilience Drills",
    Description = """
        Gets resilience drills in the specified service group. Provide a drill name to get the full details of
        that drill (including its identity, properties, and provisioning state). Omit the name to list all
        drills in the service group, returning only their id and name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillGetCommand(ILogger<DrillGetCommand> logger, IResilienceManagementService resilienceManagementService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DrillGetOptions, DrillGetCommand.DrillGetCommandResult>(subscriptionResolver)
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
                    options.Subscription!,
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
                    options.Subscription!,
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
                "Error getting drill(s). ServiceGroup: {ServiceGroup}, Name: {Name}, Subscription: {Subscription}.",
                options.ServiceGroup, options.Name, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Drill not found. Verify the drill name, service group, subscription, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the drill. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill not found. Verify the drill and service group exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record DrillGetCommandResult(List<ResourceSummary>? Drills = null, JsonElement Drill = default);
}
