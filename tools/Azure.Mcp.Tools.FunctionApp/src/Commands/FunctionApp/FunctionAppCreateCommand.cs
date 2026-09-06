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
    Id = "a19eaab4-4822-41cb-a6ec-ffdc56405400",
    Name = "create",
    Title = "Create Azure Function App",
    Description = """
        Creates a new Azure Function App hosted on an App Service plan (Consumption, Flex Consumption, Premium, or App Service).
        Automatically provisions dependencies when they are omitted: the resource group, a Storage account, and an App Service plan.
        Applies sensible runtime, operating system, and SKU defaults. For Azure Container Apps hosting use 'functionapp containerapp create' instead.

        Required options:
        - subscription: Target Azure subscription (ID or name)
        - resource-group: Resource group (created if missing)
        - function-app: Globally unique Function App name
        - location: Azure region (e.g. eastus)

        Optional options:
        - app-service-plan: Existing App Service plan to use; if omitted a plan named '<function-app>-plan' is created.
        - plan-type: Hosting kind to create when a plan is needed (consumption|flex|premium|appservice). Default: consumption.
            * consumption -> Y1 (Dynamic)
            * flex -> FC1 (Flex Consumption, Linux only)
            * premium -> EP1 (Elastic Premium)
            * appservice -> B1 (Basic) unless overridden by --plan-sku
        - plan-sku: Explicit App Service plan SKU (e.g. B1, S1, P1v3). Overrides the SKU implied by --plan-type.
        - runtime: FUNCTIONS_WORKER_RUNTIME (dotnet|dotnet-isolated|node|python|java|powershell). Default: dotnet.
        - runtime-version: Runtime version; if omitted a default per runtime is applied (python 3.12, node 22, dotnet 8.0, java 17, powershell 7.4).
        - os: windows|linux. Default: windows unless the runtime or plan requires Linux (python, flex consumption).
        - storage-account: Existing or new Storage account name (auto-generated when omitted).
        - storage-auth-mode: managed-identity (default) or connection-string. With managed-identity the site gets a system-assigned identity
          and AzureWebJobsStorage__accountName is set; grant that identity the 'Storage Blob Data Owner' role on the storage account after
          creation. With connection-string, AzureWebJobsStorage is set using an access key.

        Returns the created functionApp (name, resourceGroupName, location, appServicePlanName, status, defaultHostName, operatingSystem, tags).
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class FunctionAppCreateCommand(ILogger<FunctionAppCreateCommand> logger, IFunctionAppService functionAppService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FunctionAppCreateOptions, FunctionAppCreateCommand.FunctionAppCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<FunctionAppCreateCommand> _logger = logger;
    private readonly IFunctionAppService _functionAppService = functionAppService;

    public override void ValidateOptions(FunctionAppCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        var nameError = FunctionAppValidation.ValidateFunctionAppNameLength(options.FunctionApp);
        if (nameError is not null)
        {
            validationResult.Errors.Add(nameError);
        }

        if (FunctionAppValidation.IsContainerAppPlanType(options.PlanType))
        {
            validationResult.Errors.Add("Container Apps hosting is not supported by this command. Use 'functionapp containerapp create' instead.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FunctionAppCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var functionApp = await _functionAppService.CreateFunctionApp(
                options.Subscription!,
                options.ResourceGroup,
                options.FunctionApp,
                options.Location,
                options.AppServicePlan,
                options.PlanType,
                options.PlanSku,
                options.Runtime,
                options.RuntimeVersion,
                options.OperatingSystem,
                options.StorageAccount,
                options.StorageAuthMode,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(functionApp), FunctionAppJsonContext.Default.FunctionAppCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating function app. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FunctionApp: {FunctionApp}, Location: {Location}.",
                options.Subscription, options.ResourceGroup, options.FunctionApp, options.Location);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Function App name already exists or conflicts with a resource in the resource group. Choose a different name or check the plan settings.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource group or App Service plan not found. Verify they exist and you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the Function App. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record FunctionAppCreateCommandResult(FunctionAppInfo FunctionApp);
}
