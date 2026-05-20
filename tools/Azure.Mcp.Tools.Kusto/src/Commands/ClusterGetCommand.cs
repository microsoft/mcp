// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Kusto.Models;
using Azure.Mcp.Tools.Kusto.Options;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Kusto.Commands;

[CommandMetadata(
    Id = "5fc5a42b-a7f6-4d4a-9517-a8e119752b7a",
    Name = "get",
    Title = "Get Kusto Cluster Details",
    Description = "Get/retrieve/show details for a specific Azure Data Explorer/Kusto/KQL cluster in a subscription. Not for listing multiple clusters. Required: --cluster and --subscription.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ClusterGetCommand(
    ILogger<ClusterGetCommand> logger,
    IKustoService kustoService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ClusterGetOptions, ClusterGetCommand.ClusterGetCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ClusterGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var cluster = await kustoService.GetClusterAsync(
                options.Subscription!,
                options.Cluster,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = cluster is null ?
                null : ResponseResult.Create(new ClusterGetCommandResult(cluster), KustoJsonContext.Default.ClusterGetCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred getting Kusto cluster details. Cluster: {Cluster}.", options.Cluster);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => $"Kusto cluster not found. Verify the cluster name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Kusto cluster not found. Verify the cluster name, resource group, and subscription, and ensure you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the Kusto cluster. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        KeyNotFoundException => HttpStatusCode.NotFound,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public record ClusterGetCommandResult(KustoClusterModel Cluster);
}
