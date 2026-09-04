// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.MySql.Options.Table;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.MySql.Commands.Table;

[CommandMetadata(
    Id = "1c8d2584-fa52-4641-85f9-fb67a8f5c7c9",
    Name = "get",
    Title = "Get MySQL Table Schema",
    Description = "Retrieves detailed schema information for a specific table within an Azure Database for MySQL Flexible Server database. This command provides comprehensive metadata including column definitions, data types, constraints, indexes, and relationships, essential for understanding table structure and supporting application development.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TableSchemaGetCommand(ILogger<TableSchemaGetCommand> logger, IMySqlService mysqlService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TableSchemaGetOptions, TableSchemaGetCommand.TableSchemaGetCommandResult>(subscriptionResolver)
{
    private readonly IMySqlService _mysqlService = mysqlService;
    private readonly ILogger<TableSchemaGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TableSchemaGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var schema = await _mysqlService.GetTableSchemaAsync(options.Subscription!, options.ResourceGroup, options.User, options.Server, options.Database, options.Table, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(schema ?? []), MySqlJsonContext.Default.TableSchemaGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred retrieving table schema.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record TableSchemaGetCommandResult(List<string> Schema);
}
