// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
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

    private readonly Option<string> _optionalFunctionAppNameOption = FunctionAppOptionDefinitions.OptionalFunctionApp;

    public override string Name => "get";

    public override string Description =>
        """
        Gets Azure Function App details. Lists all Function Apps in the subscription or resource group.  If function app name and resource group
        is specified, retrieves the details of that specific function app.  Returns the details of Azure Function Apps, including its name,
        location, status, and app service plan name.
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
        UseResourceGroup();
        command.Options.Add(_optionalFunctionAppNameOption);
        command.Validators.Add(result =>
        {
            if (result.HasOptionResult(_optionalFunctionAppNameOption) && !result.HasOptionResult(_resourceGroupOption))
            {
                result.AddError($"--{_optionalFunctionAppNameOption.Name} option requires --{_resourceGroupOption.Name} option to be specified.");
            }
        });
    }

    protected override FunctionAppGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FunctionAppName = parseResult.GetValue(_optionalFunctionAppNameOption);
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
            var functionApps = await service.GetFunctionApp(
                options.Subscription!,
                options.FunctionAppName!,
                options.ResourceGroup!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = functionApps is { Count: > 0 }
                ? ResponseResult.Create(new(functionApps), FunctionAppJsonContext.Default.FunctionAppGetCommandResult)
                : null;
        }
        catch (Exception ex)
        {
            if (options.FunctionAppName is null)
            {
                _logger.LogError(ex, "Error listing function apps. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                    options.Subscription, options.ResourceGroup, options);
            }
            else
            {
                _logger.LogError(ex,
                    "Error getting function app. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FunctionApp: {FunctionApp}, Options: {@Options}",
                    options.Subscription, options.ResourceGroup, options.FunctionAppName, options);
            }
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

    internal record FunctionAppGetCommandResult(List<FunctionAppInfo> FunctionApps);
}
