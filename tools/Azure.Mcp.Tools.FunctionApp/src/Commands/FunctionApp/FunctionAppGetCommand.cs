// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
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
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(FunctionAppOptionDefinitions.FunctionApp);

        command.Validators.Add(result =>
        {
            if (result.HasOptionResult(FunctionAppOptionDefinitions.FunctionApp.Name) && !result.HasOptionResult(OptionDefinitions.Common.ResourceGroup.Name))
            {
                result.AddError($"--{FunctionAppOptionDefinitions.FunctionApp.Name} option requires --{OptionDefinitions.Common.ResourceGroup.Name} option to be specified.");
            }
        });
    }

    protected override FunctionAppGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FunctionAppName = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.FunctionApp.Name);
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

            context.Response.Results = ResponseResult.Create(new(functionApps ?? []), FunctionAppJsonContext.Default.FunctionAppGetCommandResult);
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
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Function App not found. Verify the app name, resource group, and subscription, and ensure you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the Function App. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record FunctionAppGetCommandResult(List<FunctionAppInfo> FunctionApps);
}
