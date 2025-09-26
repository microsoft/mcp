// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Aks.Options;
using Azure.Mcp.Tools.Aks.Options.Nodepool;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Aks.Commands.Nodepool;

public sealed class NodepoolGetCommand(ILogger<NodepoolGetCommand> logger) : BaseAksCommand<NodepoolGetOptions>
{
    private const string CommandTitle = "Get Azure Kubernetes Service (AKS) Node Pool";
    private readonly ILogger<NodepoolGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        """
        Get or list Azure Kubernetes Service (AKS) node pools (agent pools) in a cluster. If a specific node pool name
        is provided, that node pool will be retrieved. Otherwise, all node pools will be listed in the specified cluster.
        Returns key configuration and status including size, count, OS, mode, autoscaling, and provisioning state.
        """;

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
        command.Options.Add(AksOptionDefinitions.Cluster.AsRequired());
        command.Options.Add(AksOptionDefinitions.Nodepool);
    }

    protected override NodepoolGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ClusterName = parseResult.GetValueOrDefault<string>(AksOptionDefinitions.Cluster.Name);
        options.NodepoolName = parseResult.GetValueOrDefault<string>(AksOptionDefinitions.Nodepool.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var aksService = context.GetService<IAksService>();
            var nodePools = await aksService.GetNodePools(
                options.Subscription!,
                options.ResourceGroup!,
                options.ClusterName!,
                options.NodepoolName,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(nodePools ?? []), AksJsonContext.Default.NodepoolGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting AKS node pool. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ClusterName: {ClusterName}, Nodepool: {Nodepool}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options.ClusterName, options.NodepoolName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record NodepoolGetCommandResult(List<Models.NodePool> NodePools);
}

