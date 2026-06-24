// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Options.Database;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Database;

[CommandMetadata(
    Id = "14be1264-82c8-4a4c-8271-7cfe1fbebbc8",
    Name = "add",
    Title = "Add Database to App Service",
    Description = """
        Add a database connection for an App Service using connection string for an existing database. This command configures database connection
        settings for the specified App Service, allowing it to connect to a database server name. You must specify the App Service name, database name,
        database type, database server name, connection string, resource group name and subscription.
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DatabaseAddCommand(ILogger<DatabaseAddCommand> logger, IAppServiceService appServiceService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DatabaseAddOptions, DatabaseAddCommand.DatabaseAddResult>(subscriptionResolver)
{
    private readonly ILogger<DatabaseAddCommand> _logger = logger;
    private readonly IAppServiceService _appServiceService = appServiceService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DatabaseAddOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var connectionInfo = await _appServiceService.AddDatabaseAsync(
                options.App,
                options.ResourceGroup,
                options.DatabaseType,
                options.DatabaseServer,
                options.Database,
                options.ConnectionString ?? string.Empty, // connectionString - will be generated if not provided
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(connectionInfo), AppServiceJsonContext.Default.DatabaseAddResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add database connection to App Service '{App}'", options.App);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record DatabaseAddResult(DatabaseConnectionInfo ConnectionInfo);
}
