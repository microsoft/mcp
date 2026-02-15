// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ServiceFabric.Options;
using Azure.Mcp.Tools.ServiceFabric.Options.ManagedCluster;
using Azure.Mcp.Tools.ServiceFabric.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ServiceFabric.Commands.ManagedCluster;

public sealed class ManagedClusterNodeGetCommand(ILogger<ManagedClusterNodeGetCommand> logger)
    : BaseServiceFabricCommand<ManagedClusterNodeGetOptions>
{
    private const string CommandTitle = "Get Service Fabric Managed Cluster Nodes";
    private readonly ILogger<ManagedClusterNodeGetCommand> _logger = logger;

    public override string Id => "a3f1b2c4-d5e6-47f8-9a0b-1c2d3e4f5a6b";

    public override string Name => "get";

    public override string Description =>
        "Get nodes for a Service Fabric managed cluster. Returns all nodes by default or a single node when a node name is specified. Includes name, node type, status, IP address, fault domain, upgrade domain, health state, and seed node status.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(ServiceFabricOptionDefinitions.Cluster.AsRequired());
        command.Options.Add(ServiceFabricOptionDefinitions.Node.AsOptional());
    }

    protected override ManagedClusterNodeGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ClusterName = parseResult.GetValueOrDefault<string>(ServiceFabricOptionDefinitions.Cluster.Name);
        options.NodeName = parseResult.GetValueOrDefault<string>(ServiceFabricOptionDefinitions.Node.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var service = context.GetService<IServiceFabricService>();

            if (!string.IsNullOrEmpty(options.NodeName))
            {
                var node = await service.GetManagedClusterNode(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.ClusterName!,
                    options.NodeName,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new ManagedClusterNodeGetCommandResult([node]), ServiceFabricJsonContext.Default.ManagedClusterNodeGetCommandResult);
            }
            else
            {
                var nodes = await service.ListManagedClusterNodes(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.ClusterName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new ManagedClusterNodeGetCommandResult(nodes ?? []), ServiceFabricJsonContext.Default.ManagedClusterNodeGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting Service Fabric managed cluster nodes. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Cluster: {Cluster}, Node: {Node}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options.ClusterName, options.NodeName, options);
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

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        HttpRequestException httpEx when httpEx.StatusCode.HasValue => httpEx.StatusCode.Value,
        _ => base.GetStatusCode(ex)
    };

    internal record ManagedClusterNodeGetCommandResult(List<Models.ManagedClusterNode> Nodes);
}
