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
    Id = "0bd79f0b-c360-4c96-b3e0-02fce97dcc41",
    Name = "list",
    Title = "List Kusto Databases",
    Description = "List/enumerate all databases in an Azure Data Explorer/Kusto/KQL cluster. Required: --cluster-uri ( or --cluster and --subscription).",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DatabaseListCommand(
    ILogger<DatabaseListCommand> logger,
    IKustoService kustoService,
    ISubscriptionResolver subscriptionResolver)
    : BaseClusterCommand<DatabaseListOptions, DatabaseListCommand.DatabaseListCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DatabaseListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            List<string> databasesNames;

            if (UseClusterUri(options))
            {
                databasesNames = await kustoService.ListDatabasesAsync(
                    options.ClusterUri!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }
            else
            {
                databasesNames = await kustoService.ListDatabasesAsync(
                    options.Subscription!,
                    options.Cluster!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(new DatabaseListCommandResult(databasesNames ?? []), KustoJsonContext.Default.DatabaseListCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred listing databases. Cluster: {Cluster}.", options.ClusterUri ?? options.Cluster);
            HandleException(context, ex);
        }
        return context.Response;
    }

    public record DatabaseListCommandResult(List<string> Databases);
}
