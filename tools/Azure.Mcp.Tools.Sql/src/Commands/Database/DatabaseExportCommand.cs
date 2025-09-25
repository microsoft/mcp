// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Sql.Commands;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options;
using Azure.Mcp.Tools.Sql.Options.Database;
using Azure.Mcp.Tools.Sql.Services;
using Azure.ResourceManager.Sql.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.Database;

public sealed class DatabaseExportCommand(ILogger<DatabaseExportCommand> logger)
    : BaseDatabaseCommand<DatabaseExportOptions>(logger)
{
    private const string CommandTitle = "Export SQL Database";

    public override string Name => "export";

    public override string Description =>
        """
        Export an Azure SQL Database to a BACPAC file in Azure Storage. This command creates a logical backup
        of the database schema and data that can be used for archiving or migration purposes. The export
        operation is equivalent to 'az sql db export'. Returns export operation information including status.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = true
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SqlOptionDefinitions.StorageUriOption);
        command.Options.Add(SqlOptionDefinitions.StorageKeyOption);
        command.Options.Add(SqlOptionDefinitions.StorageKeyTypeOption);
        command.Options.Add(SqlOptionDefinitions.AdminUserOption);
        command.Options.Add(SqlOptionDefinitions.AdminPasswordOption);
        command.Options.Add(SqlOptionDefinitions.AuthTypeOption);
    }

    protected override DatabaseExportOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.StorageUri = parseResult.GetValueOrDefault(SqlOptionDefinitions.StorageUriOption);
        options.StorageKey = parseResult.GetValueOrDefault(SqlOptionDefinitions.StorageKeyOption);
        options.StorageKeyType = parseResult.GetValueOrDefault(SqlOptionDefinitions.StorageKeyTypeOption);
        options.AdminUser = parseResult.GetValueOrDefault(SqlOptionDefinitions.AdminUserOption);
        options.AdminPassword = parseResult.GetValueOrDefault(SqlOptionDefinitions.AdminPasswordOption);
        options.AuthType = parseResult.GetValueOrDefault(SqlOptionDefinitions.AuthTypeOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        // Additional validation for export-specific parameters
        if (string.IsNullOrEmpty(options.StorageUri))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "Storage URI is required for database export.";
            return context.Response;
        }

        if (string.IsNullOrEmpty(options.StorageKey))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "Storage key is required for database export.";
            return context.Response;
        }

        if (string.IsNullOrEmpty(options.StorageKeyType))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "Storage key type is required for database export.";
            return context.Response;
        }

        if (string.IsNullOrEmpty(options.AdminUser))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "Administrator user is required for database export.";
            return context.Response;
        }

        if (string.IsNullOrEmpty(options.AdminPassword))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "Administrator password is required for database export.";
            return context.Response;
        }

        var validStorageKeyTypes = new[] { "StorageAccessKey", "SharedAccessKey" };
        if (!validStorageKeyTypes.Contains(options.StorageKeyType, StringComparer.OrdinalIgnoreCase))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = $"Invalid storage key type '{options.StorageKeyType}'. Valid values are: {string.Join(", ", validStorageKeyTypes)}";
            return context.Response;
        }

        if (!Uri.TryCreate(options.StorageUri, UriKind.Absolute, out _))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "Storage URI must be a valid absolute URI.";
            return context.Response;
        }

        if (!string.IsNullOrEmpty(options.AuthType))
        {
            var validAuthTypes = new[] { "SQL", "ADPassword", "ManagedIdentity" };
            if (!validAuthTypes.Contains(options.AuthType, StringComparer.OrdinalIgnoreCase))
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = $"Invalid authentication type '{options.AuthType}'. Valid values are: {string.Join(", ", validAuthTypes)}";
                return context.Response;
            }
        }

        try
        {
            var sqlService = context.GetService<ISqlService>();

            var exportResult = await sqlService.ExportDatabaseAsync(
                options.Server!,
                options.Database!,
                options.ResourceGroup!,
                options.Subscription!,
                options.StorageUri!,
                options.StorageKey!,
                options.StorageKeyType!,
                options.AdminUser!,
                options.AdminPassword!,
                options.AuthType,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new DatabaseExportResult(exportResult),
                SqlJsonContext.Default.DatabaseExportResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error exporting SQL database. Server: {Server}, Database: {Database}, ResourceGroup: {ResourceGroup}, StorageUri: {StorageUri}",
                options.Server, options.Database, options.ResourceGroup, options.StorageUri);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "SQL database or server not found. Verify the database name, server name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed exporting the SQL database. Verify you have appropriate permissions and the storage account is accessible. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Invalid export parameters. Check your storage URI, credentials, and database configuration. Details: {reqEx.Message}",
        ArgumentException argEx =>
            $"Invalid argument: {argEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record DatabaseExportResult(SqlDatabaseExportResult ExportResult);
}
