// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Runtime.InteropServices;
using Azure.Mcp.Core.Configuration;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Azure.Mcp.Core.Extensions;

public static class OpenTelemetryExtensions
{
    private const string DefaultAppInsights = "InstrumentationKey=21e003c0-efee-4d3f-8a98-1868515aa2c9;IngestionEndpoint=https://centralus-2.in.applicationinsights.azure.com/;LiveEndpoint=https://centralus.livediagnostics.monitor.azure.com/;ApplicationId=f14f6a2d-6405-4f88-bd58-056f25fe274f";

    public static void ConfigureOpenTelemetry(this IServiceCollection services)
    {
        services.AddOptions<AzureMcpServerConfiguration>()
            .Configure(options =>
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                var assemblyName = entryAssembly?.GetName() ?? new AssemblyName();
                if (assemblyName?.Version != null)
                {
                    options.Version = assemblyName.Version.ToString();
                }

                var collectTelemetry = Environment.GetEnvironmentVariable("AZURE_MCP_COLLECT_TELEMETRY");

                options.IsTelemetryEnabled = string.IsNullOrEmpty(collectTelemetry)
                    || (bool.TryParse(collectTelemetry, out var shouldCollect) && shouldCollect);
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

        var appInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

        if (string.IsNullOrEmpty(appInsightsConnectionString))
        {
            appInsightsConnectionString = DefaultAppInsights;
        }

        services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
        {
            var serverConfig = sp.GetRequiredService<IOptions<AzureMcpServerConfiguration>>();
            if (!serverConfig.Value.IsTelemetryEnabled)
            {
                return;
            }

            builder.AddSource(serverConfig.Value.Name);
        });

        services.AddOpenTelemetry()
            .ConfigureResource(r =>
            {
                var version = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString();

                r.AddService("azmcp", version)
                    .AddTelemetrySdk();
            })
            .UseAzureMonitorExporter(options =>
            {
#if DEBUG
                options.EnableLiveMetrics = true;
                options.Diagnostics.IsLoggingEnabled = true;
                options.Diagnostics.IsLoggingContentEnabled = true;
#endif
                options.ConnectionString = appInsightsConnectionString;
            });
    }
}
