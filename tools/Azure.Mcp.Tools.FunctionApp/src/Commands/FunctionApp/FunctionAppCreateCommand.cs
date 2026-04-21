// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.FunctionApp.Models;
using Azure.Mcp.Tools.FunctionApp.Options;
using Azure.Mcp.Tools.FunctionApp.Options.FunctionApp;
using Azure.Mcp.Tools.FunctionApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FunctionApp.Commands.FunctionApp;

public sealed class FunctionAppCreateCommand(ILogger<FunctionAppCreateCommand> logger, IFunctionAppService functionAppService)
    : BaseFunctionAppCommand<FunctionAppCreateOptions>
{
    private const string CommandTitle = "Create Azure Function App";
    private readonly ILogger<FunctionAppCreateCommand> _logger = logger;
    private readonly IFunctionAppService _functionAppService = functionAppService;

    public override string Id => "a19eaab4-4822-41cb-a6ec-ffdc56405400";

    public override string Name => "create";

    public override string Description =>
    """
    Create a new Azure Function App hosted on an App Service plan (Consumption, Flex Consumption, Premium, or App Service).
    Automatically provisions dependencies when omitted (App Service plan and Storage account) and applies sensible runtime & SKU defaults.
    For Container Apps hosting, use 'functionapp containerapp create' instead.

    Required options:
    - subscription: Target Azure subscription (ID or name)
    - resource-group: Resource group (created if missing)
    - function-app: Globally unique Function App name
    - location: Azure region (e.g. eastus)

    Optional options:
    - app-service-plan: Use an existing App Service plan; if omitted one is created.
    - plan-type: Hosting kind to create when a plan is needed (consumption|flex|premium|appservice). Default: consumption.
        * consumption  -> Y1 (Dynamic)
        * flex / flexconsumption -> FC1 (FlexConsumption, Linux only)
        * premium / functionspremium -> EP1 (Elastic Premium)
        * appservice -> B1 (Basic) unless overridden by --plan-sku
    - plan-sku: Explicit App Service plan SKU (e.g. B1, S1, P1v3). Overrides --plan-type SKU selection.
    - runtime: FUNCTIONS_WORKER_RUNTIME (dotnet|dotnet-isolated|node|python|java|powershell). Default: dotnet.
    - runtime-version: Specific runtime version; if omitted a default per runtime is applied (see defaults below).
    - os: windows|linux. Default: windows unless runtime/plan requires linux (python, flex consumption). Python & flex consumption do not support Windows.
    - storage-account: Existing or new Storage account name (auto-generated when omitted).
    - storage-auth-mode: 'managed-identity' (default; enables system-assigned identity on the site and configures `AzureWebJobsStorage__accountName`, and for Flex Consumption sets `functionAppConfig.deployment.storage.authentication.type` to SystemAssignedIdentity) or 'connection-string' (uses an access key). With managed-identity you must grant the site's identity the `Storage Blob Data Owner` role on the storage account after creation; the role is not assigned automatically.

    Automatic resources & defaults:
    - Storage account: Created when not supplied (Standard_LRS, HTTPS only, blob public access disabled). Name pattern: <sanitized-functionapp>[random6]. With the default managed-identity auth mode, `AzureWebJobsStorage__accountName` is set; with --storage-auth-mode connection-string, `AzureWebJobsStorage` is set with the account key.
    - App Service plan: Auto-created when not provided (name: <function-app>-plan).
    - Linux vs Windows: Linux automatically enforced for python and flex consumption. Other runtimes default to Windows unless plan-type dictates Linux (flex) or runtime is python.
    - Explicit --os overrides default when compatible; incompatible combinations cause validation errors (e.g. --os windows with python or flex consumption).
    - Runtime version defaults (LinuxFxVersion when Linux):
        * python -> 3.12
        * node -> 22
        * dotnet -> 8.0
        * java -> 17.0
        * powershell -> 7.4
    - WEBSITE_NODE_DEFAULT_VERSION: Set to ~<major> for Windows Node apps when a version is supplied.
    - FUNCTIONS_EXTENSION_VERSION: Always ~4.

    Returns: functionApp object (name, resourceGroup, location, plan, state, defaultHostName, tags)
    """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FunctionAppOptionDefinitions.FunctionApp.AsRequired());
        command.Options.Add(FunctionAppOptionDefinitions.Location.AsRequired());
        command.Options.Add(FunctionAppOptionDefinitions.AppServicePlan);
        command.Options.Add(FunctionAppOptionDefinitions.PlanType);
        command.Options.Add(FunctionAppOptionDefinitions.PlanSku);
        command.Options.Add(FunctionAppOptionDefinitions.Runtime);
        command.Options.Add(FunctionAppOptionDefinitions.RuntimeVersion);
        command.Options.Add(FunctionAppOptionDefinitions.OperatingSystem);
        command.Options.Add(FunctionAppOptionDefinitions.StorageAccount);
        command.Options.Add(FunctionAppOptionDefinitions.StorageAuthMode);
    }

    protected override FunctionAppCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FunctionAppName = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.FunctionApp.Name);
        options.Location = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.Location.Name);
        options.AppServicePlan = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.AppServicePlan.Name);
        options.PlanType = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.PlanType.Name);
        options.PlanSku = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.PlanSku.Name);
        options.Runtime = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.Runtime.Name) ?? "dotnet";
        options.RuntimeVersion = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.RuntimeVersion.Name);
        options.OperatingSystem = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.OperatingSystem.Name);
        options.StorageAccount = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.StorageAccount.Name);
        options.StorageAuthMode = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.StorageAuthMode.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
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

            if (string.Equals(options.PlanType, "containerapp", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(options.PlanType, "containerapps", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = "Container Apps hosting is not supported by this command. Use 'functionapp containerapp create' instead.";
                return context.Response;
            }

            var result = await _functionAppService.CreateFunctionApp(
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
                options.StorageAuthMode,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

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
