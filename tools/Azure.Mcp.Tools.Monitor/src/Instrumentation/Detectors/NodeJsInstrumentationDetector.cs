using System.Text.Json;
using Azure.Mcp.Tools.Monitor.Models;

namespace Azure.Mcp.Tools.Monitor.Detectors;

public class NodeJsInstrumentationDetector : IInstrumentationDetector
{
    public Language SupportedLanguage => Language.NodeJs;

    private static readonly string[] AzureMonitorPackages = {
        "@azure/monitor-opentelemetry",
        "@azure/monitor-opentelemetry-exporter"
    };

    private static readonly string[] OpenTelemetryPackages = {
        "@opentelemetry/api",
        "@opentelemetry/sdk-node",
        "@opentelemetry/auto-instrumentations-node"
    };

    private static readonly string[] ApplicationInsightsPackages = {
        "applicationinsights"
    };

    public InstrumentationResult Detect(string workspacePath)
    {

        var packageJsonPath = Path.Combine(workspacePath, "package.json");
        if (!File.Exists(packageJsonPath))
        {
            return new InstrumentationResult(
                InstrumentationState.Greenfield,
                null
            );
        }

        try
        {
            var packageJson = JsonDocument.Parse(File.ReadAllText(packageJsonPath));
            var root = packageJson.RootElement;

            var dependencies = new List<string>();

            // Collect all dependencies
            if (root.TryGetProperty("dependencies", out var depsElement))
            {
                foreach (var dep in depsElement.EnumerateObject())
                {
                    dependencies.Add(dep.Name);
                }
            }

            if (root.TryGetProperty("devDependencies", out var devDepsElement))
            {
                foreach (var dep in devDepsElement.EnumerateObject())
                {
                    dependencies.Add(dep.Name);
                }
            }

            // Check for Azure Monitor packages
            var azureMonitorFound = dependencies.Any(d => AzureMonitorPackages.Contains(d));
            if (azureMonitorFound)
            {
                return new InstrumentationResult(
                    InstrumentationState.Brownfield,
                    new ExistingInstrumentation
                    {
                        Type = InstrumentationType.AzureMonitorDistro,
                        Evidence = [new Evidence { File = packageJsonPath, Indicator = "Azure Monitor package found in dependencies" }]
                    }
                );
            }

            // Check for OpenTelemetry packages
            var openTelemetryFound = dependencies.Any(d => OpenTelemetryPackages.Contains(d));
            if (openTelemetryFound)
            {
                return new InstrumentationResult(
                    InstrumentationState.Brownfield,
                    new ExistingInstrumentation
                    {
                        Type = InstrumentationType.OpenTelemetry,
                        Evidence = [new Evidence { File = packageJsonPath, Indicator = "OpenTelemetry package found in dependencies" }]
                    }
                );
            }

            // Check for Application Insights SDK
            var appInsightsFound = dependencies.Any(d => ApplicationInsightsPackages.Contains(d));
            if (appInsightsFound)
            {
                return new InstrumentationResult(
                    InstrumentationState.Brownfield,
                    new ExistingInstrumentation
                    {
                        Type = InstrumentationType.ApplicationInsightsSdk,
                        Evidence = [new Evidence { File = packageJsonPath, Indicator = "Application Insights SDK found in dependencies" }]
                    }
                );
            }

            // No instrumentation found
            return new InstrumentationResult(
                InstrumentationState.Greenfield,
                null
            );
        }
        catch (JsonException)
        {
            return new InstrumentationResult(
                InstrumentationState.Greenfield,
                null
            );
        }
    }
}
