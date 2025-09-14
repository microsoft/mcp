// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Sql.Commands;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options;
using Azure.Mcp.Tools.Sql.Options.Database;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.Database;

public sealed class DatabaseCreateCommand(ILogger<DatabaseCreateCommand> logger)
    : BaseDatabaseCommand<DatabaseCreateOptions>(logger)
{
    private const string CommandTitle = "Create SQL Database";

    private readonly Option<string> _skuNameOption = SqlOptionDefinitions.SkuNameOption;
    private readonly Option<string> _skuTierOption = SqlOptionDefinitions.SkuTierOption;
    private readonly Option<int> _skuCapacityOption = SqlOptionDefinitions.SkuCapacityOption;
    private readonly Option<string> _collationOption = SqlOptionDefinitions.CollationOption;
    private readonly Option<long> _maxSizeBytesOption = SqlOptionDefinitions.MaxSizeBytesOption;
    private readonly Option<string> _elasticPoolNameOption = SqlOptionDefinitions.ElasticPoolNameOption;
    private readonly Option<bool> _zoneRedundantOption = SqlOptionDefinitions.ZoneRedundantOption;
    private readonly Option<string> _readScaleOption = SqlOptionDefinitions.ReadScaleOption;

    public override string Name => "create";

    public override string Description =>
        """
        Create a new Azure SQL Database on an existing SQL Server. This command creates a database with configurable
        performance tiers, size limits, and other settings. Equivalent to 'az sql db create'.
        Returns the newly created database information including configuration details.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_skuNameOption);
        command.Options.Add(_skuTierOption);
        command.Options.Add(_skuCapacityOption);
        command.Options.Add(_collationOption);
        command.Options.Add(_maxSizeBytesOption);
        command.Options.Add(_elasticPoolNameOption);
        command.Options.Add(_zoneRedundantOption);
        command.Options.Add(_readScaleOption);
    }

    protected override DatabaseCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SkuName = parseResult.GetValueOrDefault(_skuNameOption);
        options.SkuTier = parseResult.GetValueOrDefault(_skuTierOption);
        options.SkuCapacity = parseResult.GetValueOrDefault(_skuCapacityOption);
        options.Collation = parseResult.GetValueOrDefault(_collationOption);
        options.MaxSizeBytes = parseResult.GetValueOrDefault(_maxSizeBytesOption);
        options.ElasticPoolName = parseResult.GetValueOrDefault(_elasticPoolNameOption);
        options.ZoneRedundant = parseResult.GetValueOrDefault(_zoneRedundantOption);
        options.ReadScale = parseResult.GetValueOrDefault(_readScaleOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var sqlService = context.GetService<ISqlService>();

            var database = await sqlService.CreateDatabaseAsync(
                options.Server!,
                options.Database!,
                options.ResourceGroup!,
                options.Subscription!,
                options.SkuName,
                options.SkuTier,
                options.SkuCapacity,
                options.Collation,
                options.MaxSizeBytes,
                options.ElasticPoolName,
                options.ZoneRedundant,
                options.ReadScale,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new DatabaseCreateResult(database),
                SqlJsonContext.Default.DatabaseCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating SQL database. Server: {Server}, Database: {Database}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Server, options.Database, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 409 =>
            "Database already exists with the specified name. Choose a different database name or use the update command.",
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "SQL server not found. Verify the server name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed creating the SQL database. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == 400 =>
            $"Invalid database configuration. Check your SKU, size, and other parameters. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record DatabaseCreateResult(SqlDatabase Database);
}
