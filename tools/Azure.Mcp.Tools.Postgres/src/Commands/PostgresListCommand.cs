// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Postgres.Commands;

public sealed class PostgresListCommand(ILogger<PostgresListCommand> logger) : BasePostgresCommand<PostgresListOptions>(logger)
{
    private readonly Option<string?> _serverOption = new("--server")
    {
        Description = "The PostgreSQL server to list databases from (optional)."
    };

    private readonly Option<string?> _databaseOption = new("--database")
    {
        Description = "The PostgreSQL database to list tables from (optional, requires --server)."
    };

    public override string Id => "8a12c3f4-2e5d-4b3a-9f2c-5e6d7f8a9b0c";

    public override string Name => "list";

    public override string Description => "List PostgreSQL servers, databases, or tables in your subscription. Returns all servers by default. Specify --server to list databases on that server, or --server and --database to list tables in a specific database.";

    public override string Title => "List PostgreSQL Resources";

    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_serverOption);
        command.Options.Add(_databaseOption);
        command.Options.Add(PostgresOptionDefinitions.AuthType);
        command.Options.Add(PostgresOptionDefinitions.Password);
    }

    protected override PostgresListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Server = parseResult.GetValue(_serverOption);
        options.Database = parseResult.GetValue(_databaseOption);
        options.AuthType = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.AuthType.Name);
        options.Password = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Password.Name);
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

            IPostgresService postgresService = context.GetService<IPostgresService>() ?? throw new InvalidOperationException("PostgreSQL service is not available.");

            // Route based on provided parameters
            if (!string.IsNullOrEmpty(options.Database))
            {
                // List tables in specified database
                List<string> tables = await postgresService.ListTablesAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.AuthType!,
                    options.User!,
                    options.Password,
                    options.Server!,
                    options.Database!,
                    cancellationToken);

                context.Response.Results = tables?.Count > 0 ?
                    ResponseResult.Create(
                        new PostgresListCommandResult(null, null, tables),
                        PostgresJsonContext.Default.PostgresListCommandResult) :
                    null;
            }
            else if (!string.IsNullOrEmpty(options.Server))
            {
                // List databases on specified server
                List<string> databases = await postgresService.ListDatabasesAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.AuthType!,
                    options.User!,
                    options.Password,
                    options.Server!,
                    cancellationToken);

                context.Response.Results = databases?.Count > 0 ?
                    ResponseResult.Create(
                        new PostgresListCommandResult(null, databases, null),
                        PostgresJsonContext.Default.PostgresListCommandResult) :
                    null;
            }
            else
            {
                // List servers in resource group
                List<string> servers = await postgresService.ListServersAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.User!,
                    cancellationToken);

                context.Response.Results = servers?.Count > 0 ?
                    ResponseResult.Create(
                        new PostgresListCommandResult(servers, null, null),
                        PostgresJsonContext.Default.PostgresListCommandResult) :
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

    public record PostgresListCommandResult(List<string>? Servers, List<string>? Databases, List<string>? Tables);
}
