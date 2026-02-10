// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tests.Helpers;

namespace Azure.Mcp.Tests.Client.Helpers;

public class LiveTestSettings
{
    public const string TestSettingsFileName = ".testsettings.json";

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

            try
            {
                settings = JsonSerializer.Deserialize<LiveTestSettings>(json, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });

                if (settings != null)
                {
                    settings.SettingsDirectory = Path.GetDirectoryName(path) ?? string.Empty;
                    return true;
                }
            }
            catch (JsonException ex) when (ex.Path?.Contains(nameof(TestMode), StringComparison.OrdinalIgnoreCase) == true)
            {
                var validValues = string.Join(", ", Enum.GetNames<TestMode>());
                Console.Error.WriteLine(
                    $"Invalid TestMode value in {TestSettingsFileName}. Valid values are: {validValues}. " +
                    $"Error details: {ex.Message}");
                settings = null;
                return false;
            }
        }

        settings = null;
        return false;
    }
}
