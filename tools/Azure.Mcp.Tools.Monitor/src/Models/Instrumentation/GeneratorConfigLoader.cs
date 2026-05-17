using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models;

/// <summary>
/// Configuration model for generator actions loaded from JSON
/// </summary>
public class GeneratorConfig
{
    public string GeneratorType { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string AppType { get; set; } = string.Empty;
    public DecisionConfig Decision { get; set; } = new();
    public List<ActionConfig> Actions { get; set; } = new();
}

public class DecisionConfig
{
    public string Intent { get; set; } = string.Empty;
    public string Solution { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
}

public class ActionConfig
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? DependsOn { get; set; }

    // Review Education specific
    [JsonPropertyName("resources")]
    public List<string>? LearningResourceIds { get; set; }

    // Package specific
    public string? PackageManager { get; set; }
    public string? PackageName { get; set; }
    public string? Version { get; set; }
    public string? TargetFile { get; set; }

    // Modify Code specific
    public string? CodeTemplate { get; set; }
    [JsonPropertyName("insertionHint")]
    public string? InsertLocation { get; set; }
    [JsonPropertyName("requiredImport")]
    public string? RequiredNamespace { get; set; }

    // Config specific
    [JsonPropertyName("configKey")]
    public string? ConfigPath { get; set; }
    [JsonPropertyName("configValue")]
    public string? DefaultValue { get; set; }
    [JsonPropertyName("validationKey")]
    public string? EnvVarName { get; set; }

    // Manual Step specific
    public string? Instructions { get; set; }
    public List<string>? Links { get; set; }

    // Validate Install specific
    public List<string>? FilesToExist { get; set; }
    public Dictionary<string, List<string>>? FileContentChecks { get; set; }
}

/// <summary>
/// Service to load generator configurations from embedded resources
/// </summary>
public class GeneratorConfigLoader
{
    private static readonly ConcurrentDictionary<string, GeneratorConfig> ConfigCache = new();

    public static GeneratorConfig LoadConfig(string generatorType)
    {
        if (ConfigCache.TryGetValue(generatorType, out var cachedConfig))
        {
            return cachedConfig;
        }

        var assembly = Assembly.GetExecutingAssembly();
        // Replace hyphens with underscores for embedded resource naming
        var resourceName = $"Azure.Mcp.Tools.Monitor.Instrumentation.Resources.generator_configs.{generatorType}.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Configuration not found: {resourceName}");
        }

        try
        {
            var config = JsonSerializer.Deserialize(stream, OnboardingJsonContext.Default.GeneratorConfig);

            if (config == null)
            {
                throw new InvalidOperationException($"Failed to deserialize configuration: {generatorType}");
            }

            ConfigCache.TryAdd(generatorType, config);
            return config;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Malformed JSON in configuration: {generatorType}", ex);
        }
    }

    public static string LoadCodeTemplate(string templatePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        // Replace slashes with dots for embedded resource naming (hyphens are kept as-is)
        // Note: templatePath already includes "templates/" prefix from JSON config
        var resourceName = $"Azure.Mcp.Tools.Monitor.Instrumentation.Resources.generator_configs.{templatePath.Replace('/', '.')}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Template not found: {resourceName}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
