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
    Id = "41daed5c-bf44-4cdf-9f3c-1df775465e53",
    Name = "sample",
    Title = "Sample Kusto Table Data",
    Description = "Return a sample of rows from a specific table in an Azure Data Explorer/Kusto/KQL cluster. Required: --cluster-uri (or --cluster and --subscription), --database, and --table.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SampleCommand(
    ILogger<SampleCommand> logger,
    IKustoService kustoService,
    ISubscriptionResolver subscriptionResolver)
    : BaseClusterCommand<SampleOptions, SampleCommand.SampleCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SampleOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Validate limit is within safe bounds to prevent resource abuse
            var safeLimit = Math.Clamp(options.Limit ?? 10, 1, 10000);

            var query = $"{KustoService.EscapeKqlIdentifier(options.Table)} | sample {safeLimit}";

            List<JsonElement> results;

            if (UseClusterUri(options))
            {
                results = await kustoService.QueryItemsAsync(
                    options.ClusterUri!,
                    options.Database,
                    query,
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
                    query,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(new SampleCommandResult(results ?? []), KustoJsonContext.Default.SampleCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred sampling table. Cluster: {Cluster}, Database: {Database}, Table: {Table}.", options.ClusterUri ?? options.Cluster, options.Database, options.Table);
            HandleException(context, ex);
        }
        return context.Response;
    }

    public record SampleCommandResult(List<JsonElement> Results);
}
