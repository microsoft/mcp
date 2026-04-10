using Azure.Mcp.Tools.Monitor.Models;

namespace Azure.Mcp.Tools.Monitor.Detectors;

/// <summary>
/// Detects Python application instrumentation status.
/// 
/// Library lists are loaded from Resources/instrumentations/python-instrumentations.json
/// which can be updated by running the update script.
/// 
/// Sources:
/// - OpenTelemetry Python Contrib: https://github.com/open-telemetry/opentelemetry-python-contrib
/// - Azure Monitor Distro: https://github.com/Azure/azure-sdk-for-python/tree/main/sdk/monitor/azure-monitor-opentelemetry
/// </summary>
public class PythonInstrumentationDetector : IInstrumentationDetector
{
    public Language SupportedLanguage => Language.Python;

    public InstrumentationResult Detect(string workspacePath)
    {
        var dependencies = new List<string>();
        string? evidenceFile = null;

        // Check for requirements.txt
        var requirementsPath = Path.Combine(workspacePath, "requirements.txt");
        if (File.Exists(requirementsPath))
        {
            var content = TryReadFile(requirementsPath);
            if (content != null)
            {
                dependencies.AddRange(ParseRequirementsTxt(content));
                evidenceFile ??= requirementsPath;
            }
        }

        // Check for pyproject.toml
        var pyprojectPath = Path.Combine(workspacePath, "pyproject.toml");
        if (File.Exists(pyprojectPath))
        {
            var content = TryReadFile(pyprojectPath);
            if (content != null)
            {
                dependencies.AddRange(ParsePyprojectToml(content));
                evidenceFile ??= pyprojectPath;
            }
        }

        // No dependency files found = Greenfield
        if (evidenceFile == null)
        {
            return new InstrumentationResult(
                InstrumentationState.Greenfield,
                null
            );
        }

        // Normalize and deduplicate package names
        dependencies = dependencies
            .Select(PythonInstrumentationRegistry.NormalizePackageName)
            .Distinct()
            .ToList();

        // TODO: Re-enable Brownfield detection scenarios
        // // Check for Azure Monitor packages (highest priority)
        // var azureMonitorFound = dependencies.FirstOrDefault(d => 
        //     PythonInstrumentationRegistry.AzureMonitorPackages.Contains(d));
        // if (azureMonitorFound != null)
        // {
        //     return new InstrumentationResult(
        //         InstrumentationState.Brownfield,
        //         new ExistingInstrumentation
        //         {
        //             Type = InstrumentationType.AzureMonitorDistro,
        //             Evidence = [new Evidence { File = evidenceFile, Indicator = $"Azure Monitor package '{azureMonitorFound}' found in dependencies" }]
        //         }
        //     );
        // }

        // // Check for OpenTelemetry instrumentation packages (opentelemetry-instrumentation-*)
        // var otelInstrumentationPackage = dependencies.FirstOrDefault(d => 
        //     d.StartsWith("opentelemetry-instrumentation-") && d != "opentelemetry-instrumentation");
        // if (otelInstrumentationPackage != null)
        // {
        //     return new InstrumentationResult(
        //         InstrumentationState.Brownfield,
        //         new ExistingInstrumentation
        //         {
        //             Type = InstrumentationType.OpenTelemetry,
        //             Evidence = [new Evidence { File = evidenceFile, Indicator = $"OpenTelemetry instrumentation package '{otelInstrumentationPackage}' found in dependencies" }]
        //         }
        //     );
        // }

        // // Check for OpenTelemetry core packages
        // var openTelemetryFound = dependencies.FirstOrDefault(d => 
        //     PythonInstrumentationRegistry.OpenTelemetryCorePackages.Contains(d));
        // if (openTelemetryFound != null)
        // {
        //     return new InstrumentationResult(
        //         InstrumentationState.Brownfield,
        //         new ExistingInstrumentation
        //         {
        //             Type = InstrumentationType.OpenTelemetry,
        //             Evidence = [new Evidence { File = evidenceFile, Indicator = $"OpenTelemetry package '{openTelemetryFound}' found in dependencies" }]
        //         }
        //     );
        // }

        // // Check for Application Insights SDK
        // var appInsightsFound = dependencies.FirstOrDefault(d => 
        //     PythonInstrumentationRegistry.ApplicationInsightsPackages.Contains(d));
        // if (appInsightsFound != null)
        // {
        //     return new InstrumentationResult(
        //         InstrumentationState.Brownfield,
        //         new ExistingInstrumentation
        //         {
        //             Type = InstrumentationType.ApplicationInsightsSdk,
        //             Evidence = [new Evidence { File = evidenceFile, Indicator = $"Application Insights SDK '{appInsightsFound}' found in dependencies" }]
        //         }
        //     );
        // }

        // No instrumentation found (Brownfield checks disabled)
        return new InstrumentationResult(
            InstrumentationState.Greenfield,
            null
        );
    }

