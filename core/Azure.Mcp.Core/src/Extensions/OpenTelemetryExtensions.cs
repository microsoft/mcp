// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Runtime.InteropServices;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Configuration;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Monitor.OpenTelemetry.Exporter; // Don't believe this is unused, it is needed for UseAzureMonitorExporter
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Azure.Mcp.Core.Extensions;

public static class OpenTelemetryExtensions
{
    // private const string DefaultAppInsights = "InstrumentationKey=21e003c0-efee-4d3f-8a98-1868515aa2c9;IngestionEndpoint=https://centralus-2.in.applicationinsights.azure.com/;LiveEndpoint=https://centralus.livediagnostics.monitor.azure.com/;ApplicationId=f14f6a2d-6405-4f88-bd58-056f25fe274f";

    private const string DefaultAppInsights = "InstrumentationKey=a0fa69bf-6f60-4778-b767-043583e5f02c;IngestionEndpoint=https://eastus2-3.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus2.livediagnostics.monitor.azure.com/;ApplicationId=b8ec4bb1-32af-4701-8914-54b1e2286a09";

    public static void ConfigureOpenTelemetry(this IServiceCollection services)
    {
        services.AddOptions<AzureMcpServerConfiguration>()
            .Configure<IOptions<ServiceStartOptions>>((options, serviceStartOptions) =>
            {
                // Assembly.GetEntryAssembly is used to retrieve the version of the server application as that is
                // the assembly that will run the tool calls.
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    options.Version = GetServerVersion(entryAssembly);
                }

                var collectTelemetry = Environment.GetEnvironmentVariable("AZURE_MCP_COLLECT_TELEMETRY");

                options.IsTelemetryEnabled = string.IsNullOrEmpty(collectTelemetry) || (bool.TryParse(collectTelemetry, out var shouldCollect) && shouldCollect);
                
                Console.WriteLine($"ConfigureOpenTelemetry method - Telemetry Enabled: {options.IsTelemetryEnabled}");
            });

        services.AddSingleton<ITelemetryService, TelemetryService>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            services.AddSingleton<IMachineInformationProvider, WindowsMachineInformationProvider>();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            services.AddSingleton<IMachineInformationProvider, MacOSXMachineInformationProvider>();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            services.AddSingleton<IMachineInformationProvider, LinuxMachineInformationProvider>();
        }
        else
        {
            services.AddSingleton<IMachineInformationProvider, DefaultMachineInformationProvider>();
        }

        EnableAzureMonitor(services);
    }

    public static void ConfigureOpenTelemetryLogger(this ILoggingBuilder builder)
    {
        builder.AddOpenTelemetry(logger =>
        {
            logger.AddProcessor(new TelemetryLogRecordEraser());
        });
    }

    private static void EnableAzureMonitor(this IServiceCollection services)
    {
#if DEBUG
        services.AddSingleton(sp =>
        {
            var forwarder = new AzureEventSourceLogForwarder(sp.GetRequiredService<ILoggerFactory>());
            forwarder.Start();
            return forwarder;
        });
#endif

        services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
        {
            var serverConfig = sp.GetRequiredService<IOptions<AzureMcpServerConfiguration>>();
            if (!serverConfig.Value.IsTelemetryEnabled)
            {
                return;
            }

            builder.AddSource(serverConfig.Value.Name);
        });

        var otelBuilder = services.AddOpenTelemetry()
            .ConfigureResource(r =>
            {
                var version = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString();

                r.AddService("azmcp", version)
                    .AddTelemetrySdk();
            });

        var appInsightsConnectionStrings = new List<(string Name, string ConnectionString)>();

        var userProvidedAppInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

        if (!string.IsNullOrEmpty(userProvidedAppInsightsConnectionString))
        {
            appInsightsConnectionStrings.Add(("UserProvided", userProvidedAppInsightsConnectionString));
        }

        appInsightsConnectionStrings.Add(("Microsoft", DefaultAppInsights));

        ConfigureMultipleAzureMonitorExporters(otelBuilder, appInsightsConnectionStrings);

#if RELEASE
        otelBuilder.UseAzureMonitorExporter(options =>
        {
            options.ConnectionString = appInsightsConnectionString;
        });
#endif

        var enableOtlp = Environment.GetEnvironmentVariable("AZURE_MCP_ENABLE_OTLP_EXPORTER");
        if (!string.IsNullOrEmpty(enableOtlp) && bool.TryParse(enableOtlp, out var shouldEnable) && shouldEnable)
        {
            otelBuilder.WithTracing(tracing => tracing.AddOtlpExporter())
                .WithMetrics(metrics => metrics.AddOtlpExporter())
                .WithLogging(logging => logging.AddOtlpExporter());
        }
    }

    /// <summary>
    /// Gets the version information for the server.  Uses logic from Azure SDK for .NET to generate the same version string.
    /// https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/core/System.ClientModel/src/Pipeline/UserAgentPolicy.cs#L91
    /// For example, an informational version of "6.14.0-rc.116+54d611f7" will return "6.14.0-rc.116"
    /// </summary>
    /// <param name="entryAssembly">The entry assembly to extract name and version information from.</param>
    /// <returns>A version string.</returns>
    internal static string GetServerVersion(Assembly entryAssembly)
    {
        AssemblyInformationalVersionAttribute? versionAttribute = entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (versionAttribute == null)
        {
            throw new InvalidOperationException(
                $"{nameof(AssemblyInformationalVersionAttribute)} is required on client SDK assembly '{entryAssembly.FullName}'.");
        }

        string version = versionAttribute.InformationalVersion;

        int hashSeparator = version.IndexOf('+');
        if (hashSeparator != -1)
        {
            version = version.Substring(0, hashSeparator);
        }

        return version;
    }


    private static void ConfigureMultipleAzureMonitorExporters(OpenTelemetry.OpenTelemetryBuilder otelBuilder, List<(string Name, string ConnectionString)> appInsightsConnectionStrings)
    {
        foreach (var exporter in appInsightsConnectionStrings)
        {
            otelBuilder.WithLogging(logging =>
            {
                logging.AddAzureMonitorLogExporter(options =>
                {
                    options.ConnectionString = exporter.ConnectionString;
                },
                name: exporter.Name);
            });

            otelBuilder.WithMetrics(metrics =>
            {
                metrics.AddAzureMonitorMetricExporter(options =>
                {
                    options.ConnectionString = exporter.ConnectionString;
                },
                name: exporter.Name);
            });

            otelBuilder.WithTracing(tracing =>
            {
                tracing.AddAzureMonitorTraceExporter(options =>
                {
                    options.ConnectionString = exporter.ConnectionString;
                },
                name: exporter.Name);
            });
        }
    }
}
