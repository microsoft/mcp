// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Kusto.Options;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Kusto.Commands;

[CommandMetadata(
    Id = "d1e22074-53ce-4eef-8596-0ea134a9e317",
    Name = "query",
    Title = "Query Kusto Database",
    Description = "Executes a query against an Azure Data Explorer/Kusto/KQL cluster to search for specific terms, retrieve records, or perform management operations. Required: --cluster-uri (or --cluster and --subscription), --database, and --query.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class QueryCommand(
    ILogger<QueryCommand> logger,
    IKustoService kustoService,
    ISubscriptionResolver subscriptionResolver)
    : BaseClusterCommand<QueryOptions, QueryCommand.QueryCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, QueryOptions options, CancellationToken cancellationToken)
    {
        try
        {
            List<JsonElement> results;

            if (UseClusterUri(options))
            {
                results = await kustoService.QueryItemsAsync(
                    options.ClusterUri!,
                    options.Database,
                    options.Query,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }
            else
            {
                results = await kustoService.QueryItemsAsync(
                    options.Subscription!,
                    options.Cluster!,
                    options.Database,
                    options.Query,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(new QueryCommandResult(results ?? []), KustoJsonContext.Default.QueryCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred querying Kusto. Cluster: {Cluster}, Database: {Database},"
            + " Query: {Query}", options.ClusterUri ?? options.Cluster, options.Database, options.Query);
            HandleException(context, ex);
        }
        return context.Response;
    }

    public record QueryCommandResult(List<JsonElement> Items);
}
