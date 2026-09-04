// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.MySql.Options;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.MySql.Commands;

[CommandMetadata(
    Id = "77e60b50-5c16-4879-96b1-6a40d9c08a37",
    Name = "list",
    Title = "List MySQL Resources",
    Description = "List MySQL servers, databases, or tables in your subscription. Returns all servers in the subscription by default, or servers in a resource group when --resource-group is specified. Specify --server to list databases on that server, or --server and --database to list tables in a specific database.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class MySqlListCommand(ILogger<MySqlListCommand> logger, IMySqlService mysqlService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<MySqlListOptions, MySqlListCommand.MySqlListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<MySqlListCommand> _logger = logger;
    private readonly IMySqlService _mysqlService = mysqlService;

    public override void ValidateOptions(MySqlListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // --user is required when performing data-plane operations (listing databases or tables)
        if (!string.IsNullOrEmpty(options.Server) && string.IsNullOrEmpty(options.User))
        {
            validationResult.Errors.Add("The --user parameter is required when --server is specified.");
        }

        // --server is required when --database is specified
        if (!string.IsNullOrEmpty(options.Database) && string.IsNullOrEmpty(options.Server))
        {
            validationResult.Errors.Add("The --server parameter is required when --database is specified.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, MySqlListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Route based on provided parameters
            if (!string.IsNullOrEmpty(options.Database))
            {
                // List tables in specified database
                TableListResult tableResult = await _mysqlService.GetTablesAsync(
                    options.Subscription!,
                    options.ResourceGroup ?? string.Empty,
                    options.User!,
                    options.Server!,
                    options.Database!,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(null, null, tableResult.Tables ?? [], tableResult.IsTruncated ? true : null),
                    MySqlJsonContext.Default.MySqlListCommandResult);
            }
            else if (!string.IsNullOrEmpty(options.Server))
            {
                // List databases on specified server
                List<string> databases = await _mysqlService.ListDatabasesAsync(
                    options.Subscription!,
                    options.ResourceGroup ?? string.Empty,
                    options.User!,
                    options.Server!,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(null, databases ?? [], null),
                    MySqlJsonContext.Default.MySqlListCommandResult);
            }
            else if (!string.IsNullOrEmpty(options.ResourceGroup))
            {
                // List servers scoped to a specific resource group
                List<string> servers = await _mysqlService.ListServersAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(servers ?? [], null, null),
                    MySqlJsonContext.Default.MySqlListCommandResult);
            }
            else
            {
                // List all servers in the subscription
                List<string> servers = await _mysqlService.ListServersInSubscriptionAsync(
                    options.Subscription!,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(servers ?? [], null, null),
                    MySqlJsonContext.Default.MySqlListCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Server: {Server}, Database: {Database}.", Name, options?.Subscription, options?.ResourceGroup, options?.Server, options?.Database);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record MySqlListCommandResult(List<string>? Servers, List<string>? Databases, List<string>? Tables, bool? TablesTruncated = null);
}
