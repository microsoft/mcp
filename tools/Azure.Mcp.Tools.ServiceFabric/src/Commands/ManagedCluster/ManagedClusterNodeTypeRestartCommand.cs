// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ServiceFabric.Options;
using Azure.Mcp.Tools.ServiceFabric.Options.ManagedCluster;
using Azure.Mcp.Tools.ServiceFabric.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ServiceFabric.Commands.ManagedCluster;

public sealed class ManagedClusterNodeTypeRestartCommand(ILogger<ManagedClusterNodeTypeRestartCommand> logger)
    : BaseServiceFabricCommand<ManagedClusterNodeTypeRestartOptions>
{
    private const string CommandTitle = "Restart Service Fabric Managed Cluster Nodes";
    private readonly ILogger<ManagedClusterNodeTypeRestartCommand> _logger = logger;

    public override string Id => "b4f2c3d5-e6f7-48a9-8b1c-2d3e4f5a6b7c";

    public override string Name => "restart";

    public override string Description =>
        "Restart nodes of a specific node type in a Service Fabric managed cluster. Requires the cluster name, node type, and list of node names to restart. Optionally specify the update type (Default or ByUpgradeDomain).";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(ServiceFabricOptionDefinitions.Cluster.AsRequired());
        command.Options.Add(ServiceFabricOptionDefinitions.NodeType.AsRequired());
        command.Options.Add(ServiceFabricOptionDefinitions.Nodes.AsRequired());
        command.Options.Add(ServiceFabricOptionDefinitions.UpdateType.AsOptional());
    }

    protected override ManagedClusterNodeTypeRestartOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ClusterName = parseResult.GetValueOrDefault<string>(ServiceFabricOptionDefinitions.Cluster.Name);
        options.NodeType = parseResult.GetValueOrDefault<string>(ServiceFabricOptionDefinitions.NodeType.Name);
        options.Nodes = parseResult.GetValueOrDefault<string[]>(ServiceFabricOptionDefinitions.Nodes.Name);
        options.UpdateType = parseResult.GetValueOrDefault<string>(ServiceFabricOptionDefinitions.UpdateType.Name);
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
            var response = await service.RestartManagedClusterNodes(
                options.Subscription!,
                options.ResourceGroup!,
                options.ClusterName!,
                options.NodeType!,
                options.Nodes!,
                options.UpdateType ?? "Default",
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ManagedClusterNodeTypeRestartCommandResult(response),
                ServiceFabricJsonContext.Default.ManagedClusterNodeTypeRestartCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error restarting Service Fabric managed cluster nodes. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Cluster: {Cluster}, NodeType: {NodeType}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options.ClusterName, options.NodeType, options);
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

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        HttpRequestException httpEx when httpEx.StatusCode.HasValue => httpEx.StatusCode.Value,
        _ => base.GetStatusCode(ex)
    };

    internal record ManagedClusterNodeTypeRestartCommandResult(Models.RestartNodeResponse Response);
}
