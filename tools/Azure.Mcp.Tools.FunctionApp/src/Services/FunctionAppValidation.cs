// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;

namespace Azure.Mcp.Tools.FunctionApp.Services;

public enum HostingKind
{
    Consumption,
    FlexConsumption,
    Premium,
    AppService,
    ContainerApp
}

public readonly record struct NormalizedInputs(
    string Runtime,
    string? RuntimeVersion,
    string? PlanType,
    string? PlanSku,
    string? OperatingSystem,
    string? StorageAccountName,
    string? ContainerAppsEnvironmentName);

public readonly record struct CreateOptions(
    string Runtime,
    string? RuntimeVersion,
    HostingKind HostingKind,
    bool RequiresLinux,
    string? ExplicitSku,
    string? ExplicitOs,
    bool UseManagedIdentityStorage = false);

public static class FunctionAppValidation
{
    private static readonly HashSet<string> SupportedRuntimes = new()
    {
        "dotnet", "dotnet-isolated", "node", "python", "java", "powershell", "custom"
    };

    private static void ValidateRequired(string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Missing required parameter: {name}");
    }

    public static string? GetDefaultRuntimeVersion(string runtime) => runtime switch
    {
        "python" => "3.12",
        "node" => "22",
        "dotnet" => "8.0",
        "dotnet-isolated" => "8.0",
        "java" => "17",
        "powershell" => "7.4",
        _ => null
    };

    public static NormalizedInputs ValidateAndNormalizeInputs(
        string subscription,
        string resourceGroup,
        string functionAppName,
        string location,
        string? runtime,
        string? runtimeVersion,
        string? hostingKind,
        string? sku,
        string? os,
        string? storageAccountName,
        string? containerAppsEnvironmentName)
    {
        ValidateRequired(nameof(subscription), subscription);
        ValidateRequired(nameof(resourceGroup), resourceGroup);
        ValidateRequired(nameof(functionAppName), functionAppName);
        ValidateRequired(nameof(location), location);

        var inputs = new NormalizedInputs(
            string.IsNullOrWhiteSpace(runtime) ? "dotnet" : runtime.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(runtimeVersion) ? null : runtimeVersion.Trim(),
            string.IsNullOrWhiteSpace(hostingKind) ? null : hostingKind.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(sku) ? null : sku.Trim(),
            string.IsNullOrWhiteSpace(os) ? null : os.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(storageAccountName) ? null : storageAccountName.Trim(),
            string.IsNullOrWhiteSpace(containerAppsEnvironmentName) ? null : containerAppsEnvironmentName.Trim());

        ValidateParameterCombinations(inputs);
        return inputs;
    }

    public static void ValidateParameterCombinations(NormalizedInputs inputs)
    {
        var hostingKind = ParseHostingKind(inputs.PlanType);

        if (inputs.OperatingSystem is not null && inputs.OperatingSystem != "windows" && inputs.OperatingSystem != "linux")
            throw new ArgumentException("Operating system must be either 'windows' or 'linux'.");

        if (inputs.StorageAccountName is not null)
        {
            if (inputs.StorageAccountName.Length < 3 || inputs.StorageAccountName.Length > 24)
                throw new ArgumentException("Storage account name must be between 3 and 24 characters long.");
            if (!inputs.StorageAccountName.All(c => char.IsLetterOrDigit(c) && (char.IsDigit(c) || char.IsLower(c))))
                throw new ArgumentException("Storage account name must contain only lowercase letters and numbers.");
        }

        if (inputs.ContainerAppsEnvironmentName is not null && hostingKind != HostingKind.ContainerApp)
            throw new InvalidOperationException("Container Apps environment name can only be specified when using Container Apps hosting.");

        if (hostingKind == HostingKind.ContainerApp && inputs.PlanSku is not null)
            throw new InvalidOperationException("Plan SKU cannot be specified for Container Apps hosting.");

        if (!SupportedRuntimes.Contains(inputs.Runtime))
            throw new ArgumentException($"Runtime '{inputs.Runtime}' is not supported. Supported runtimes: {string.Join(", ", SupportedRuntimes)}.");

        if (inputs.Runtime == "python" && inputs.OperatingSystem == "windows")
            throw new InvalidOperationException("Python runtime requires Linux operating system.");

        if (hostingKind == HostingKind.FlexConsumption && inputs.Runtime == "dotnet" && inputs.RuntimeVersion is not null)
        {
            throw new InvalidOperationException("Flex Consumption with .NET runtime automatically uses dotnet-isolated. Specify runtime as 'dotnet-isolated' instead.");
        }
    }

