// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.MySql.Options;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.MySql.Commands.Server;

[CommandMetadata(
    Id = "677cef4f-0eb1-4665-a3a2-89301a75c201",
    Name = "get",
    Title = "Get MySQL Server Configuration",
    Description = "Retrieves comprehensive configuration details for the specified Azure Database for MySQL Flexible Server instance. This command provides insights into server settings, performance parameters, security configurations, and operational characteristics essential for database administration and optimization. Returns configuration data in JSON format including ServerName, Location, Version, SKU, StorageSizeGB, BackupRetentionDays, and GeoRedundantBackup properties.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ServerConfigGetCommand(ILogger<ServerConfigGetCommand> logger, IMySqlService mysqlService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<MySqlServerOptions, ServerConfigGetCommand.ServerConfigGetCommandResult>(subscriptionResolver)
{
    private readonly IMySqlService _mysqlService = mysqlService;
    private readonly ILogger<ServerConfigGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, MySqlServerOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var config = await _mysqlService.GetServerConfigAsync(options.Subscription!, options.ResourceGroup, options.Server, cancellationToken);
            context.Response.Results = !string.IsNullOrEmpty(config) ?
                ResponseResult.Create(new(config), MySqlJsonContext.Default.ServerConfigGetCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred getting server configuration.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record ServerConfigGetCommandResult(string Configuration);
}
