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
        Updates the running state of an Azure App Service web app. Restart operations can be soft and can synchronously wait for completion.
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

    public override void ValidateOptions(WebappChangeStateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.StateChange != WebappStateChange.Restart)
        {
            if (options.SoftRestart)
            {
                validationResult.Errors.Add("soft-restart only applies to restart operations.");
            }
            if (options.WaitForCompletion)
            {
                validationResult.Errors.Add("wait-for-completion only applies to restart operations.");
            }
        }
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
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(stateChange), AppServiceJsonContext.Default.WebappChangeStateResult);
        }
        catch (Exception ex)
        {
            if (options.StateChange == WebappStateChange.Restart)
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
