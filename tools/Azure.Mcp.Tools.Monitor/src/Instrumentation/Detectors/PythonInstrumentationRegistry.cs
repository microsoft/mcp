using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Monitor.Models;

namespace Azure.Mcp.Tools.Monitor.Detectors;

/// <summary>
/// Loads and caches Python instrumentation data from the JSON resource file.
/// This allows the instrumentation list to be updated without code changes.
/// </summary>
public static class PythonInstrumentationRegistry
{
    private static readonly Lazy<InstrumentationData> _data = new(LoadInstrumentationData);
    private static readonly Lazy<Dictionary<string, InstrumentationInfo>> _instrumentationLookup =
        new(() => BuildInstrumentationLookup(_data.Value));
    private static readonly Lazy<HashSet<string>> _distroSet =
        new(() => new HashSet<string>(_data.Value.DistroInstrumentations, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Gets the loaded instrumentation data.
    /// </summary>
    public static InstrumentationData Data => _data.Value;

    /// <summary>
    /// Gets the Azure Monitor packages list.
    /// </summary>
    public static IReadOnlyList<string> AzureMonitorPackages => _data.Value.AzureMonitorPackages;

    /// <summary>
    /// Gets the OpenTelemetry core packages list.
    /// </summary>
    public static IReadOnlyList<string> OpenTelemetryCorePackages => _data.Value.OpenTelemetryCorePackages;

    /// <summary>
    /// Gets the Application Insights packages list.
    /// </summary>
    public static IReadOnlyList<string> ApplicationInsightsPackages => _data.Value.ApplicationInsightsPackages;

    /// <summary>
    /// Gets the set of libraries bundled with Azure Monitor Distro.
    /// </summary>
    public static IReadOnlySet<string> DistroInstrumentations => _distroSet.Value;

    /// <summary>
    /// Check if a library has OpenTelemetry instrumentation available.
    /// </summary>
    public static bool HasInstrumentation(string libraryName)
    {
        var normalized = NormalizePackageName(libraryName);
        return _instrumentationLookup.Value.ContainsKey(normalized);
    }

    /// <summary>
    /// Get instrumentation info for a library.
    /// </summary>
    public static InstrumentationInfo? GetInstrumentation(string libraryName)
    {
        var normalized = NormalizePackageName(libraryName);
        return _instrumentationLookup.Value.TryGetValue(normalized, out var info) ? info : null;
    }

    /// <summary>
    /// Get the OpenTelemetry instrumentation package name for a library.
    /// </summary>
    public static string? GetInstrumentationPackage(string libraryName)
    {
        return GetInstrumentation(libraryName)?.InstrumentationPackage;
    }

    /// <summary>
    /// Check if a library is auto-instrumented by Azure Monitor Distro.
    /// Uses the inDistro flag from the instrumentation entry if available,
    /// otherwise falls back to the distroInstrumentations array.
    /// </summary>
    public static bool IsInDistro(string libraryName)
    {
        var normalized = NormalizePackageName(libraryName);

        // First check the instrumentation entry's InDistro flag
        var info = GetInstrumentation(libraryName);
        if (info != null)
        {
            return info.InDistro;
        }

        // Fall back to the distroInstrumentations array for backward compatibility
        return _distroSet.Value.Contains(normalized);
    }

    /// <summary>
    /// Get all instrumentations as a flat dictionary.
    /// </summary>
    public static IReadOnlyDictionary<string, InstrumentationInfo> GetAllInstrumentations()
    {
        return _instrumentationLookup.Value;
    }

    /// <summary>
    /// Get only instrumentations that are bundled with Azure Monitor Distro.
    /// These are auto-instrumented when using azure-monitor-opentelemetry.
    /// </summary>
    public static IEnumerable<InstrumentationInfo> GetDistroInstrumentations()
    {
        return _instrumentationLookup.Value.Values.Where(i => i.InDistro);
    }

    /// <summary>
    /// Get instrumentations that require manual setup (not in distro).
    /// These need their instrumentation packages to be installed and configured.
    /// </summary>
    public static IEnumerable<InstrumentationInfo> GetManualInstrumentations()
    {
        return _instrumentationLookup.Value.Values.Where(i => !i.InDistro);
    }

    /// <summary>
    /// Get instrumentations by category.
    /// </summary>
    public static IEnumerable<InstrumentationInfo> GetByCategory(string category)
    {
        return _instrumentationLookup.Value.Values.Where(i =>
            i.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Find instrumentable libraries from a list of dependencies.
    /// Returns libraries that have OpenTelemetry instrumentation available,
    /// with information about whether they are in distro or need manual setup.
    /// </summary>
    public static List<(string Library, string InstrumentationPackage, bool InDistro)> FindInstrumentableLibraries(
        IEnumerable<string> dependencies)
    {
        var result = new List<(string Library, string InstrumentationPackage, bool InDistro)>();

        foreach (var dep in dependencies)
        {
            var normalized = NormalizePackageName(dep);
            if (_instrumentationLookup.Value.TryGetValue(normalized, out var info))
            {
                result.Add((normalized, info.InstrumentationPackage, info.InDistro));
            }
        }

        return result;
    }

    /// <summary>
    /// Normalize Python package names: lowercase and replace underscores with hyphens.
    /// </summary>
    public static string NormalizePackageName(string name)
    {
        return name.ToLowerInvariant().Replace("_", "-");
    }

    private static InstrumentationData LoadInstrumentationData()
    {
        // Try to load from file system first (allows updates without recompilation)
        var resourcePath = FindResourceFile();

        if (resourcePath != null && File.Exists(resourcePath))
        {
            try
            {
                var json = File.ReadAllText(resourcePath);
                var data = JsonSerializer.Deserialize(json, OnboardingJsonContext.Default.InstrumentationData);
                if (data != null)
                {
                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: Failed to load instrumentations from {resourcePath}: {ex.Message}");
            }
        }

        // Fall back to embedded default data
        Console.Error.WriteLine("Warning: Using fallback instrumentation data");
        return GetFallbackData();
    }

    private static string? FindResourceFile()
    {
        // Check multiple locations
        var possiblePaths = new[]
        {
            // Relative to executable
            Path.Combine(AppContext.BaseDirectory, "Resources", "instrumentations", "python-instrumentations.json"),
            // Development path
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Resources", "instrumentations", "python-instrumentations.json"),
            // Current directory
            Path.Combine(Directory.GetCurrentDirectory(), "Resources", "instrumentations", "python-instrumentations.json"),
        };

        return possiblePaths.FirstOrDefault(File.Exists);
    }

    private static Dictionary<string, InstrumentationInfo> BuildInstrumentationLookup(InstrumentationData data)
    {
        var lookup = new Dictionary<string, InstrumentationInfo>(StringComparer.OrdinalIgnoreCase);

        void AddCategory(List<InstrumentationInfo>? items, string category)
        {
            if (items == null)
                return;
            foreach (var item in items)
            {
                item.Category = category;
                var normalized = NormalizePackageName(item.LibraryName);
                lookup.TryAdd(normalized, item);
            }
        }

        if (data.Instrumentations != null)
        {
            AddCategory(data.Instrumentations.Framework, "framework");
            AddCategory(data.Instrumentations.Http, "http");
            AddCategory(data.Instrumentations.Database, "database");
            AddCategory(data.Instrumentations.Messaging, "messaging");
            AddCategory(data.Instrumentations.Cloud, "cloud");
            AddCategory(data.Instrumentations.Genai, "genai");
            AddCategory(data.Instrumentations.Other, "other");
        }

        return lookup;
    }

    private static InstrumentationData GetFallbackData()
    {
        // Minimal fallback data in case the JSON file is not found
        return new InstrumentationData
        {
            Metadata = new MetadataInfo { LastUpdated = "fallback", Source = "embedded" },
            AzureMonitorPackages = ["azure-monitor-opentelemetry", "azure-monitor-opentelemetry-exporter"],
            OpenTelemetryCorePackages = ["opentelemetry-api", "opentelemetry-sdk", "opentelemetry-instrumentation"],
            ApplicationInsightsPackages = ["applicationinsights", "opencensus-ext-azure"],
            DistroInstrumentations = ["django", "flask", "fastapi", "requests", "urllib", "urllib3", "psycopg2"],
            Instrumentations = new InstrumentationCategories
            {
                Framework =
                [
                    new() { LibraryName = "django", DisplayName = "Django", InstrumentationPackage = "opentelemetry-instrumentation-django" },
                    new() { LibraryName = "flask", DisplayName = "Flask", InstrumentationPackage = "opentelemetry-instrumentation-flask" },
                    new() { LibraryName = "fastapi", DisplayName = "FastAPI", InstrumentationPackage = "opentelemetry-instrumentation-fastapi" }
                ],
                Http =
                [
                    new() { LibraryName = "requests", DisplayName = "Requests", InstrumentationPackage = "opentelemetry-instrumentation-requests" },
                    new() { LibraryName = "urllib3", DisplayName = "URLLib3", InstrumentationPackage = "opentelemetry-instrumentation-urllib3" }
                ],
                Database =
                [
                    new() { LibraryName = "psycopg2", DisplayName = "Psycopg2", InstrumentationPackage = "opentelemetry-instrumentation-psycopg2" },
                    new() { LibraryName = "sqlalchemy", DisplayName = "SQLAlchemy", InstrumentationPackage = "opentelemetry-instrumentation-sqlalchemy" }
                ]
            }
        };
    }

}

#region Data Models

public class InstrumentationData
{
    [JsonPropertyName("metadata")]
    public MetadataInfo? Metadata { get; set; }

    [JsonPropertyName("azureMonitorPackages")]
    public List<string> AzureMonitorPackages { get; set; } = [];

    [JsonPropertyName("openTelemetryCorePackages")]
    public List<string> OpenTelemetryCorePackages { get; set; } = [];

    [JsonPropertyName("applicationInsightsPackages")]
    public List<string> ApplicationInsightsPackages { get; set; } = [];

    [JsonPropertyName("distroInstrumentations")]
    public List<string> DistroInstrumentations { get; set; } = [];

    [JsonPropertyName("instrumentations")]
    public InstrumentationCategories? Instrumentations { get; set; }
}

public class MetadataInfo
{
    [JsonPropertyName("lastUpdated")]
    public string? LastUpdated { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("distroSource")]
    public string? DistroSource { get; set; }
}

public class InstrumentationCategories
{
    [JsonPropertyName("framework")]
    public List<InstrumentationInfo>? Framework { get; set; }

    [JsonPropertyName("http")]
    public List<InstrumentationInfo>? Http { get; set; }

    [JsonPropertyName("database")]
    public List<InstrumentationInfo>? Database { get; set; }

    [JsonPropertyName("messaging")]
    public List<InstrumentationInfo>? Messaging { get; set; }

    [JsonPropertyName("cloud")]
    public List<InstrumentationInfo>? Cloud { get; set; }

    [JsonPropertyName("genai")]
    public List<InstrumentationInfo>? Genai { get; set; }

    [JsonPropertyName("other")]
    public List<InstrumentationInfo>? Other { get; set; }
}

public class InstrumentationInfo
{
    [JsonPropertyName("libraryName")]
    public string LibraryName { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("moduleName")]
    public string? ModuleName { get; set; }

    [JsonPropertyName("instrumentationPackage")]
    public string InstrumentationPackage { get; set; } = string.Empty;

    /// <summary>
    /// Whether this library is auto-instrumented by Azure Monitor Distro.
    /// If true, no manual instrumentation is needed when using azure-monitor-opentelemetry.
    /// If false, the instrumentation package must be installed and configured manually.
    /// </summary>
    [JsonPropertyName("inDistro")]
    public bool InDistro { get; set; } = false;

    /// <summary>
    /// Optional note about special handling for this instrumentation.
    /// </summary>
    [JsonPropertyName("note")]
    public string? Note { get; set; }

    /// <summary>
    /// Category is set after loading from JSON based on which array the item was in.
    /// </summary>
    [JsonIgnore]
    public string Category { get; set; } = string.Empty;
}

#endregion
