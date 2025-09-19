// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
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
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SqlOptionDefinitions.SkuNameOption);
        command.Options.Add(SqlOptionDefinitions.SkuTierOption);
        command.Options.Add(SqlOptionDefinitions.SkuCapacityOption);
        command.Options.Add(SqlOptionDefinitions.CollationOption);
        command.Options.Add(SqlOptionDefinitions.MaxSizeBytesOption);
        command.Options.Add(SqlOptionDefinitions.ElasticPoolNameOption);
        command.Options.Add(SqlOptionDefinitions.ZoneRedundantOption);
        command.Options.Add(SqlOptionDefinitions.ReadScaleOption);
    }

    protected override DatabaseCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SkuName = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.SkuName);
        options.SkuTier = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.SkuTier);
        options.SkuCapacity = parseResult.GetValueOrDefault<int?>(SqlOptionDefinitions.SkuCapacity);
        options.Collation = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.Collation);
        options.MaxSizeBytes = parseResult.GetValueOrDefault<long?>(SqlOptionDefinitions.MaxSizeBytes);
        options.ElasticPoolName = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.ElasticPoolName);
        options.ZoneRedundant = parseResult.GetValueOrDefault<bool?>(SqlOptionDefinitions.ZoneRedundant);
        options.ReadScale = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.ReadScale);
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

            context.Response.Results = ResponseResult.Create(new(database), SqlJsonContext.Default.DatabaseCreateResult);
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

    internal record DatabaseCreateResult(SqlDatabase Database);
}