    public static CreateOptions BuildCreateOptions(NormalizedInputs inputs, bool useManagedIdentityStorage = false)
    {
        var hostingKind = ParseHostingKind(inputs.PlanType);
        var selectedRuntime = hostingKind == HostingKind.FlexConsumption && inputs.Runtime == "dotnet"
            ? "dotnet-isolated"
            : inputs.Runtime;
        var selectedRuntimeVersion = inputs.RuntimeVersion ?? GetDefaultRuntimeVersion(selectedRuntime);
        var (requiresLinux, normalizedOs) = ResolveOs(selectedRuntime, hostingKind, inputs.OperatingSystem);
        return new CreateOptions(selectedRuntime, selectedRuntimeVersion, hostingKind, requiresLinux, inputs.PlanSku, normalizedOs, useManagedIdentityStorage);
    }

    public static bool ParseStorageAuthMode(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
            return false;
        var normalized = mode.Trim().ToLowerInvariant();
        return normalized switch
        {
            "managed-identity" or "managedidentity" or "mi" => true,
            "connection-string" or "connectionstring" or "key" => false,
            _ => throw new ArgumentException($"Invalid --storage-auth-mode value '{mode}'. Expected 'connection-string' or 'managed-identity'.")
        };
    }

    public static HostingKind ParseHostingKind(string? planType) => planType switch
    {
        "flex" or "flexconsumption" => HostingKind.FlexConsumption,
        "premium" or "functionspremium" => HostingKind.Premium,
        "appservice" => HostingKind.AppService,
        "containerapp" or "containerapps" => HostingKind.ContainerApp,
        _ => HostingKind.Consumption
    };

    public static (bool RequiresLinux, string? NormalizedOs) ResolveOs(string runtime, HostingKind hostingKind, string? operatingSystem)
    {
        var forcedLinux = runtime == "python" || hostingKind == HostingKind.FlexConsumption || hostingKind == HostingKind.ContainerApp;
        bool requiresLinux = forcedLinux;

        if (string.IsNullOrEmpty(operatingSystem))
            return (requiresLinux, null);

        if (forcedLinux && operatingSystem == "windows")
            throw new InvalidOperationException("Selected runtime/plan requires Linux operating system.");

        if (!forcedLinux)
            requiresLinux = operatingSystem == "linux";

        return (requiresLinux, operatingSystem);
    }

    public static string? ExtractMajorVersion(string? version) => string.IsNullOrWhiteSpace(version)
        ? null
        : new string(version.Trim().TakeWhile(char.IsDigit).ToArray()) switch { "" => null, var v => v };

    public static string InferTier(string skuName)
    {
        var upper = skuName.Trim().ToUpperInvariant();

        if (upper.StartsWith("FC"))
            return "FlexConsumption";
        if (upper.StartsWith("EP"))
            return "ElasticPremium";
        if (upper.StartsWith("P") && (upper.Contains("V3") || upper.StartsWith("P1V3") || upper.StartsWith("P2V3") || upper.StartsWith("P3V3")))
            return "PremiumV3";
        if (upper.StartsWith("P"))
            return "Premium";
        if (upper.StartsWith("B"))
            return "Basic";
        if (upper.StartsWith("S"))
            return "Standard";
        if (upper.StartsWith("Y"))
            return "Dynamic";

        return "Standard";
    }

    public static bool IsFunctionApp(WebSiteData siteData) =>
        siteData.Kind?.Contains("functionapp", StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsFlexConsumption(AppServicePlanData plan) =>
        string.Equals(plan.Sku?.Tier, "FlexConsumption", StringComparison.OrdinalIgnoreCase);
}
