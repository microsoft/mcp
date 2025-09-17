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
        $"--{LocationName}",
        "The Azure region for the Function App (e.g., eastus, westus2).")
    {
        IsRequired = true
    };

    public static readonly Option<string> AppServicePlan = new(
        $"--{AppServicePlanName}",
        "The App Service plan name to use. If not supplied, a Consumption plan will be created automatically.")
    {
        IsRequired = false
    };

    public static readonly Option<string> PlanType = new(
        $"--{PlanTypeName}",
        "The App Service plan type when creating a plan automatically. Values: consumption, flex, premium. Defaults to consumption.")
    {
        IsRequired = false
    };

    public static readonly Option<string> PlanSku = new(
        $"--{PlanSkuName}",
        "The explicit App Service plan SKU (e.g., B1, S1, P1v3). Mutually exclusive with --plan-type. If provided and --app-service-plan omitted a dedicated plan using this SKU is created.")
    {
        IsRequired = false
    };

    public static readonly Option<string> Runtime = new(
        $"--{RuntimeName}",
        "Function runtime worker. Examples: dotnet, node, python. Defaults to dotnet.")
    {
        IsRequired = false
    };

    public static readonly Option<string> RuntimeVersion = new(
        $"--{RuntimeVersionName}",
        "Runtime version for the selected worker (e.g., node: 22, 20; python: 3.12). If omitted, a sensible default is used.")
    {
        IsRequired = false
    };

    public static readonly Option<string> OperatingSystem = new(
        $"--{OperatingSystemName}",
        "Target operating system (windows|linux). Defaults to windows except when runtime/plan requires Linux (python, flex consumption, containerapp). Python and flex consumption are Linux only.")
    {
        IsRequired = false
    };

    public static readonly Option<string> StorageAccount = new(
        $"--{StorageAccountName}",
        "The name of the Storage Account to use or create.")
    {
        IsRequired = false
    };

    public static readonly Option<string> ContainerAppsEnvironment = new(
        $"--{ContainerAppsEnvironmentName}",
        "The name of the Container Apps environment to use or create.")
    {
        IsRequired = false
    };
}
