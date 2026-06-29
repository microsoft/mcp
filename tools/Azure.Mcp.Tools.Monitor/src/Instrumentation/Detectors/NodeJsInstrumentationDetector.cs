// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Monitor.Models.Instrumentation;

namespace Azure.Mcp.Tools.Monitor.Instrumentation.Detectors;

public class NodeJsInstrumentationDetector : IInstrumentationDetector
{
    public Language SupportedLanguage => Language.NodeJs;

    private static readonly string[] s_azureMonitorPackages = {
        "@azure/monitor-opentelemetry",
        "@azure/monitor-opentelemetry-exporter"
    };

    private static readonly string[] s_openTelemetryPackages = {
        "@opentelemetry/api",
        "@opentelemetry/sdk-node",
        "@opentelemetry/auto-instrumentations-node"
    };

    private static readonly string[] s_applicationInsightsPackages = {
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
            var azureMonitorFound = dependencies.Any(d => s_azureMonitorPackages.Contains(d));
            if (azureMonitorFound)
            {
                return new InstrumentationResult(
                    InstrumentationState.Brownfield,
                    new ExistingInstrumentation
                    {
                        Type = InstrumentationType.AzureMonitorDistro,
                        Evidence = [new(packageJsonPath, "Azure Monitor package found in dependencies")]
                    }
                );
            }

            // Check for OpenTelemetry packages
            var openTelemetryFound = dependencies.Any(d => s_openTelemetryPackages.Contains(d));
            if (openTelemetryFound)
            {
                return new InstrumentationResult(
                    InstrumentationState.Brownfield,
                    new ExistingInstrumentation
                    {
                        Type = InstrumentationType.OpenTelemetry,
                        Evidence = [new(packageJsonPath, "OpenTelemetry package found in dependencies")]
                    }
                );
            }

            // Check for Application Insights SDK
            var appInsightsFound = dependencies.Any(d => s_applicationInsightsPackages.Contains(d));
            if (appInsightsFound)
            {
                return new InstrumentationResult(
                    InstrumentationState.Brownfield,
                    new ExistingInstrumentation
                    {
                        Type = InstrumentationType.ApplicationInsightsSdk,
                        Evidence = [new(packageJsonPath, "Application Insights SDK found in dependencies")]
                    }
                );
            }

            // No instrumentation found
            return new(InstrumentationState.Greenfield, null);
        }
        catch (JsonException)
        {
            return new(InstrumentationState.Greenfield, null);
        }
    }
}
