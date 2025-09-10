// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Options;
using Azure.Mcp.Tools.AppService.Options.Database;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppService.Commands.Database;

public sealed class DatabaseAddCommand(ILogger<DatabaseAddCommand> logger) : BaseAppServiceCommand<DatabaseAddOptions>()
{
    private const string CommandTitle = "Add Database to App Service";
    private readonly ILogger<DatabaseAddCommand> _logger = logger;

    public override string Name => "add";

    public override string Description =>
        """
        Add a database connection to an App Service. This command configures database connection
        settings for the specified App Service, allowing it to connect to a database server.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AppServiceOptionDefinitions.AppServiceName);
        command.Options.Add(AppServiceOptionDefinitions.DatabaseTypeOption);
        command.Options.Add(AppServiceOptionDefinitions.DatabaseServerOption);
        command.Options.Add(AppServiceOptionDefinitions.DatabaseNameOption);
        command.Options.Add(AppServiceOptionDefinitions.ConnectionStringOption);
    }

    protected override DatabaseAddOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.AppName = parseResult.GetValueOrDefault(AppServiceOptionDefinitions.AppServiceName);
        options.DatabaseType = parseResult.GetValueOrDefault(AppServiceOptionDefinitions.DatabaseTypeOption);
        options.DatabaseServer = parseResult.GetValueOrDefault(AppServiceOptionDefinitions.DatabaseServerOption);
        options.DatabaseName = parseResult.GetValueOrDefault(AppServiceOptionDefinitions.DatabaseNameOption);
        options.ConnectionString = parseResult.GetValueOrDefault(AppServiceOptionDefinitions.ConnectionStringOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        // Validate first, then bind
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var appServiceService = context.GetService<IAppServiceService>();
            var connectionInfo = await appServiceService.AddDatabaseAsync(
                options.AppName!,
                options.ResourceGroup!,
                options.DatabaseType!,
                options.DatabaseServer!,
                options.DatabaseName!,
                options.ConnectionString ?? string.Empty, // connectionString - will be generated if not provided
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new Result(connectionInfo),
                AppServiceJsonContext.Default.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add database connection to App Service '{AppName}'", options.AppName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record Result(DatabaseConnectionInfo ConnectionInfo);
}
