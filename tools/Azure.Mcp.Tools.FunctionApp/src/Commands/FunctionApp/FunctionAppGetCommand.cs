// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.Mcp.Tools.FunctionApp.Options;
using Azure.Mcp.Tools.FunctionApp.Options.FunctionApp;
using Azure.Mcp.Tools.FunctionApp.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.FunctionApp.Commands.FunctionApp;

public sealed class FunctionAppGetCommand(ILogger<FunctionAppGetCommand> logger)
    : BaseFunctionAppCommand<FunctionAppGetOptions>()
{
    private const string CommandTitle = "Get Azure Function App Details";
    private readonly ILogger<FunctionAppGetCommand> _logger = logger;

    private readonly Option<string> _functionAppNameOption = FunctionAppOptionDefinitions.FunctionApp;

    public override string Name => "get";

    public override string Description =>
        """
        Get the Azure Function App details for the specified function app name and resource group.
        This command retrieves the details of a specific Azure Function App, including its name, location, status, and app service plan name.
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

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.Options.Add(_functionAppNameOption);
    }

    protected override FunctionAppGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FunctionAppName = parseResult.GetValue(_functionAppNameOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;

        var options = BindOptions(parseResult);

        try
        {
            var service = context.GetService<IFunctionAppService>();
            var functionApp = await service.GetFunctionApp(
                options.Subscription!,
                options.FunctionAppName!,
                options.ResourceGroup!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = functionApp is null
                ? null
                : ResponseResult.Create(
                    new FunctionAppGetCommandResult(functionApp),
                    FunctionAppJsonContext.Default.FunctionAppGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting function app. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FunctionApp: {FunctionApp}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options.FunctionAppName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "Function App not found. Verify the app name, resource group, and subscription, and ensure you have access.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the Function App. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record FunctionAppGetCommandResult(FunctionAppInfo FunctionApp);
}
