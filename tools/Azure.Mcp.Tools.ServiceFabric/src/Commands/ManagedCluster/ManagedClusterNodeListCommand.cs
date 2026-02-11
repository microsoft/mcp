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

public sealed class ManagedClusterNodeListCommand(ILogger<ManagedClusterNodeListCommand> logger)
    : BaseServiceFabricCommand<ManagedClusterNodeListOptions>
{
    private const string CommandTitle = "List Service Fabric Managed Cluster Nodes";
    private readonly ILogger<ManagedClusterNodeListCommand> _logger = logger;

    public override string Id => "a3f1b2c4-d5e6-47f8-9a0b-1c2d3e4f5a6b";

    public override string Name => "list";

    public override string Description =>
        "List all nodes for a Service Fabric managed cluster. Returns node information including name, node type, status, IP address, fault domain, upgrade domain, health state, and seed node status.";

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
    }

    protected override ManagedClusterNodeListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ClusterName = parseResult.GetValueOrDefault<string>(ServiceFabricOptionDefinitions.Cluster.Name);
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
            var nodes = await service.ListManagedClusterNodes(
                options.Subscription!,
                options.ResourceGroup!,
                options.ClusterName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(nodes ?? []), ServiceFabricJsonContext.Default.ManagedClusterNodeListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Service Fabric managed cluster nodes. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Cluster: {Cluster}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options.ClusterName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.NotFound =>
            "Managed cluster or resource group not found. Verify the cluster name, resource group, and that you have access.",
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

    internal record ManagedClusterNodeListCommandResult(List<Models.ManagedClusterNode> Nodes);
}
