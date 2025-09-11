// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.Mcp.Tools.FunctionApp.Options.FunctionApp;
using Azure.Mcp.Tools.FunctionApp.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.FunctionApp.Commands.FunctionApp;

public sealed class FunctionAppListCommand(ILogger<FunctionAppListCommand> logger)
    : BaseFunctionAppCommand<FunctionAppListOptions>()
{
    private const string CommandTitle = "List Azure Function Apps";
    private readonly ILogger<FunctionAppListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        """
        Lists all Azure Function Apps in a subscription.
        Returns a list of function app details including name, location, status, and app service plan name.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;

        var options = BindOptions(parseResult);

        try
        {
            var functionAppService = context.GetService<IFunctionAppService>();
            var functionApps = await functionAppService.ListFunctionApps(
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = functionApps?.Count > 0
                ? ResponseResult.Create(
                    new FunctionAppListCommandResult(functionApps),
                    FunctionAppJsonContext.Default.FunctionAppListCommandResult)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing function apps. Subscription: {Subscription}, Options: {@Options}",
                options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "Subscription not found. Verify the subscription ID and you have access.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the function app resources. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record FunctionAppListCommandResult(List<FunctionAppInfo> Results);
}
