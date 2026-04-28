// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Aks.Options;
using Azure.Mcp.Tools.Aks.Options.Nodepool;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Aks.Commands.Nodepool;

[CommandMetadata(
    Id = "9abb0904-2ffc-4aab-b4ea-fc454b6351b1",
    Name = "get",
    Title = "Get Azure Kubernetes Service (AKS) Node Pool Details",
    Description = "List/enumerate all AKS (Azure Kubernetes Service) node pools in a cluster. Get/retrieve/show the details of a specific node pool if a name is provided.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class NodepoolGetCommand(ILogger<NodepoolGetCommand> logger, IAksService aksService) : BaseAksCommand<NodepoolGetOptions>
{
    private readonly ILogger<NodepoolGetCommand> _logger = logger;
    private readonly IAksService _aksService = aksService;

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
        options.ResourceGroup ??= parseResult.GetValueOrDefault(OptionDefinitions.Common.ResourceGroup);
        options.ClusterName = parseResult.GetValueOrDefault(AksOptionDefinitions.Cluster);
        options.NodepoolName = parseResult.GetValueOrDefault(AksOptionDefinitions.Nodepool);
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
            var nodePools = await _aksService.GetNodePools(
                options.Subscription!,
                options.ResourceGroup!,
                options.ClusterName!,
                options.NodepoolName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(nodePools ?? []), AksJsonContext.Default.NodepoolGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting AKS node pool. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ClusterName: {ClusterName}, Nodepool: {Nodepool}.",
                options.Subscription, options.ResourceGroup, options.ClusterName, options.NodepoolName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record NodepoolGetCommandResult(List<Models.NodePool> NodePools);
}

