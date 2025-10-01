// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.MySql.Options.Server;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.MySql.Commands.Server;

public sealed class ServerConfigGetCommand(ILogger<ServerConfigGetCommand> logger) : BaseServerCommand<ServerConfigGetOptions>(logger)
{
    private const string CommandTitle = "Get MySQL Server Configuration";

    public override string Name => "get";

    public override string Description => "Retrieves comprehensive configuration details for the specified Azure Database for MySQL Flexible Server instance. This command provides insights into server settings, performance parameters, security configurations, and operational characteristics essential for database administration and optimization. Returns configuration data in JSON format including ServerName, Location, Version, SKU, StorageSizeGB, BackupRetentionDays, and GeoRedundantBackup properties.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            IMySqlService mysqlService = context.GetService<IMySqlService>() ?? throw new InvalidOperationException("MySQL service is not available.");
            string config = await mysqlService.GetServerConfigAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!);
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

    internal record ServerConfigGetCommandResult(string Configuration);
}
