// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
public sealed class TableSchemaCommand(ILogger<TableSchemaCommand> logger, IKustoService kustoService) : BaseTableCommand<TableSchemaOptions>
{
    private readonly ILogger<TableSchemaCommand> _logger = logger;
    private readonly IKustoService _kustoService = kustoService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            string tableSchema;

            if (UseClusterUri(options))
            {
                tableSchema = await _kustoService.GetTableSchemaAsync(
                    options.ClusterUri!,
                    options.Database!,
                    options.Table!,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy,
                    cancellationToken);
            }
            else
            {
                tableSchema = await _kustoService.GetTableSchemaAsync(
                    options.Subscription!,
                    options.ClusterName!,
                    options.Database!,
                    options.Table!,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy,
                    cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(new(tableSchema), KustoJsonContext.Default.TableSchemaCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred getting table schema. Cluster: {Cluster}, Table: {Table}.", options.ClusterUri ?? options.ClusterName, options.Table);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record TableSchemaCommandResult(string Schema);
}
