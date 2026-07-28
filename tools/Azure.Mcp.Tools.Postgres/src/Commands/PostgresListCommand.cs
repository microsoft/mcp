// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Postgres.Commands;

[CommandMetadata(
    Id = "8a12c3f4-2e5d-4b3a-9f2c-5e6d7f8a9b0c",
    Name = "list",
    Title = "List PostgreSQL Resources",
    Description = "List PostgreSQL servers, databases, or tables. Returns all servers in the subscription by default (optionally scoped to a --resource-group). Specify --server to list databases on that server, or --server and --database to list tables in a specific database. --user is required when --server is provided.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class PostgresListCommand(IPostgresService postgresService, ILogger<PostgresListCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<PostgresListOptions, PostgresListCommand.PostgresListCommandResult>(subscriptionResolver)
{
    private readonly IPostgresService _postgresService = postgresService;
    private readonly ILogger<PostgresListCommand> _logger = logger;

    public override void ValidateOptions(PostgresListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Validate that --server is provided when --database is specified
        if (!string.IsNullOrEmpty(options.Database) && string.IsNullOrEmpty(options.Server))
        {
            validationResult.Errors.Add("The --server parameter is required when --database is specified.");
        }

        // Validate that --user is provided when --server is specified
        if (!string.IsNullOrEmpty(options.Server) && string.IsNullOrEmpty(options.User))
        {
            validationResult.Errors.Add("The --user parameter is required when --server is specified.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PostgresListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Route based on provided parameters
            if (!string.IsNullOrEmpty(options.Database))
            {
                // List tables in specified database
                TableListResult tableResult = await _postgresService.ListTablesAsync(
                    options.AuthType!,
                    options.User!,
                    options.Password,
                    options.Server!,
                    options.Database!,
                    string.IsNullOrEmpty(options.Schema) ? "public" : options.Schema,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(null, null, tableResult.Tables ?? [], tableResult.IsTruncated ? true : null),
                    PostgresJsonContext.Default.PostgresListCommandResult);
            }
            else if (!string.IsNullOrEmpty(options.Server))
            {
                // List databases on specified server
                DatabaseListResult databaseResult = await _postgresService.ListDatabasesAsync(
                    options.AuthType!,
                    options.User!,
                    options.Password,
                    options.Server!,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(null, databaseResult.Databases ?? [], null, databaseResult.IsTruncated ? true : null),
                    PostgresJsonContext.Default.PostgresListCommandResult);
            }
            else
            {
                // List all servers in the subscription (optionally scoped to a resource group)
                List<string> servers = await _postgresService.ListServersAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(servers ?? [], null, null),
                    PostgresJsonContext.Default.PostgresListCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Server: {Server}, Database: {Database}.", Name, options?.Subscription, options?.ResourceGroup, options?.Server, options?.Database);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record PostgresListCommandResult(List<string>? Servers, List<string>? Databases, List<string>? Tables, bool? ResultsTruncated = null);
}
