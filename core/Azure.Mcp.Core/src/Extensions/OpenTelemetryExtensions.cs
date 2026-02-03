// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Runtime.InteropServices;
using Azure.Mcp.Core.Configuration;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Commands;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Azure.Mcp.Core.Extensions;

public static class OpenTelemetryExtensions
{
    /// <summary>
    /// The App Insights connection string to send telemetry to Microsoft.
    /// </summary>
    private const string MicrosoftOwnedAppInsightsConnectionString = "InstrumentationKey=21e003c0-efee-4d3f-8a98-1868515aa2c9;IngestionEndpoint=https://centralus-2.in.applicationinsights.azure.com/;LiveEndpoint=https://centralus.livediagnostics.monitor.azure.com/;ApplicationId=f14f6a2d-6405-4f88-bd58-056f25fe274f";

    public static void ConfigureOpenTelemetry(this IServiceCollection services)
    {
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
        // Intentionally left empty. OpenTelemetry logging is configured as part of EnableAzureMonitor
        // when a telemetry destination (for example, a user-provided Application Insights instance)
        // is enabled. This method does not perform any logging configuration itself.
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

        var userProvidedAppInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

        if (!string.IsNullOrWhiteSpace(userProvidedAppInsightsConnectionString))
        {
            // Configure telemetry to be sent to user-provided Application Insights instance regardless of build configuration.
            ConfigureUserProvidedAzureMonitorExporter(otelBuilder, userProvidedAppInsightsConnectionString);
        }

        // Configure Microsoft-owned telemetry only in RELEASE builds to avoid polluting telemetry during development.
#if RELEASE
        // This environment variable can be used to disable Microsoft telemetry collection.
        // By default, Microsoft telemetry is enabled.
        var microsoftTelemetry = Environment.GetEnvironmentVariable("AZURE_MCP_COLLECT_TELEMETRY_MICROSOFT");

        bool shouldCollectMicrosoftTelemetry = string.IsNullOrWhiteSpace(microsoftTelemetry) || (bool.TryParse(microsoftTelemetry, out var shouldCollect) && shouldCollect);

        if (shouldCollectMicrosoftTelemetry)
        {
            ConfigureMicrosoftAzureMonitorExporter(otelBuilder, MicrosoftOwnedAppInsightsConnectionString);
        }
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
    /// Configures OpenTelemetry to use Azure Monitor exporters with Microsoft's Application Insights instance.
    /// </summary>
    /// <param name="otelBuilder">The OpenTelemetry builder to configure.</param>
    /// <param name="appInsightsConnectionString">The Application Insights connection string for Microsoft's telemetry instance.</param>
    private static void ConfigureMicrosoftAzureMonitorExporter(OpenTelemetry.OpenTelemetryBuilder otelBuilder, string appInsightsConnectionString)
    {
        otelBuilder.WithMetrics(metrics =>
        {
            metrics.AddAzureMonitorMetricExporter(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            },
            name: AppInsightsInstanceType.Microsoft);
        });

        otelBuilder.WithTracing(tracing =>
        {
            tracing.AddAzureMonitorTraceExporter(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            },
            name: AppInsightsInstanceType.Microsoft);
        });
    }

    /// <summary>
    /// Configures OpenTelemetry to use Azure Monitor exporters with a user-provided Application Insights connection string.
    /// </summary>
    /// <param name="otelBuilder">The OpenTelemetry builder to configure.</param>
    /// <param name="appInsightsConnectionString">The Application Insights connection string provided by the user.</param>
    private static void ConfigureUserProvidedAzureMonitorExporter(OpenTelemetry.OpenTelemetryBuilder otelBuilder, string appInsightsConnectionString)
    {
        otelBuilder.WithLogging(logging =>
        {
            logging.AddAzureMonitorLogExporter(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            },
            name: AppInsightsInstanceType.UserProvided);
        });

        otelBuilder.WithMetrics(metrics =>
        {
            metrics.AddAzureMonitorMetricExporter(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            },
            name: AppInsightsInstanceType.UserProvided);
        });

        otelBuilder.WithTracing(tracing =>
        {
            tracing.AddAzureMonitorTraceExporter(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            },
            name: AppInsightsInstanceType.UserProvided);
        });
    }
}
