// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ServiceFabric.Options.ManagedCluster;
using Azure.Mcp.Tools.ServiceFabric.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ServiceFabric.Commands.ManagedCluster;

[CommandMetadata(
    Id = "b4f2c3d5-e6f7-48a9-8b1c-2d3e4f5a6b7c",
    Name = "restart",
    Title = "Restart Service Fabric Managed Cluster Nodes",
    Description = "Restart nodes of a specific node type in a Service Fabric managed cluster. Requires the cluster name, node type, and list of node names to restart. Optionally specify the update type (Default or ByUpgradeDomain).",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ManagedClusterNodeTypeRestartCommand(ILogger<ManagedClusterNodeTypeRestartCommand> logger, IServiceFabricService serviceFabricService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ManagedClusterNodeTypeRestartOptions, ManagedClusterNodeTypeRestartCommand.ManagedClusterNodeTypeRestartCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ManagedClusterNodeTypeRestartCommand> _logger = logger;
    private readonly IServiceFabricService _serviceFabricService = serviceFabricService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ManagedClusterNodeTypeRestartOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _serviceFabricService.RestartManagedClusterNodes(
                options.Subscription!,
                options.ResourceGroup,
                options.Cluster,
                options.NodeType,
                options.Nodes,
                options.UpdateType ?? Models.UpdateType.Default,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(response),
                ServiceFabricJsonContext.Default.ManagedClusterNodeTypeRestartCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error restarting Service Fabric managed cluster nodes. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Cluster: {Cluster}, NodeType: {NodeType}.",
                options.Subscription, options.ResourceGroup, options.Cluster, options.NodeType);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.NotFound =>
            "Managed cluster, resource group, or node type not found. Verify the names and that you have access.",
        HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the Service Fabric managed cluster. Details: {httpEx.Message}",
        HttpRequestException httpEx => httpEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record ManagedClusterNodeTypeRestartCommandResult(Models.RestartNodeResponse Response);
}
