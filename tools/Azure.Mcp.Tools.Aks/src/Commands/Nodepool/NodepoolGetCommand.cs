// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Aks.Options;
using Azure.Mcp.Tools.Aks.Options.Nodepool;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Aks.Commands.Nodepool;

public sealed class NodepoolGetCommand(ILogger<NodepoolGetCommand> logger) : BaseAksCommand<NodepoolGetOptions>
{
    private const string CommandTitle = "Get AKS Node Pool";
    private readonly ILogger<NodepoolGetCommand> _logger = logger;

    private readonly Option<string> _clusterNameOption = AksOptionDefinitions.Cluster;
    private readonly Option<string> _nodepoolNameOption = AksOptionDefinitions.Nodepool;

    public override string Name => "get";

    public override string Description =>
        """
        Get details for a specific node pool (agent pool) in an Azure Kubernetes Service (AKS) cluster.
        Returns key configuration and status including size, count, OS, mode, autoscaling, and provisioning state.
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
        command.Options.Add(_nodepoolNameOption);
    }

    protected override NodepoolGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ClusterName = parseResult.GetValue(_clusterNameOption);
        options.NodepoolName = parseResult.GetValue(_nodepoolNameOption);
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
            var nodePool = await aksService.GetNodePool(
                options.Subscription!,
                options.ResourceGroup!,
                options.ClusterName!,
                options.NodepoolName!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = nodePool is null ?
                null : ResponseResult.Create(
                    new NodepoolGetCommandResult(nodePool),
                    AksJsonContext.Default.NodepoolGetCommandResult);
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

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "AKS node pool not found. Verify the node pool name, cluster, resource group, and subscription, and ensure you have access.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the AKS node pool. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record NodepoolGetCommandResult(Models.NodePool NodePool);
}

