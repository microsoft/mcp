using System.Xml.Linq;
using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Detectors;

public class DotNetInstrumentationDetector : IInstrumentationDetector
{
    public Language SupportedLanguage => Language.DotNet;

    public InstrumentationResult Detect(string workspacePath)
    {
        var evidence = new List<Evidence>();
        var csprojFiles = Directory.GetFiles(workspacePath, "*.csproj", SearchOption.AllDirectories);

        foreach (var csproj in csprojFiles)
        {
            var projectEvidence = AnalyzeProjectReferences(csproj);
            evidence.AddRange(projectEvidence);
        }

        // Also check for config files
        var configEvidence = CheckConfigFiles(workspacePath);
        evidence.AddRange(configEvidence);

        // Check packages.config for classic .NET Framework projects
        var packagesConfigEvidence = CheckPackagesConfig(workspacePath);
        evidence.AddRange(packagesConfigEvidence);

        if (evidence.Count == 0)
        {
            return new InstrumentationResult(InstrumentationState.Greenfield, null);
        }

        // Determine instrumentation type from evidence
        var instrumentationType = DetermineInstrumentationType(evidence);
        var version = ExtractVersion(evidence);
        var isTargetVersion = IsAlreadyOnTargetVersion(instrumentationType, version);

        return new InstrumentationResult(
            InstrumentationState.Brownfield,
            new ExistingInstrumentation
            {
                Type = instrumentationType,
                Version = version,
                IsTargetVersion = isTargetVersion,
                Evidence = evidence
            }
        );
    }

    private List<Evidence> CheckPackagesConfig(string workspacePath)
    {
        var evidence = new List<Evidence>();
        var packagesConfigs = Directory.GetFiles(workspacePath, "packages.config", SearchOption.AllDirectories);

        foreach (var configFile in packagesConfigs)
        {
            try
            {
                var doc = XDocument.Load(configFile);
                var packages = doc.Descendants("package");

                foreach (var pkg in packages)
                {
                    var id = pkg.Attribute("id")?.Value ?? string.Empty;
                    var version = pkg.Attribute("version")?.Value ?? "unknown";

                    if (PackageDetection.AiSdkPackages.Any(p => id.Equals(p, StringComparison.OrdinalIgnoreCase)))
                    {
                        evidence.Add(new Evidence
                        {
                            File = configFile,
                            Indicator = $"PackageReference: {id} {version}"
                        });
                    }
                }
            }
            catch
            {
                // Skip files we can't parse
            }
        }

        return evidence;
    }

    private List<Evidence> AnalyzeProjectReferences(string csprojPath)
    {
        var evidence = new List<Evidence>();

        try
        {
            var doc = XDocument.Load(csprojPath);
            var packageRefs = doc.Descendants("PackageReference");

            foreach (var pkgRef in packageRefs)
            {
                var include = pkgRef.Attribute("Include")?.Value ?? "";
                var version = pkgRef.Attribute("Version")?.Value
                    ?? pkgRef.Attribute("VersionOverride")?.Value
                    ?? "unknown";

                if (PackageDetection.AiSdkPackages.Any(p => include.Equals(p, StringComparison.OrdinalIgnoreCase)))
                {
                    evidence.Add(new Evidence
                    {
                        File = csprojPath,
                        Indicator = $"PackageReference: {include} {version}"
                    });
                }
                else if (PackageDetection.OtelPackages.Any(p => include.Equals(p, StringComparison.OrdinalIgnoreCase)))
                {
                    evidence.Add(new Evidence
                    {
                        File = csprojPath,
                        Indicator = $"PackageReference: {include} {version}"
                    });
                }
                else if (PackageDetection.AzureMonitorDistroPackages.Any(p => include.Equals(p, StringComparison.OrdinalIgnoreCase)))
                {
                    evidence.Add(new Evidence
                    {
                        File = csprojPath,
                        Indicator = $"PackageReference: {include} {version}"
                    });
                }
            }
        }
        catch
        {
            // Skip files we can't parse
        }

        return evidence;
    }

    private List<Evidence> CheckConfigFiles(string workspacePath)
    {
        var evidence = new List<Evidence>();

        // Check for applicationinsights.config (classic)
        var aiConfig = Directory.GetFiles(workspacePath, "applicationinsights.config", SearchOption.AllDirectories);
        foreach (var config in aiConfig)
        {
            evidence.Add(new Evidence
            {
                File = config,
                Indicator = "applicationinsights.config file present"
            });
        }

        // Check appsettings.json for instrumentation key or connection string
        var appSettings = Directory.GetFiles(workspacePath, "appsettings*.json", SearchOption.AllDirectories);
        foreach (var settings in appSettings)
        {
            try
            {
                var content = File.ReadAllText(settings);
                if (content.Contains("InstrumentationKey", StringComparison.OrdinalIgnoreCase))
                {
                    evidence.Add(new Evidence
                    {
                        File = settings,
                        Indicator = "InstrumentationKey found in configuration"
                    });
                }
                if (content.Contains("ApplicationInsights", StringComparison.OrdinalIgnoreCase))
                {
                    evidence.Add(new Evidence
                    {
                        File = settings,
                        Indicator = "ApplicationInsights configuration section found"
                    });
                }
            }
            catch
            {
                // Skip files we can't read
            }
        }

        return evidence;
    }

    private InstrumentationType DetermineInstrumentationType(List<Evidence> evidence)
    {
        var indicators = evidence.Select(e => e.Indicator).ToList();

        // Check for Azure Monitor Distro first (most specific)
        if (indicators.Any(i => PackageDetection.AzureMonitorDistroPackages.Any(p => i.Contains(p))))
            return InstrumentationType.AzureMonitorDistro;

        // Check for AI SDK
        if (indicators.Any(i => PackageDetection.AiSdkPackages.Any(p => i.Contains(p))))
            return InstrumentationType.ApplicationInsightsSdk;

        // Check for plain OpenTelemetry
        if (indicators.Any(i => PackageDetection.OtelPackages.Any(p => i.Contains(p))))
            return InstrumentationType.OpenTelemetry;

        return InstrumentationType.Other;
    }

    private string? ExtractVersion(List<Evidence> evidence)
    {
        // Try to extract version from package reference evidence
        foreach (var e in evidence)
        {
            if (e.Indicator.StartsWith("PackageReference:"))
            {
                var parts = e.Indicator.Split(' ');
                if (parts.Length >= 3)
                    return parts[2];
            }
        }
        return null;
    }

    /// <summary>
    /// Determines if the detected SDK is already on the target version:
    /// - ApplicationInsightsSdk: 3.x is target (2.x needs migration)
    /// - AzureMonitorDistro: any version is target (already on the recommended path)
    /// </summary>
    private static bool IsAlreadyOnTargetVersion(InstrumentationType type, string? version)
    {
        if (type == InstrumentationType.AzureMonitorDistro)
        {
            return true;
        }

        if (type != InstrumentationType.ApplicationInsightsSdk || string.IsNullOrWhiteSpace(version))
        {
            return false;
        }

        // Handle wildcard versions like "3.*"
        if (version.StartsWith("3.", StringComparison.Ordinal) || version.StartsWith("3-", StringComparison.Ordinal))
        {
            return true;
        }

        // Try to parse as a real version
        // Strip leading 'v' if present, handle pre-release suffix
        var versionToParse = version.TrimStart('v');
        var dashIndex = versionToParse.IndexOf('-');
        var versionCore = dashIndex > 0 ? versionToParse[..dashIndex] : versionToParse;

        if (Version.TryParse(versionCore, out var parsed))
        {
            return parsed.Major >= 3;
        }

        return false;
    }
}
