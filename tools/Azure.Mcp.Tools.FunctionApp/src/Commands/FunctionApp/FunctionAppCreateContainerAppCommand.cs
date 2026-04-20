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

public sealed class FunctionAppCreateContainerAppCommand(ILogger<FunctionAppCreateContainerAppCommand> logger, IFunctionAppService functionAppService)
    : BaseFunctionAppCommand<FunctionAppCreateContainerAppOptions>
{
    private const string CommandTitle = "Create Azure Function App on Container Apps";
    private readonly ILogger<FunctionAppCreateContainerAppCommand> _logger = logger;
    private readonly IFunctionAppService _functionAppService = functionAppService;

    public override string Id => "c7e8b8a4-9d71-4b91-8d15-7a4c2a5f3e02";

    public override string Name => "create-containerapp";

    public override string Description =>
    """
    Create a new Azure Function App hosted in Azure Container Apps. A managed Container Apps environment and container app are created (or reused) using an official Azure Functions base image for the chosen runtime. For App Service / Consumption / Flex / Premium hosting, use 'functionapp create'.

    Required options:
    - subscription: Target Azure subscription (ID or name)
    - resource-group: Resource group (created if missing)
    - function-app: Globally unique Function App / Container App name
    - location: Azure region (e.g. eastus)

    Optional options:
    - runtime: FUNCTIONS_WORKER_RUNTIME (dotnet|dotnet-isolated|node|python|java|powershell). Default: dotnet.
    - runtime-version: Specific runtime version; if omitted a default per runtime is applied.
    - storage-account: Existing or new Storage account name (auto-generated when omitted).
    - storage-auth-mode: 'managed-identity' (default; enables system-assigned identity on the Container App and emits `AzureWebJobsStorage__accountName` + `AzureWebJobsStorage__credential=managedidentity` env vars) or 'connection-string' (uses an access key). With managed-identity you must grant the container app's identity the `Storage Blob Data Owner` role on the storage account after creation.
    - container-apps-environment: Existing Container Apps managed environment name. When omitted one is created named '<function-app>-env'.

    Automatic resources & defaults:
    - Storage account: Always created when not supplied (Standard_LRS, HTTPS only, blob public access disabled). Connection string injected as AzureWebJobsStorage.
    - Container Apps managed environment: Created when not supplied.
    - Container image: Official Azure Functions image for the runtime (mcr.microsoft.com/azure-functions/<runtime>:4).
    - Operating system: Always Linux.
    - Runtime version defaults:
        * python -> 3.12
        * node -> 22
        * dotnet / dotnet-isolated -> 8.0
        * java -> 17.0
        * powershell -> 7.4
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
        command.Options.Add(FunctionAppOptionDefinitions.Runtime);
        command.Options.Add(FunctionAppOptionDefinitions.RuntimeVersion);
        command.Options.Add(FunctionAppOptionDefinitions.StorageAccount);
        command.Options.Add(FunctionAppOptionDefinitions.StorageAuthMode);
        command.Options.Add(FunctionAppOptionDefinitions.ContainerAppsEnvironment);
    }

    protected override FunctionAppCreateContainerAppOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FunctionAppName = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.FunctionApp.Name);
        options.Location = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.Location.Name);
        options.Runtime = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.Runtime.Name) ?? "dotnet";
        options.RuntimeVersion = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.RuntimeVersion.Name);
        options.StorageAccount = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.StorageAccount.Name);
        options.StorageAuthMode = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.StorageAuthMode.Name);
        options.ContainerAppsEnvironment = parseResult.GetValueOrDefault<string>(FunctionAppOptionDefinitions.ContainerAppsEnvironment.Name);
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

            var result = await _functionAppService.CreateContainerAppFunctionApp(
                options.Subscription!,
                options.ResourceGroup!,
                options.FunctionAppName!,
                options.Location!,
                options.Runtime ?? "dotnet",
                options.RuntimeVersion,
                options.StorageAccount,
                options.StorageAuthMode,
                options.ContainerAppsEnvironment,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new FunctionAppCreateContainerAppCommandResult(result),
                FunctionAppJsonContext.Default.FunctionAppCreateContainerAppCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating Container Apps-hosted function app. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FunctionApp: {FunctionApp}, Options: {@Options}",
                options.Subscription, options.ResourceGroup, options.FunctionAppName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Container App name already exists or conflict in resource group. Choose a different name or check environment settings.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource group or container apps environment not found. Verify the resources exist and you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the Container App-hosted Function App. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record FunctionAppCreateContainerAppCommandResult(FunctionAppInfo FunctionApp);
}
