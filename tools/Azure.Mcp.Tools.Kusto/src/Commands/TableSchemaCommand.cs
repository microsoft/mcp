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
    Id = "9a972c48-6797-49bb-9784-8063ad1f7e96",
    Name = "schema",
    Title = "Get Kusto Table Schema",
    Description = "Get/retrieve/show the schema of a specific table in an Azure Data Explorer/Kusto/KQL cluster. Required: --cluster-uri (or --cluster and --subscription), --database, and --table.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TableSchemaCommand(
    ILogger<TableSchemaCommand> logger,
    IKustoService kustoService,
    ISubscriptionResolver subscriptionResolver)
    : BaseClusterCommand<TableSchemaOptions, TableSchemaCommand.TableSchemaCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TableSchemaOptions options, CancellationToken cancellationToken)
    {
        try
        {
            string tableSchema;

            if (UseClusterUri(options))
            {
                tableSchema = await kustoService.GetTableSchemaAsync(
                    options.ClusterUri!,
                    options.Database,
                    options.Table,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }
            else
            {
                tableSchema = await kustoService.GetTableSchemaAsync(
                    options.Subscription!,
                    options.Cluster!,
                    options.Database,
                    options.Table,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(new TableSchemaCommandResult(tableSchema), KustoJsonContext.Default.TableSchemaCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred getting table schema. Cluster: {Cluster}, Table: {Table}.", options.ClusterUri ?? options.Cluster, options.Table);
            HandleException(context, ex);
        }
        return context.Response;
    }

    public record TableSchemaCommandResult(string Schema);
}
