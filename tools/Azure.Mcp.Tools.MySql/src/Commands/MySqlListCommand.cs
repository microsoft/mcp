// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.MySql.Options;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.MySql.Commands;

public sealed class MySqlListCommand(ILogger<MySqlListCommand> logger) : BaseMySqlCommand<MySqlDatabaseOptions>(logger)
{
    public override string Id => "77e60b50-5c16-4879-96b1-6a40d9c08a37";

    public override string Name => "list";

    public override string Description => "List MySQL servers, databases, or tables in your subscription. Returns all servers by default. Specify --server to list databases on that server, or --server and --database to list tables in a specific database.";

    public override string Title => "List MySQL Resources";

    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, Secret = false, LocalRequired = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(MySqlOptionDefinitions.ServerOptional);
        command.Options.Add(MySqlOptionDefinitions.DatabaseOptional);
    }

    protected override MySqlDatabaseOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Server = parseResult.GetValueOrDefault<string>(MySqlOptionDefinitions.ServerOptional.Name);
        options.Database = parseResult.GetValueOrDefault<string>(MySqlOptionDefinitions.DatabaseOptional.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        try
        {
            var options = BindOptions(parseResult);
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            // Validate that --server is provided when --database is specified
            if (!string.IsNullOrEmpty(options.Database) && string.IsNullOrEmpty(options.Server))
            {
                context.Response.Status = System.Net.HttpStatusCode.BadRequest;
                context.Response.Message = "The --server parameter is required when --database is specified.";
                return context.Response;
            }

            IMySqlService mysqlService = context.GetService<IMySqlService>() ?? throw new InvalidOperationException("MySQL service is not available.");

            // Route based on provided parameters
            if (!string.IsNullOrEmpty(options.Database))
            {
                // List tables in specified database
                List<string> tables = await mysqlService.GetTablesAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.User!,
                    options.Server!,
                    options.Database!,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new MySqlListCommandResult(null, null, tables ?? []),
                    MySqlJsonContext.Default.MySqlListCommandResult);
            }
            else if (!string.IsNullOrEmpty(options.Server))
            {
                // List databases on specified server
                List<string> databases = await mysqlService.ListDatabasesAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.User!,
                    options.Server!,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new MySqlListCommandResult(null, databases ?? [], null),
                    MySqlJsonContext.Default.MySqlListCommandResult);
            }
            else
            {
                // List servers in resource group
                List<string> servers = await mysqlService.ListServersAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.User!,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new MySqlListCommandResult(servers ?? [], null, null),
                    MySqlJsonContext.Default.MySqlListCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {@Options}", Name, BindOptions(parseResult));
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record MySqlListCommandResult(List<string>? Servers, List<string>? Databases, List<string>? Tables);
}
