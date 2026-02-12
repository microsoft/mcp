// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Tools.MySql.Options;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.MySql.Commands;

public sealed class MySqlListCommand(ILogger<MySqlListCommand> logger) : BaseMySqlCommand<MySqlListOptions>(logger)
{
    private readonly Option<string?> _serverOption = new("--server")
    {
        Description = "The MySQL server to list databases from (optional)."
    };

    private readonly Option<string?> _databaseOption = new("--database")
    {
        Description = "The MySQL database to list tables from (optional, requires --server)."
    };

    public override string Id => "77e60b50-5c16-4879-96b1-6a40d9c08a37";

    public override string Name => "list";

    public override string Description => "List MySQL servers, databases, or tables in your subscription. Returns all servers by default. Specify --server to list databases on that server, or --server and --database to list tables in a specific database.";

    public override string Title => "List MySQL Resources";

    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_serverOption);
        command.Options.Add(_databaseOption);
    }

    protected override MySqlListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Server = parseResult.GetValue(_serverOption);
        options.Database = parseResult.GetValue(_databaseOption);
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

                context.Response.Results = tables?.Count > 0 ?
                    ResponseResult.Create(
                        new MySqlListCommandResult(null, null, tables),
                        MySqlJsonContext.Default.MySqlListCommandResult) :
                    null;
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

                context.Response.Results = databases?.Count > 0 ?
                    ResponseResult.Create(
                        new MySqlListCommandResult(null, databases, null),
                        MySqlJsonContext.Default.MySqlListCommandResult) :
                    null;
            }
            else
            {
                // List servers in resource group
                List<string> servers = await mysqlService.ListServersAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.User!,
                    cancellationToken);

                context.Response.Results = servers?.Count > 0 ?
                    ResponseResult.Create(
                        new MySqlListCommandResult(servers, null, null),
                        MySqlJsonContext.Default.MySqlListCommandResult) :
                    null;
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
