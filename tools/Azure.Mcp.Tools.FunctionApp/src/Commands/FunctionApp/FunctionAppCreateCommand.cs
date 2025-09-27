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

public sealed class FunctionAppCreateCommand(ILogger<FunctionAppCreateCommand> logger)
    : BaseFunctionAppCommand<FunctionAppCreateOptions>
{
    private const string CommandTitle = "Create Azure Function App";
    private readonly ILogger<FunctionAppCreateCommand> _logger = logger;

    private readonly Option<string> _functionAppNameOption = FunctionAppOptionDefinitions.FunctionApp;
    private readonly Option<string> _locationOption = FunctionAppOptionDefinitions.Location;
    private readonly Option<string> _appServicePlanOption = FunctionAppOptionDefinitions.AppServicePlan;
    private readonly Option<string> _planTypeOption = FunctionAppOptionDefinitions.PlanType;
    private readonly Option<string> _planSkuOption = FunctionAppOptionDefinitions.PlanSku;
    private readonly Option<string> _runtimeOption = FunctionAppOptionDefinitions.Runtime;
    private readonly Option<string> _runtimeVersionOption = FunctionAppOptionDefinitions.RuntimeVersion;
    private readonly Option<string> _osOption = FunctionAppOptionDefinitions.OperatingSystem;
    private readonly Option<string> _storageAccountOption = FunctionAppOptionDefinitions.StorageAccount;
    private readonly Option<string> _containerAppsEnvironmentOption = FunctionAppOptionDefinitions.ContainerAppsEnvironment;

    public override string Name => "create";

    public override string Description =>
    """
    Create a new Azure Function App in the specified resource group and region.
    Automatically provisions dependencies when omitted (App Service plan OR Container App managed environment + Container App, and a Storage account) and applies sensible runtime & SKU defaults.

    Required options:
    - subscription: Target Azure subscription (ID or name)
    - resource-group: Resource group (created if missing)
    - function-app: Globally unique Function App name
    - location: Azure region (e.g. eastus)

    Optional options:
    - app-service-plan: Use an existing App Service plan; if omitted one is created when hosting on App Service (non-container).
    - plan-type: Hosting kind to create when a plan is needed (consumption|flex|premium|appservice|containerapp). Default: consumption.
        * consumption  -> Y1   (Dynamic)
        * flex / flexconsumption -> FC1 (FlexConsumption, Linux only)
        * premium / functionspremium -> EP1 (Elastic Premium)
        * appservice   -> B1 (Basic) unless overridden by --plan-sku
    * containerapp -> Creates a Container App instead of an App Service plan/site (no plan created). Container App will reuse the function-app name.
    - plan-sku: Explicit App Service plan SKU (e.g. B1, S1, P1v3). Overrides --plan-type SKU selection (ignored for containerapp).
    - runtime: FUNCTIONS_WORKER_RUNTIME (dotnet|dotnet-isolated|node|python|java|powershell). Default: dotnet.
    - runtime-version: Specific runtime version; if omitted a default per runtime is applied (see defaults below).
    - os: windows|linux. Default: windows unless runtime/plan requires linux (python, flex consumption, containerapp). Overridden to linux automatically when required. Python & flex consumption do not support Windows.

    Automatic resources & defaults:
    - Storage account: Always created (Standard_LRS, HTTPS only, blob public access disabled). Name pattern: <sanitized-functionapp>[random6]. Connection string injected as AzureWebJobsStorage.
    - App Service plan: Auto-created when not provided (name: <function-app>-plan) unless containerapp hosting.
    - Container App: If containerapp hosting selected, a managed environment and container app are created using the function-app name and an official Azure Functions image for the runtime.
    - Linux vs Windows: Linux automatically enforced for python and flex consumption. Other runtimes default to Windows unless plan-type dictates Linux (flex) or runtime is python.
    - Explicit --os overrides default when compatible; incompatible combinations cause validation errors (e.g. --os windows with python or flex consumption).
    - Runtime version defaults (LinuxFxVersion when Linux):
        * python -> 3.12
        * node -> 22
        * dotnet -> 8.0
        * java -> 21.0
        * powershell -> 7.4
    - WEBSITE_NODE_DEFAULT_VERSION: Set to ~<major> for Windows Node apps when a version is supplied.
    - FUNCTIONS_EXTENSION_VERSION: Always ~4.

    Behavior notes:
    - Providing --plan-sku with --plan-type is allowed; SKU wins.
    - --container-app path skips App Service plan & site creation and provisions a Container App instead.
    - Invalid combination examples: specifying app-service-plan with plan-type containerapp.

    Returns: functionApp object (name, resourceGroup, location, plan, state, defaultHostName, tags)
    """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = true, ReadOnly = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(_functionAppNameOption);
        command.Options.Add(_locationOption);
        command.Options.Add(_appServicePlanOption);
        command.Options.Add(_planTypeOption);
        command.Options.Add(_planSkuOption);
        command.Options.Add(_runtimeOption);
        command.Options.Add(_runtimeVersionOption);
        command.Options.Add(_osOption);
        command.Options.Add(_storageAccountOption);
        command.Options.Add(_containerAppsEnvironmentOption);
    }

    protected override FunctionAppCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FunctionAppName = parseResult.GetValueOrDefault<string>(_functionAppNameOption.Name);
        options.Location = parseResult.GetValueOrDefault<string>(_locationOption.Name);
        options.AppServicePlan = parseResult.GetValueOrDefault<string>(_appServicePlanOption.Name);
        options.PlanType = parseResult.GetValueOrDefault<string>(_planTypeOption.Name);
        options.PlanSku = parseResult.GetValueOrDefault<string>(_planSkuOption.Name);
        options.Runtime = parseResult.GetValueOrDefault<string>(_runtimeOption.Name) ?? "dotnet";
        options.RuntimeVersion = parseResult.GetValueOrDefault<string>(_runtimeVersionOption.Name);
        options.OperatingSystem = parseResult.GetValueOrDefault<string>(_osOption.Name);
        options.StorageAccount = parseResult.GetValueOrDefault<string>(_storageAccountOption.Name);
        options.ContainerAppsEnvironment = parseResult.GetValueOrDefault<string>(_containerAppsEnvironmentOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
                return context.Response;

            if (!string.IsNullOrWhiteSpace(options.FunctionAppName))
            {
                var len = options.FunctionAppName.Length;
                if (len < 2 || len > 43)
                {
                    context.Response.Status = HttpStatusCode.BadRequest;
                    context.Response.Message = "function-app name must be between 2 and 43 characters.";
                    return context.Response;
                }
            }

            if (!string.IsNullOrWhiteSpace(options.AppServicePlan) && string.Equals(options.PlanType, "containerapp", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = "--app-service-plan cannot be combined with --plan-type containerapp.";
                return context.Response;
            }

            var service = context.GetService<IFunctionAppService>();
            var result = await service.CreateFunctionApp(
                options.Subscription!,
                options.ResourceGroup!,
                options.FunctionAppName!,
                options.Location!,
                options.AppServicePlan,
                options.PlanType,
                options.PlanSku,
                options.Runtime ?? "dotnet",
                options.RuntimeVersion,
                options.OperatingSystem,
                options.StorageAccount,
                options.ContainerAppsEnvironment,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new FunctionAppCreateCommandResult(result),
                FunctionAppJsonContext.Default.FunctionAppCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating function app. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FunctionApp: {FunctionApp}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options.FunctionAppName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Function App name already exists or conflict in resource group. Choose a different name or check plan settings.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource group or plan not found. Verify the resource group and plan exist and you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the Function App. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record FunctionAppCreateCommandResult(FunctionAppInfo FunctionApp);
}
