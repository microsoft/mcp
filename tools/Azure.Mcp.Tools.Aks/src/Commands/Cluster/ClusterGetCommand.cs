// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Aks.Options.Cluster;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Aks.Commands.Cluster;

[CommandMetadata(
    Id = "34e0d3d3-cbc5-4df8-8244-1439b97f3de5",
    Name = "get",
    Title = "Get Azure Kubernetes Service (AKS) Cluster Details",
    Description = "List/enumerate all AKS (Azure Kubernetes Service) clusters in a subscription. Get/retrieve/show the details of a specific cluster if a name is provided.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ClusterGetCommand(ILogger<ClusterGetCommand> logger, IAksService aksService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ClusterGetOptions, ClusterGetCommand.ClusterGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ClusterGetCommand> _logger = logger;
    private readonly IAksService _aksService = aksService;

    public override void ValidateOptions(ClusterGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ClusterGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var clusters = await _aksService.GetClusters(
                options.Subscription!,
                options.Cluster,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(clusters ?? []), AksJsonContext.Default.ClusterGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting AKS cluster. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ClusterName: {ClusterName}.",
                options.Subscription, options.ResourceGroup, options.Cluster);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ClusterGetCommandResult(List<Models.Cluster> Clusters);
}
