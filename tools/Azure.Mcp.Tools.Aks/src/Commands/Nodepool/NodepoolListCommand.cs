// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Aks.Options;
using Azure.Mcp.Tools.Aks.Options.Nodepool;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Aks.Commands.Nodepool;

public sealed class NodepoolListCommand(ILogger<NodepoolListCommand> logger) : BaseAksCommand<NodepoolListOptions>
{
    private const string CommandTitle = "List AKS Node Pools";
    private readonly ILogger<NodepoolListCommand> _logger = logger;

    private readonly Option<string> _clusterNameOption = AksOptionDefinitions.Cluster;

    public override string Name => "list";

    public override string Description =>
        """
        List all node pools for a specific Azure Kubernetes Service (AKS) cluster.
        Returns key node pool details including sizing, count, OS type, mode, and autoscaling settings.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.Options.Add(_clusterNameOption);
    }

    protected override NodepoolListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ClusterName = parseResult.GetValue(_clusterNameOption);
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
            var nodePools = await aksService.ListNodePools(
                options.Subscription!,
                options.ResourceGroup!,
                options.ClusterName!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = nodePools?.Count > 0 ?
                ResponseResult.Create(
                    new NodepoolListCommandResult(nodePools),
                    AksJsonContext.Default.NodepoolListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing AKS node pools. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ClusterName: {ClusterName}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options.ClusterName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "AKS cluster or node pools not found. Verify the cluster name, resource group, and subscription, and ensure you have access.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing AKS node pools. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record NodepoolListCommandResult(List<Models.NodePool> NodePools);
}

