// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Postgres.Commands;

public sealed class PostgresListCommand(ILogger<PostgresListCommand> logger) : BasePostgresCommand<BasePostgresOptions>(logger)
{
    public override string Id => "8a12c3f4-2e5d-4b3a-9f2c-5e6d7f8a9b0c";

    public override string Name => "list";

    public override string Description => "List PostgreSQL servers, databases, or tables in your subscription. Returns all servers by default. Specify --server to list databases on that server, or --server and --database to list tables in a specific database.";

    public override string Title => "List PostgreSQL Resources";

    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, Secret = false, LocalRequired = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(PostgresOptionDefinitions.ServerOptional);
        command.Options.Add(PostgresOptionDefinitions.DatabaseOptional);
        command.Options.Add(PostgresOptionDefinitions.AuthType);
        command.Options.Add(PostgresOptionDefinitions.Password);
        command.Validators.Add(result =>
        {
            // Validate that --server is provided when --database is specified
            if (!string.IsNullOrEmpty(result.GetValueOrDefault<string>(PostgresOptionDefinitions.DatabaseOptional.Name)) &&
                string.IsNullOrEmpty(result.GetValueOrDefault<string>(PostgresOptionDefinitions.ServerOptional.Name)))
            {
                result.AddError("The --server parameter is required when --database is specified.");
            }
        });
    }

    protected override BasePostgresOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Server = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.ServerOptional.Name);
        options.Database = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.DatabaseOptional.Name);
        options.AuthType = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.AuthType.Name);
        options.Password = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Password.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var options = BindOptions(parseResult);

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

                context.Response.Results = ResponseResult.Create(
                    new(null, null, tables ?? []),
                    PostgresJsonContext.Default.PostgresListCommandResult);
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

                context.Response.Results = ResponseResult.Create(
                    new(null, databases ?? [], null),
                    PostgresJsonContext.Default.PostgresListCommandResult);
            }
            else
            {
                // List servers in resource group
                List<string> servers = await postgresService.ListServersAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.User!,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(servers ?? [], null, null),
                    PostgresJsonContext.Default.PostgresListCommandResult);
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
