// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FunctionApp.Options;

public static class FunctionAppOptionDefinitions
{
    public const string FunctionAppName = "function-app";
    public const string LocationName = "location";
    public const string AppServicePlanName = "app-service-plan";
    public const string PlanTypeName = "plan-type";
    public const string PlanSkuName = "plan-sku";
    public const string RuntimeName = "runtime";
    public const string RuntimeVersionName = "runtime-version";
    public const string OperatingSystemName = "os";
    public const string StorageAccountName = "storage-account";
    public const string ContainerAppsEnvironmentName = "container-apps-environment";

    public static readonly Option<string> FunctionApp = new(
        $"--{FunctionAppName}"
    )
    {
        Description = "The name of the Function App.",
        Required = false
    };

    public static readonly Option<string> Location = new(
        $"--{LocationName}"
    )
    {
        Description = "The Azure region for the Function App (e.g., eastus, westus2).",
        Required = true
    };

    public static readonly Option<string> AppServicePlan = new(
        $"--{AppServicePlanName}"
    )
    {
        Description = "The App Service plan name to use. If not supplied, a Consumption plan will be created automatically.",
        Required = false
    };

    public static readonly Option<string> PlanType = new(
        $"--{PlanTypeName}"
    )
    {
        Description = "The App Service plan type when creating a plan automatically. Values: consumption, flex, premium. Defaults to consumption.",
        Required = false
    };

    public static readonly Option<string> PlanSku = new(
        $"--{PlanSkuName}"
    )
    {
        Description = "The explicit App Service plan SKU (e.g., B1, S1, P1v3). Mutually exclusive with --plan-type. If provided and --app-service-plan omitted a dedicated plan using this SKU is created.",
        Required = false
    };

    public static readonly Option<string> Runtime = new(
        $"--{RuntimeName}"
    )
    {
        Description = "Function runtime worker. Examples: dotnet, node, python. Defaults to dotnet.",
        Required = false
    };

    public static readonly Option<string> RuntimeVersion = new(
        $"--{RuntimeVersionName}"
    )
    {
        Description = "Runtime version for the selected worker (e.g., node: 22, 20; python: 3.12). If omitted, a sensible default is used.",
        Required = false
    };

    public static readonly Option<string> OperatingSystem = new(
        $"--{OperatingSystemName}"
    )
    {
        Description = "Target operating system (windows|linux). Defaults to windows except when runtime/plan requires Linux (python, flex consumption, containerapp). Python and flex consumption are Linux only.",
        Required = false
    };

    public static readonly Option<string> StorageAccount = new(
        $"--{StorageAccountName}"
    )
    {
        Description = "The name of the Storage Account to use or create.",
        Required = false
    };

    public static readonly Option<string> ContainerAppsEnvironment = new(
        $"--{ContainerAppsEnvironmentName}"
    )
    {
        Description = "The name of the Container Apps environment to use or create.",
        Required = false
    };
}
