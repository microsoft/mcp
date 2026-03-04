// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Options;
using Azure.Mcp.Tools.AppService.Options.Webapp;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp;

public sealed class WebappChangeStateCommand(ILogger<WebappChangeStateCommand> logger)
    : BaseAppServiceCommand<WebappChangeStateOptions>(resourceGroupRequired: true, appRequired: true)
{
    private const string CommandTitle = "Change an Azure App Service Web App Running State";
    private readonly ILogger<WebappChangeStateCommand> _logger = logger;
    public override string Id => "8d9cd2af-cd79-4101-968b-501d9f0b217c";
    public override string Name => "change-state";

    public override string Description =>
        """
        Updates the running state of an Azure App Service web app using one of the following states:
        
        - "start": Starts a stopped web app.
        - "stop": Stops a running web app.
        - "restart": Restarts a running web app.

        Restart has additional options to specify whether to perform a soft restart and whether to synchronously wait
        for the restart to complete before returning.

        Returns a message indicating the result of the operation.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        Secret = true,
        LocalRequired = false
    };

    private static readonly HashSet<string> ValidStateChanges = new(StringComparer.OrdinalIgnoreCase)
    {
        "start",
        "stop",
        "restart"
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AppServiceOptionDefinitions.StateChange);
        command.Options.Add(AppServiceOptionDefinitions.SoftRestart);
        command.Options.Add(AppServiceOptionDefinitions.WaitForCompletion);
        command.Validators.Add(result =>
        {
            var stateChange = result.GetValueOrDefault<string>(AppServiceOptionDefinitions.StateChange.Name);
            if (!ValidateStateChange(stateChange, out var errorMessage))
            {
                result.AddError(errorMessage);
            }
        });
    }

    internal static bool ValidateStateChange(string? stateChange, out string errorMessage)
    {
        if (string.IsNullOrEmpty(stateChange) || !ValidStateChanges.Contains(stateChange))
        {
            errorMessage = $"Invalid value for state change. Valid values are: {string.Join(", ", ValidStateChanges)}.";
            return false;
        }

        errorMessage = "";
        return true;
    }

    protected override WebappChangeStateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.StateChange = parseResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.StateChange.Name);
        options.SoftRestart = parseResult.GetValueOrDefault<bool>(AppServiceOptionDefinitions.SoftRestart.Name);
        options.WaitForCompletion = parseResult.GetValueOrDefault<bool>(AppServiceOptionDefinitions.WaitForCompletion.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
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
            var stateChange = await appServiceService.ChangeWebAppStateAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.AppName!,
                options.StateChange!,
                options.SoftRestart,
                options.WaitForCompletion,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(stateChange), AppServiceJsonContext.Default.WebappChangeStateResult);
        }
        catch (Exception ex)
        {
            if ("restart".Equals(options.StateChange, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError(ex, "Failed to restart the Web App '{AppName}' in subscription {Subscription} and resource group {ResourceGroup} (Soft Restart: {SoftRestart}, Wait For Completion: {WaitForCompletion})",
                    options.AppName, options.Subscription, options.ResourceGroup, options.SoftRestart, options.WaitForCompletion);
            }
            else
            {
                _logger.LogError(ex, "Failed to {StateChange} the Web App '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                    options.StateChange, options.AppName, options.Subscription, options.ResourceGroup);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record WebappChangeStateResult(string StateChangeStatus);
}
