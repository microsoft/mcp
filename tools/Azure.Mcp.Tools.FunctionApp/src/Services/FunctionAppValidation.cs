// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.AppService;

namespace Azure.Mcp.Tools.FunctionApp.Services;

/// <summary>
/// Validation, normalization, and defaulting rules shared by the Function App create commands.
/// </summary>
public static class FunctionAppValidation
{
    public const string DefaultRuntime = "dotnet";
    public const int MinFunctionAppNameLength = 2;
    public const int MaxFunctionAppNameLength = 43;

    private static readonly HashSet<string> s_supportedRuntimes = new(StringComparer.Ordinal)
    {
        "dotnet", "dotnet-isolated", "node", "python", "java", "powershell", "custom"
    };

    private static readonly HashSet<string> s_containerAppPlanTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "containerapp", "containerapps"
    };

    public static string? ValidateFunctionAppNameLength(string? functionAppName)
    {
        if (string.IsNullOrWhiteSpace(functionAppName))
        {
            return null;
        }

        var length = functionAppName.Length;
        if (length < MinFunctionAppNameLength || length > MaxFunctionAppNameLength)
        {
            return $"--function-app name must be between {MinFunctionAppNameLength} and {MaxFunctionAppNameLength} characters.";
        }

        return null;
    }

    public static bool IsContainerAppPlanType(string? planType) =>
        !string.IsNullOrWhiteSpace(planType) && s_containerAppPlanTypes.Contains(planType.Trim());

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
        string? planType,
        string? planSku,
        string? operatingSystem,
        string? storageAccountName,
        string? containerAppsEnvironmentName)
    {
        ValidateRequired(nameof(subscription), subscription);
        ValidateRequired(nameof(resourceGroup), resourceGroup);
        ValidateRequired(nameof(functionAppName), functionAppName);
        ValidateRequired(nameof(location), location);

        var inputs = new NormalizedInputs(
            string.IsNullOrWhiteSpace(runtime) ? DefaultRuntime : runtime.Trim().ToLowerInvariant(),
            NormalizeOptional(runtimeVersion),
            NormalizeOptional(planType)?.ToLowerInvariant(),
            NormalizeOptional(planSku),
            NormalizeOptional(operatingSystem)?.ToLowerInvariant(),
            NormalizeOptional(storageAccountName),
            NormalizeOptional(containerAppsEnvironmentName));

        ValidateParameterCombinations(inputs);
        return inputs;
    }

    public static void ValidateParameterCombinations(NormalizedInputs inputs)
    {
        var hostingKind = ParseHostingKind(inputs.PlanType);

        if (inputs.OperatingSystem is not null && inputs.OperatingSystem != "windows" && inputs.OperatingSystem != "linux")
        {
            throw new ArgumentException("Operating system must be either 'windows' or 'linux'.");
        }

        if (inputs.StorageAccountName is not null)
        {
            if (inputs.StorageAccountName.Length is < 3 or > 24)
            {
                throw new ArgumentException("Storage account name must be between 3 and 24 characters long.");
            }

            if (!inputs.StorageAccountName.All(c => char.IsAsciiDigit(c) || char.IsAsciiLetterLower(c)))
            {
                throw new ArgumentException("Storage account name must contain only lowercase letters and numbers.");
            }
        }

        if (inputs.ContainerAppsEnvironmentName is not null && hostingKind != HostingKind.ContainerApp)
        {
            throw new InvalidOperationException("Container Apps environment name can only be specified when using Container Apps hosting.");
        }

        if (hostingKind == HostingKind.ContainerApp && inputs.PlanSku is not null)
        {
            throw new InvalidOperationException("Plan SKU cannot be specified for Container Apps hosting.");
        }

        if (!s_supportedRuntimes.Contains(inputs.Runtime))
        {
            throw new ArgumentException($"Runtime '{inputs.Runtime}' is not supported. Supported runtimes: {string.Join(", ", s_supportedRuntimes)}.");
        }

        if (inputs.Runtime == "python" && inputs.OperatingSystem == "windows")
        {
            throw new InvalidOperationException("Python runtime requires Linux operating system.");
        }

        if (hostingKind == HostingKind.FlexConsumption && inputs.Runtime == "dotnet" && inputs.RuntimeVersion is not null)
        {
            throw new InvalidOperationException("Flex Consumption with .NET runtime automatically uses dotnet-isolated. Specify runtime as 'dotnet-isolated' instead.");
        }
    }

    public static CreateOptions BuildCreateOptions(NormalizedInputs inputs, bool useManagedIdentityStorage = true)
    {
        var hostingKind = ParseHostingKind(inputs.PlanType);
        var selectedRuntime = hostingKind == HostingKind.FlexConsumption && inputs.Runtime == "dotnet"
            ? "dotnet-isolated"
            : inputs.Runtime;
        var selectedRuntimeVersion = inputs.RuntimeVersion ?? GetDefaultRuntimeVersion(selectedRuntime);
        var (requiresLinux, normalizedOs) = ResolveOs(selectedRuntime, hostingKind, inputs.OperatingSystem);

        return new CreateOptions(selectedRuntime, selectedRuntimeVersion, hostingKind, requiresLinux, inputs.PlanSku, normalizedOs, useManagedIdentityStorage);
    }

    /// <summary>
    /// Returns <c>true</c> for managed identity, <c>false</c> for connection string, or <c>null</c> when unspecified.
    /// </summary>
    public static bool? ParseStorageAuthMode(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return null;
        }

        return mode.Trim().ToLowerInvariant() switch
        {
            "managed-identity" or "managedidentity" or "mi" => true,
            "connection-string" or "connectionstring" or "key" => false,
            _ => throw new ArgumentException($"Invalid --storage-auth-mode value '{mode}'. Expected 'managed-identity' or 'connection-string'.")
        };
    }

    public static HostingKind ParseHostingKind(string? planType)
    {
        if (string.IsNullOrWhiteSpace(planType))
        {
            return HostingKind.Consumption;
        }

        return planType.Trim().ToLowerInvariant() switch
        {
            "consumption" => HostingKind.Consumption,
            "flex" or "flexconsumption" => HostingKind.FlexConsumption,
            "premium" or "functionspremium" => HostingKind.Premium,
            "appservice" => HostingKind.AppService,
            "containerapp" or "containerapps" => HostingKind.ContainerApp,
            _ => throw new ArgumentException($"Unsupported plan type '{planType}'. Supported values: consumption, flex, premium, appservice.")
        };
    }

    public static (bool RequiresLinux, string? NormalizedOs) ResolveOs(string runtime, HostingKind hostingKind, string? operatingSystem)
    {
        var forcedLinux = runtime == "python" || hostingKind == HostingKind.FlexConsumption || hostingKind == HostingKind.ContainerApp;

        if (string.IsNullOrEmpty(operatingSystem))
        {
            return (forcedLinux, null);
        }

        if (forcedLinux && operatingSystem == "windows")
        {
            throw new InvalidOperationException("Selected runtime/plan requires Linux operating system.");
        }

        return (forcedLinux || operatingSystem == "linux", operatingSystem);
    }

    public static string? ExtractMajorVersion(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return null;
        }

        var major = new string(version.Trim().TakeWhile(char.IsAsciiDigit).ToArray());
        return major.Length == 0 ? null : major;
    }

    /// <summary>
    /// Infers the App Service plan tier name from a SKU name such as B1, S1, P1v3, EP1, FC1, or Y1.
    /// </summary>
    public static string InferTier(string skuName)
    {
        var upper = skuName.Trim().ToUpperInvariant();

        if (upper.StartsWith("FC", StringComparison.Ordinal))
        {
            return "FlexConsumption";
        }

        if (upper.StartsWith("EP", StringComparison.Ordinal))
        {
            return "ElasticPremium";
        }

        return upper switch
        {
            ['P', ..] => WithVersionSuffix("Premium", upper),
            ['I', ..] => WithVersionSuffix("Isolated", upper),
            ['B', ..] => "Basic",
            ['S', ..] => "Standard",
            ['Y', ..] => "Dynamic",
            _ => "Standard"
        };
    }

    private static string WithVersionSuffix(string tier, string upperSku)
    {
        var versionIndex = upperSku.IndexOf('V');
        return versionIndex > 0 && versionIndex < upperSku.Length - 1
            ? $"{tier}V{upperSku[(versionIndex + 1)..]}"
            : tier;
    }

    public static bool IsFunctionApp(WebSiteData siteData) =>
        siteData.Kind?.Contains("functionapp", StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsFlexConsumption(AppServicePlanData plan) =>
        string.Equals(plan.Sku?.Tier, "FlexConsumption", StringComparison.OrdinalIgnoreCase);

    public static string GetOperatingSystem(string? kind) =>
        kind?.Contains("linux", StringComparison.OrdinalIgnoreCase) == true ? "linux" : "windows";

    private static void ValidateRequired(string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Missing required parameter: {name}");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
