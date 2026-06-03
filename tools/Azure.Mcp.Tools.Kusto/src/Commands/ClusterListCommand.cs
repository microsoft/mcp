// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Kusto.Options;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Kusto.Commands;

[CommandMetadata(
    Id = "2cff1548-40c9-48ea-8548-6bfa91f2ea85",
    Name = "list",
    Title = "List Kusto Clusters",
    Description = "List/enumerate all Azure Data Explorer/Kusto/KQL clusters in a subscription.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ClusterListCommand(
    ILogger<ClusterListCommand> logger,
    IKustoService kustoService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ClusterListOptions, ClusterListCommand.ClusterListCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ClusterListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var clusterNames = await kustoService.ListClustersAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new ClusterListCommandResult(clusterNames?.Results ?? [], clusterNames?.AreResultsTruncated ?? false), KustoJsonContext.Default.ClusterListCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred listing Kusto clusters. Subscription: {Subscription}.", options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ClusterListCommandResult(List<string> Clusters, bool AreResultsTruncated);
}
