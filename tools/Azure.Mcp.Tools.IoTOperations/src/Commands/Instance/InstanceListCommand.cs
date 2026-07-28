// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.IoTOperations.Models;
using Azure.Mcp.Tools.IoTOperations.Options.Instance;
using Azure.Mcp.Tools.IoTOperations.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.IoTOperations.Commands.Instance;

[CommandMetadata(
    Id = "e895665e-b913-4985-8f61-4b17a5422396",
    Name = "list",
    Title = "List IoT Operations Instances",
    Description = """
        Lists Azure IoT Operations instances in a subscription or resource group. Returns instance details including
        name, location, resource group, provisioning state, Azure IoT Operations version, description, and the
        associated schema registry resource ID. If a resource group is specified, only instances within that resource
        group are returned. Otherwise, all instances in the subscription are listed.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class InstanceListCommand(ILogger<InstanceListCommand> logger, IIoTOperationsService iotOperationsService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<InstanceListOptions, InstanceListCommand.InstanceListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<InstanceListCommand> _logger = logger;
    private readonly IIoTOperationsService _iotOperationsService = iotOperationsService;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        InstanceListOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var instances = await _iotOperationsService.ListInstancesAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(instances?.Results ?? [], instances?.AreResultsTruncated ?? false),
                IoTOperationsJsonContext.Default.InstanceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error listing IoT Operations instances. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}.",
                options.Subscription,
                options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource not found. Verify the subscription and resource group exist and you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record InstanceListCommandResult(
        List<IoTOperationsInstanceInfo> Instances,
        bool AreResultsTruncated);
}
