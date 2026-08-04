// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.Mcp.Tools.FunctionApp.Options.FunctionApp;
using Azure.Mcp.Tools.FunctionApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FunctionApp.Commands.FunctionApp;

[CommandMetadata(
    Id = "5249839c-a3c6-4f9e-b62b-afde801d95a6",
    Name = "get",
    Title = "Get Azure Function App Details",
    Description = """
        Gets Azure Function App details. Lists all Function Apps in the subscription or resource group.  If function app name and resource group
        is specified, retrieves the details of that specific function app.  Returns the details of Azure Function Apps, including its name,
        location, status, and app service plan name.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class FunctionAppGetCommand(ILogger<FunctionAppGetCommand> logger, IFunctionAppService functionAppService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FunctionAppGetOptions, FunctionAppGetCommand.FunctionAppGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<FunctionAppGetCommand> _logger = logger;
    private readonly IFunctionAppService _functionAppService = functionAppService;

    public override void ValidateOptions(FunctionAppGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrWhiteSpace(options.FunctionApp) && string.IsNullOrWhiteSpace(options.ResourceGroup))
        {
            validationResult.Errors.Add($"--function-app option requires --resource-group option to be specified.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FunctionAppGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var functionApps = await _functionAppService.GetFunctionApp(
                options.Subscription!,
                options.FunctionApp,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(functionApps ?? []), FunctionAppJsonContext.Default.FunctionAppGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.FunctionApp is null)
            {
                _logger.LogError(ex, "Error listing function apps. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}.",
                    options.Subscription, options.ResourceGroup);
            }
            else
            {
                _logger.LogError(ex,
                    "Error getting function app. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FunctionApp: {FunctionApp}.",
                    options.Subscription, options.ResourceGroup, options.FunctionApp);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Function App not found. Verify the app name, resource group, and subscription, and ensure you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the Function App. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record FunctionAppGetCommandResult(List<FunctionAppInfo> FunctionApps);
}
