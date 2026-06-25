// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Resources;

[CommandMetadata(
    Id = "a4c8e1f2-7b39-4d06-8e12-5c9a0d4b6f31",
    Name = "get",
    Title = "Get or List Resilience Drill Resources",
    Description = """
        Gets the resources (targets) of a resilience drill. Provide a drill resource name to get the full
        details of that resource. Omit the name to list all resources of the drill, returning only their id
        and name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillResourceGetCommand(ILogger<DrillResourceGetCommand> logger, IResilienceManagementService resilienceManagementService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DrillResourceGetOptions, DrillResourceGetCommand.DrillResourceGetCommandResult>(subscriptionResolver)
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
                    options.Subscription!,
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
                    options.Subscription!,
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
                "Error getting drill resource(s). ServiceGroup: {ServiceGroup}, Drill: {Drill}, Name: {Name}, Subscription: {Subscription}.",
                options.ServiceGroup, options.Drill, options.Name, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Drill resource not found. Verify the drill resource name, drill, service group, subscription, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed getting the drill resource. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill resource not found. Verify the drill resource and drill exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record DrillResourceGetCommandResult(List<ResourceSummary>? DrillResources = null, JsonElement DrillResource = default);
}
