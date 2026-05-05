// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Mcp.Tests.Helpers;

namespace Microsoft.Mcp.Tests.Client.Helpers;

/// <summary>
/// Identifies the Azure cloud environment a live test run is targeting.
/// Mirrors <c>Azure.Mcp.Core.Services.Azure.Authentication.AzureCloudConfiguration.AzureCloud</c>
/// so the test assembly does not need a reference to that project.
/// </summary>
public enum AzureCloud
{
    AzurePublicCloud,
    AzureChinaCloud,
    AzureUSGovernmentCloud,
}

public class LiveTestSettings
{
    public const string TestSettingsFileName = ".testsettings.json";

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string PrincipalName { get; set; } = string.Empty;
    public bool IsServicePrincipal { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public string SubscriptionName { get; set; } = string.Empty;
    public string ResourceGroupName { get; set; } = string.Empty;
    public string ResourceBaseName { get; set; } = string.Empty;
    public string SettingsDirectory { get; set; } = string.Empty;
    public string TestPackage { get; set; } = string.Empty;
    public TestMode TestMode { get; set; } = TestMode.Live;
    public bool DebugOutput { get; set; }
    public Dictionary<string, string> DeploymentOutputs { get; set; } = [];
    public Dictionary<string, string> EnvironmentVariables { get; set; } = [];

    /// <summary>
    /// The Azure cloud the tests are targeting. Derived from the AZURE_CLOUD environment
    /// variable propagated via <see cref="EnvironmentVariables"/>. Defaults to
    /// <see cref="AzureCloud.AzurePublicCloud"/> when not set or unrecognized.
    /// </summary>
    [JsonIgnore]
    public AzureCloud Cloud
    {
        get
        {
            if (EnvironmentVariables == null || !EnvironmentVariables.TryGetValue("AZURE_CLOUD", out var value) || string.IsNullOrWhiteSpace(value))
            {
                return AzureCloud.AzurePublicCloud;
            }

            return value.ToLowerInvariant() switch
            {
                "azurecloud" or "azurepubliccloud" or "public" or "azurepublic" => AzureCloud.AzurePublicCloud,
                "azurechinacloud" or "china" or "azurechina" => AzureCloud.AzureChinaCloud,
                "azureusgovernment" or "azureusgovernmentcloud" or "usgov" or "usgovernment" => AzureCloud.AzureUSGovernmentCloud,
                _ => AzureCloud.AzurePublicCloud,
            };
        }
    }

    /// <summary>True when running against Azure US Government cloud.</summary>
    [JsonIgnore]
    public bool IsAzureUSGovernment => Cloud == AzureCloud.AzureUSGovernmentCloud;

    /// <summary>True when running against Azure China cloud.</summary>
    [JsonIgnore]
    public bool IsAzureChina => Cloud == AzureCloud.AzureChinaCloud;

    /// <summary>
    /// Resource group location surfaced from the test-resources.bicep deployment output named
    /// <c>location</c> (uppercased to <c>LOCATION</c> in DeploymentOutputs). Empty when not provided.
    /// Use this instead of hardcoding regions in live tests so they work in sovereign clouds.
    /// </summary>
    [JsonIgnore]
    public string Location =>
        DeploymentOutputs != null && DeploymentOutputs.TryGetValue("LOCATION", out var loc) ? loc : string.Empty;

    /// <summary>
    /// Returns <see cref="Location"/> when set, otherwise the supplied fallback. Use the fallback
    /// for tests authored before the location output was wired through bicep.
    /// </summary>
    public string GetLocationOrDefault(string fallback) =>
        string.IsNullOrEmpty(Location) ? fallback : Location;

    public static bool TryFindTestSettingsFile([NotNullWhen(true)] out string? path)
    {
        var directory = AppContext.BaseDirectory;

        while (!string.IsNullOrEmpty(directory))
        {
            var testSettingsFilePath = Path.Combine(directory, TestSettingsFileName);
            if (File.Exists(testSettingsFilePath))
            {
                path = testSettingsFilePath;
                return true;
            }

            directory = Path.GetDirectoryName(directory);
        }

        path = null;
        return false;
    }

    public static bool TryLoadTestSettings([NotNullWhen(true)] out LiveTestSettings? settings)
    {
        if (TryFindTestSettingsFile(out var path))
        {
            var json = File.ReadAllText(path);

            settings = JsonSerializer.Deserialize<LiveTestSettings>(json, s_jsonOptions);

            if (settings != null)
            {
                settings.SettingsDirectory = Path.GetDirectoryName(path) ?? string.Empty;
                return true;
            }
        }

        settings = null;
        return false;
    }
}
