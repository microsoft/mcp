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
    Id = "c7e8b8a4-9d71-4b91-8d15-7a4c2a5f3e02",
    Name = "create",
    Title = "Create Azure Function App on Container Apps",
    Description = """
        Creates a new Azure Function App hosted in Azure Container Apps. A Container Apps managed environment is created (or reused) and the
        Function App runs the official Azure Functions base image for the chosen runtime. For App Service, Consumption, Flex Consumption,
        or Premium hosting use 'functionapp create' instead.

        Required options:
        - subscription: Target Azure subscription (ID or name)
        - resource-group: Resource group (created if missing)
        - function-app: Globally unique Function App name
        - location: Azure region (e.g. eastus)

        Optional options:
        - runtime: FUNCTIONS_WORKER_RUNTIME (dotnet|dotnet-isolated|node|python|java|powershell). Default: dotnet.
        - runtime-version: Runtime version; if omitted a default per runtime is applied (python 3.12, node 22, dotnet 8.0, java 17, powershell 7.4).
        - storage-account: Existing or new Storage account name (auto-generated when omitted).
        - storage-auth-mode: managed-identity (default) or connection-string. With managed-identity the app gets a system-assigned identity
          and AzureWebJobsStorage__accountName is set; grant that identity the 'Storage Blob Data Owner' role on the storage account after
          creation. With connection-string, AzureWebJobsStorage is set using an access key.
        - container-apps-environment: Existing Container Apps managed environment name. When omitted one named '<function-app>-env' is created.

        Automatic defaults:
        - Container image: mcr.microsoft.com/azure-functions/<runtime>:4-<runtime><version>
        - Operating system: always Linux.
        - FUNCTIONS_EXTENSION_VERSION: always ~4.

        Returns the created functionApp (name, resourceGroupName, location, appServicePlanName, status, defaultHostName, operatingSystem, tags).
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class FunctionAppCreateContainerAppCommand(ILogger<FunctionAppCreateContainerAppCommand> logger, IFunctionAppService functionAppService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FunctionAppCreateContainerAppOptions, FunctionAppCreateContainerAppCommand.FunctionAppCreateContainerAppCommandResult>(subscriptionResolver)
{
    private readonly ILogger<FunctionAppCreateContainerAppCommand> _logger = logger;
    private readonly IFunctionAppService _functionAppService = functionAppService;

    public override void ValidateOptions(FunctionAppCreateContainerAppOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        var nameError = FunctionAppValidation.ValidateFunctionAppNameLength(options.FunctionApp);
        if (nameError is not null)
        {
            validationResult.Errors.Add(nameError);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FunctionAppCreateContainerAppOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var functionApp = await _functionAppService.CreateContainerAppFunctionApp(
                options.Subscription!,
                options.ResourceGroup,
                options.FunctionApp,
                options.Location,
                options.Runtime,
                options.RuntimeVersion,
                options.StorageAccount,
                options.StorageAuthMode,
                options.ContainerAppsEnvironment,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(functionApp), FunctionAppJsonContext.Default.FunctionAppCreateContainerAppCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating Container Apps-hosted function app. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FunctionApp: {FunctionApp}, Location: {Location}.",
                options.Subscription, options.ResourceGroup, options.FunctionApp, options.Location);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Container App name already exists or conflicts with a resource in the resource group. Choose a different name or check the environment settings.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource group or Container Apps environment not found. Verify they exist and you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the Container Apps-hosted Function App. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record FunctionAppCreateContainerAppCommandResult(FunctionAppInfo FunctionApp);
}
