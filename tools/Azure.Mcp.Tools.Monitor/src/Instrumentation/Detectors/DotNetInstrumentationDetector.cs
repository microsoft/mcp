using System.Xml.Linq;
using Azure.Mcp.Tools.MonitorInstrumentation.Models;
using static Azure.Mcp.Tools.MonitorInstrumentation.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Detectors;

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

        if (evidence.Count == 0)
        {
            return new InstrumentationResult(InstrumentationState.Greenfield, null);
        }

        // Determine instrumentation type from evidence
        var instrumentationType = DetermineInstrumentationType(evidence);
        var version = ExtractVersion(evidence);

        return new InstrumentationResult(
            InstrumentationState.Brownfield,
            new ExistingInstrumentation
            {
                Type = instrumentationType,
                Version = version,
                Evidence = evidence
            }
        );
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
}
