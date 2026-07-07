// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AppService.Options.Webapp;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp;

[CommandMetadata(
    Title = "Change an Azure App Service Web App Running State",
    Id = "8d9cd2af-cd79-4101-968b-501d9f0b217c",
    Name = "change-state",
    Description = """
        Updates the running state of an Azure App Service web app using one of the following states:
        
        - "start": Starts a stopped web app.
        - "stop": Stops a running web app.
        - "restart": Restarts a running web app.

        Restart has additional options to specify whether to perform a soft restart and whether to synchronously wait
        for the restart to complete before returning.

        Returns a message indicating the result of the operation.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false
)]
public sealed class WebappChangeStateCommand(ILogger<WebappChangeStateCommand> logger, IAppServiceService appServiceService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<WebappChangeStateOptions, WebappChangeStateCommand.WebappChangeStateResult>(subscriptionResolver)
{
    private readonly ILogger<WebappChangeStateCommand> _logger = logger;

    private static readonly HashSet<string> s_validStateChanges = new(StringComparer.OrdinalIgnoreCase)
    {
        "start",
        "stop",
        "restart"
    };

    public override void ValidateOptions(WebappChangeStateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!ValidateStateChange(options.StateChange, out var errorMessage))
        {
            validationResult.Errors.Add(errorMessage);
        }
        else
        {
            if (!"restart".Equals(options.StateChange, StringComparison.OrdinalIgnoreCase))
            {
                if (options.SoftRestart)
                {
                    validationResult.Errors.Add("soft-restart only applies for change-state 'restart'.");
                }
                if (options.WaitForCompletion)
                {
                    validationResult.Errors.Add("wait-for-completion only applies for change-state 'restart'.");
                }
            }
        }
    }

    internal static bool ValidateStateChange(string? stateChange, out string errorMessage)
    {
        if (string.IsNullOrEmpty(stateChange) || !s_validStateChanges.Contains(stateChange))
        {
            errorMessage = $"Invalid value '{stateChange}' for state change. Valid values are: start, stop, restart.";
            return false;
        }

        errorMessage = "";
        return true;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, WebappChangeStateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var stateChange = await appServiceService.ChangeWebAppStateAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.App,
                options.StateChange,
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
                _logger.LogError(ex, "Failed to restart the Web App '{App}' in subscription {Subscription} and resource group {ResourceGroup} (Soft Restart: {SoftRestart}, Wait For Completion: {WaitForCompletion})",
                    options.App, options.Subscription, options.ResourceGroup, options.SoftRestart, options.WaitForCompletion);
            }
            else
            {
                _logger.LogError(ex, "Failed to {StateChange} the Web App '{App}' in subscription {Subscription} and resource group {ResourceGroup}",
                    options.StateChange, options.App, options.Subscription, options.ResourceGroup);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record WebappChangeStateResult(string StateChangeStatus);
}
