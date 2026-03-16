// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Mcp.Tests.Helpers;

namespace Microsoft.Mcp.Tests.Client.Helpers;

public class LiveTestSettings
{
    public const string TestSettingsFileName = ".testsettings.json";

    private static readonly JsonSerializerOptions DeserializeOptions = new()
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
    public bool EnableProfiling { get; set; }
    public Dictionary<string, string> DeploymentOutputs { get; set; } = [];
    public Dictionary<string, string> EnvironmentVariables { get; set; } = [];

    /// <summary>
    /// Finds the first (closest) <c>.testsettings.json</c> file starting from
    /// <see cref="AppContext.BaseDirectory"/> and walking toward the filesystem root.
    /// </summary>
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

    /// <summary>
    /// Loads and merges all <c>.testsettings.json</c> files found between
    /// <see cref="AppContext.BaseDirectory"/> and the filesystem root.
    /// Closer files take precedence — a property set in a nearer file will
    /// NOT be overwritten by the same property in a file further up the tree.
    /// <para>
    /// This allows a repo-root <c>.testsettings.json</c> to supply defaults
    /// (e.g., <c>"enableProfiling": true</c>) that are overridden by a
    /// test-specific settings file closer to the output directory.
    /// </para>
    /// </summary>
    public static bool TryLoadTestSettings([NotNullWhen(true)] out LiveTestSettings? settings)
    {
        var merged = new JsonObject();
        string? closestSettingsDir = null;

        var directory = AppContext.BaseDirectory;

        while (!string.IsNullOrEmpty(directory))
        {
            var filePath = Path.Combine(directory, TestSettingsFileName);
            if (File.Exists(filePath))
            {
                closestSettingsDir ??= directory;

                var json = File.ReadAllText(filePath);
                var fileNode = JsonNode.Parse(json);
                if (fileNode is JsonObject fileObj)
                {
                    foreach (var property in fileObj)
                    {
                        // Closest file wins — only add if not already present
                        if (!merged.ContainsKey(property.Key))
                        {
                            merged[property.Key] = property.Value?.DeepClone();
                        }
                    }
                }
            }

            directory = Path.GetDirectoryName(directory);
        }

        if (merged.Count == 0)
        {
            settings = null;
            return false;
        }

        settings = merged.Deserialize<LiveTestSettings>(DeserializeOptions);
        if (settings != null)
        {
            settings.SettingsDirectory = closestSettingsDir ?? string.Empty;
            return true;
        }

        settings = null;
        return false;
    }
}
