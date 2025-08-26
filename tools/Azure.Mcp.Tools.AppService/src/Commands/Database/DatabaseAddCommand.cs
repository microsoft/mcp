// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.AppService.Models;
using AzureMcp.AppService.Options;
using AzureMcp.AppService.Options.Database;
using AzureMcp.AppService.Services;
using AzureMcp.Core.Commands;
using AzureMcp.Core.Models.Option;
using AzureMcp.Core.Services.Telemetry;
using Microsoft.Extensions.Logging;

namespace AzureMcp.AppService.Commands.Database;

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
        command.AddOption(AppServiceOptionDefinitions.AppServiceName);
        command.AddOption(AppServiceOptionDefinitions.DatabaseTypeOption);
        command.AddOption(AppServiceOptionDefinitions.DatabaseServerOption);
        command.AddOption(AppServiceOptionDefinitions.DatabaseNameOption);
        command.AddOption(AppServiceOptionDefinitions.ConnectionStringOption);
    }

    protected override DatabaseAddOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.AppName = parseResult.GetValueForOption(AppServiceOptionDefinitions.AppServiceName);
        options.DatabaseType = parseResult.GetValueForOption(AppServiceOptionDefinitions.DatabaseTypeOption);
        options.DatabaseServer = parseResult.GetValueForOption(AppServiceOptionDefinitions.DatabaseServerOption);
        options.DatabaseName = parseResult.GetValueForOption(AppServiceOptionDefinitions.DatabaseNameOption);
        options.ConnectionString = parseResult.GetValueForOption(AppServiceOptionDefinitions.ConnectionStringOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            context.Activity?.WithSubscriptionTag(options);

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
                AppServiceJsonContext.DatabaseAddCommandResult);
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
