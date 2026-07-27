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
    Id = "a3f1b2c4-d5e6-47f8-9a0b-1c2d3e4f5a6b",
    Name = "get",
    Title = "Get Service Fabric Managed Cluster Nodes",
    Description = "Get nodes for a Service Fabric managed cluster. Returns all nodes by default or a single node when a node name is specified. Includes name, node type, status, IP address, fault domain, upgrade domain, health state, and seed node status.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ManagedClusterNodeGetCommand(ILogger<ManagedClusterNodeGetCommand> logger, IServiceFabricService serviceFabricService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ManagedClusterNodeGetOptions, ManagedClusterNodeGetCommand.ManagedClusterNodeGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ManagedClusterNodeGetCommand> _logger = logger;
    private readonly IServiceFabricService _serviceFabricService = serviceFabricService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ManagedClusterNodeGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(options.Node))
            {
                var node = await _serviceFabricService.GetManagedClusterNode(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Cluster,
                    options.Node,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new([node]), ServiceFabricJsonContext.Default.ManagedClusterNodeGetCommandResult);
            }
            else
            {
                var nodes = await _serviceFabricService.ListManagedClusterNodes(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Cluster,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(nodes ?? []), ServiceFabricJsonContext.Default.ManagedClusterNodeGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting Service Fabric managed cluster nodes. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Cluster: {Cluster}, Node: {Node}.",
                options.Subscription, options.ResourceGroup, options.Cluster, options.Node);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.NotFound =>
            "Managed cluster, resource group, or node not found. Verify the names and that you have access.",
        HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the Service Fabric managed cluster. Details: {httpEx.Message}",
        HttpRequestException httpEx => httpEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record ManagedClusterNodeGetCommandResult(List<Models.ManagedClusterNode> Nodes);
}