    /// <summary>
    /// Parse requirements.txt format.
    /// Each line is typically: package-name==1.0.0 or package-name>=1.0
    /// </summary>
    public static List<string> ParseRequirementsTxt(string content)
    {
        var packages = new List<string>();
        var lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Skip empty lines
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            // Skip comments
            if (trimmed.StartsWith("#"))
            {
                continue;
            }

            // Skip flags like -r, -e, --index-url, etc.
            if (trimmed.StartsWith("-"))
            {
                continue;
            }

            // Extract package name by finding the first special character
            // Package names end at: ==, >=, <=, ~=, !=, <, >, [, ;, @, or space
            var packageName = ExtractPackageName(trimmed);
            if (!string.IsNullOrEmpty(packageName))
            {
                packages.Add(packageName);
            }
        }

        return packages;
    }

    /// <summary>
    /// Extract package name from a requirements line.
    /// Stops at version specifiers (==, >=, etc.) or extras ([).
    /// </summary>
    public static string ExtractPackageName(string line)
    {
        var endIndex = line.Length;

        // Find the earliest occurrence of any delimiter
        char[] delimiters = { '=', '<', '>', '!', '~', '[', ';', '@', ' ' };
        foreach (var delimiter in delimiters)
        {
            var index = line.IndexOf(delimiter);
            if (index > 0 && index < endIndex)
            {
                endIndex = index;
            }
        }

        return line.Substring(0, endIndex).Trim();
    }

    /// <summary>
    /// Parse pyproject.toml format (PEP 621 and Poetry).
    /// Uses simple line-by-line parsing instead of complex regex.
    /// </summary>
    public static List<string> ParsePyprojectToml(string content)
    {
        var packages = new List<string>();
        var lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var inPoetryDependencies = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Check for section headers
            if (trimmed.StartsWith("["))
            {
                inPoetryDependencies = trimmed == "[tool.poetry.dependencies]";
                continue;
            }

            // Check for inline dependencies array: dependencies = ["pkg1", "pkg2"]
            if (trimmed.StartsWith("dependencies") && trimmed.Contains("=") && trimmed.Contains("["))
            {
                packages.AddRange(ExtractPackagesFromArray(trimmed));
                continue;
            }

            // Poetry format: package-name = "version" or package-name = {version = "x"}
            if (inPoetryDependencies && trimmed.Contains("="))
            {
                var packageName = trimmed.Split('=')[0].Trim();
                // Skip python version and empty names
                if (!string.IsNullOrEmpty(packageName) && packageName.ToLowerInvariant() != "python")
                {
                    packages.Add(packageName);
                }
            }
        }

        return packages;
    }

    /// <summary>
    /// Extract package names from an inline array like: dependencies = ["flask>=2.0", "requests"]
    /// </summary>
    private static List<string> ExtractPackagesFromArray(string line)
    {
        var packages = new List<string>();

        // Find the array content between [ and ]
        var startIndex = line.IndexOf('[');
        var endIndex = line.LastIndexOf(']');

        if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
        {
            return packages;
        }

        var arrayContent = line.Substring(startIndex + 1, endIndex - startIndex - 1);

        // Split by comma and extract each package
        var items = arrayContent.Split(',');
        foreach (var item in items)
        {
            var trimmed = item.Trim().Trim('"', '\'');
            if (!string.IsNullOrEmpty(trimmed))
            {
                // Extract just the package name (before any version specifier)
                var packageName = ExtractPackageName(trimmed);
                if (!string.IsNullOrEmpty(packageName))
                {
                    packages.Add(packageName);
                }
            }
        }

        return packages;
    }

    /// <summary>
    /// Safely read file content with exception handling for locked files,
    /// permission issues, or encoding problems.
    /// </summary>
    internal static string? TryReadFile(string filePath)
    {
        try
        {
            return File.ReadAllText(filePath);
        }
        catch (IOException)
        {
            // File is locked or inaccessible
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            // Insufficient permissions
            return null;
        }
        catch (Exception)
        {
            // Handle other potential exceptions
            return null;
        }
    }
}
