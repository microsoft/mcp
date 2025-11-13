// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using Azure.Mcp.Core.Configuration;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Monitor.OpenTelemetry.Exporter; // Don't believe this is unused, it is needed for UseAzureMonitorExporter
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Azure.Mcp.Core.Extensions;

public static class OpenTelemetryExtensions
{
    public static void ConfigureTelemetryServices(this IServiceCollection services,
        IHostEnvironment hostEnvironment, IConfiguration configuration)
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

        ConfigureOpenTelemetry(services, hostEnvironment, configuration);
    }

    private static void ConfigureOpenTelemetry(this IServiceCollection services,
        IHostEnvironment hostEnvironment,
        IConfiguration configuration)
    {
        if (hostEnvironment.IsDevelopment())
        {
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>();
                var forwarder = new AzureEventSourceLogForwarder(logger);
                forwarder.Start();
                return forwarder;
            });
        }

        services.AddSingleton<IResourceDetector, AzureMcpServerResourceDetector>();

        // Note: Turn on all the signals for metrics, tracing, and logging.
        // The configuration for each of these components is done further down
        // because we need to resolve classes using IServiceProvider.
        //
        // There is a single resource we gather information from.
        var otelBuilder = services.AddOpenTelemetry()
            .ConfigureResource(builder =>
            {
                builder.AddDetector(sp => sp.GetRequiredService<IResourceDetector>());
            })
            .WithMetrics()
            .WithTracing()
            .WithLogging(builder =>
            {
                builder.AddProcessor(new TelemetryLogRecordEraser());
            });

        // Metrics configuration
        services.ConfigureOpenTelemetryMeterProvider((sp, builder) =>
        {
            var config = sp.GetRequiredService<IOptions<AzureMcpServerConfiguration>>().Value;
            if (!config.IsTelemetryEnabled)
            {
                return;
            }

            builder.AddAzureMonitorMetricExporter(options =>
            {
                options.ConnectionString = config.ApplicationInsightsConnectionString;
            });

            if (config.IsOtelExporterEnabled)
            {
                builder.AddOtlpExporter();
            }
        });

        // Tracer configuration
        services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
        {
            var config = sp.GetRequiredService<IOptions<AzureMcpServerConfiguration>>().Value;
            if (!config.IsTelemetryEnabled)
            {
                return;
            }

            // Matches the ActivitySource created in ITelemetryService.
            builder.AddSource(config.Name);

            builder.AddAzureMonitorTraceExporter(options =>
            {
                options.ConnectionString = config.ApplicationInsightsConnectionString;
            });

            if (config.IsOtelExporterEnabled)
            {
                builder.AddOtlpExporter();
            }
        });

        // Tracer configuration
        services.ConfigureOpenTelemetryLoggerProvider((sp, builder) =>
        {
            var config = sp.GetRequiredService<IOptions<AzureMcpServerConfiguration>>().Value;
            if (!config.IsTelemetryEnabled)
            {
                return;
            }

            builder.AddAzureMonitorLogExporter(options =>
            {
                options.ConnectionString = config.ApplicationInsightsConnectionString;
            });

            if (config.IsOtelExporterEnabled)
            {
                builder.AddOtlpExporter();
            }
        });
    }
}
