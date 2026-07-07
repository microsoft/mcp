// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Kusto.Options;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Kusto.Commands;

[CommandMetadata(
    Id = "3cd1e5f1-3353-4029-99f8-1aaa566d05e4",
    Name = "list",
    Title = "List Kusto Tables",
    Description = "List/enumerate all tables in a specific Azure Data Explorer/Kusto/KQL database. Required: --cluster-uri (or --cluster and --subscription), --database.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TableListCommand(
    ILogger<TableListCommand> logger,
    IKustoService kustoService,
    ISubscriptionResolver subscriptionResolver)
    : BaseClusterCommand<TableListOptions, TableListCommand.TableListCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TableListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            List<string> tableNames;

            if (UseClusterUri(options))
            {
                tableNames = await kustoService.ListTablesAsync(
                    options.ClusterUri!,
                    options.Database,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }
            else
            {
                tableNames = await kustoService.ListTablesAsync(
                    options.Subscription!,
                    options.Cluster!,
                    options.Database,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(new TableListCommandResult(tableNames ?? []), KustoJsonContext.Default.TableListCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred listing tables. Cluster: {Cluster}, Database: {Database}.", options.ClusterUri ?? options.Cluster, options.Database);
            HandleException(context, ex);
        }
        return context.Response;
    }

    public record TableListCommandResult(List<string> Tables);
}
