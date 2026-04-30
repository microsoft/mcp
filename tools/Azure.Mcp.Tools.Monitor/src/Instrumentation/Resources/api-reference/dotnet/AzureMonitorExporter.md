---
title: AzureMonitorExporter
category: api-reference
applies-to: 3.x
source: src/AzureMonitorExporterExtensions.cs, src/AzureMonitorExporterOptions.cs
---

# AddAzureMonitor*Exporter

**Namespace:** `Azure.Monitor.OpenTelemetry.Exporter`

## When to use

- You are building a **console app, CLI tool, or background job** without the .NET Generic Host.
- You need to export **only a subset** of signals (e.g., traces and metrics but not logs).
- You need **different options per signal** (e.g., different credentials or storage directories).

## Signatures

```csharp
public static TracerProviderBuilder AddAzureMonitorTraceExporter(
    this TracerProviderBuilder builder,
    Action<AzureMonitorExporterOptions>? configure = null,
    TokenCredential? credential = null,
    string? name = null);

public static MeterProviderBuilder AddAzureMonitorMetricExporter(
    this MeterProviderBuilder builder,
    Action<AzureMonitorExporterOptions>? configure = null,
    TokenCredential? credential = null,
    string? name = null);

public static LoggerProviderBuilder AddAzureMonitorLogExporter(
    this LoggerProviderBuilder builder,
    Action<AzureMonitorExporterOptions>? configure = null,
    TokenCredential? credential = null,
    string? name = null);
```

A legacy overload extending `OpenTelemetryLoggerOptions` also exists but is superseded by the `LoggerProviderBuilder` version above.

## Behavior notes

- **Cannot** be combined with `UseAzureMonitorExporter` in the same app — a runtime exception is thrown.
- **Live Metrics** is not supported by per-signal methods. Setting `EnableLiveMetrics = true` has no effect — a warning is logged and the setting is ignored.
- For hosted apps (ASP.NET Core, Worker Services), prefer `UseAzureMonitorExporter()`(see in UseAzureMonitorExporter.md) — it registers all three signals in one call with Live Metrics support.

## AzureMonitorExporterOptions

Inherits `Azure.Core.ClientOptions`.

| Property | Type | Default | Description |
|---|---|---|---|
| `ConnectionString` | `string` | `null` | Application Insights connection string. Falls back to `APPLICATIONINSIGHTS_CONNECTION_STRING` env var. |
| `Credential` | `TokenCredential` | `null` | Azure AD token credential for Entra-based auth. |
| `SamplingRatio` | `float` | `1.0F` | Fixed-percentage sampling ratio (0.0–1.0). Only used when `TracesPerSecond` is `null`. |
| `TracesPerSecond` | `double?` | `5.0` | Rate-limited sampling target. Takes precedence over `SamplingRatio`. Set to `null` to use fixed-percentage sampling. |
| `StorageDirectory` | `string` | `null` | Custom directory for offline/retry storage. Uses OS temp by default. |
| `DisableOfflineStorage` | `bool` | `false` | Disables persistent storage for failed telemetry. |
| `EnableLiveMetrics` | `bool` | `true` | **Ignored** by per-signal methods. Only applies with `UseAzureMonitorExporter`. |
| `EnableTraceBasedLogsSampler` | `bool` | `true` | When `true`, only logs correlated to sampled traces are exported. Logs without trace context are always exported. |

## Sampling

`AddAzureMonitorTraceExporter` automatically configures a trace sampler — you do not need to set one yourself.

**Two modes**, controlled by `TracesPerSecond` and `SamplingRatio`:

| Mode | Activated when | Sampler | Example |
|---|---|---|---|
| Rate-limited (default) | `TracesPerSecond` is non-null | `RateLimitedSampler` — caps throughput to N traces/sec | `TracesPerSecond = 5.0` → ~5 traces/sec |
| Fixed-percentage | `TracesPerSecond` is `null` | `ApplicationInsightsSampler` — samples a fixed ratio | `SamplingRatio = 0.4F` → 40% of traces |

**Environment variable overrides** — `OTEL_TRACES_SAMPLER` / `OTEL_TRACES_SAMPLER_ARG`:

| `OTEL_TRACES_SAMPLER` | `OTEL_TRACES_SAMPLER_ARG` | Effect |
|---|---|---|
| `microsoft.rate_limited` | `5` | `TracesPerSecond = 5.0` |
| `microsoft.fixed_percentage` | `0.4` | `SamplingRatio = 0.4F`, `TracesPerSecond = null` |

**Log sampling** — when `EnableTraceBasedLogsSampler` is `true` (default), logs correlated to a sampled-out trace are also dropped.

## Minimal example

```csharp
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;
using OpenTelemetry.Trace;

// Connection string from APPLICATIONINSIGHTS_CONNECTION_STRING env var
using var sdk = OpenTelemetrySdk.Create(builder => builder
    .WithTracing(tracing => tracing
        .AddSource("MyApp")
        .AddAzureMonitorTraceExporter()));

// sdk.Dispose() flushes and shuts down on exit.
```

## Full example

```csharp
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var credential = new DefaultAzureCredential();
var connStr = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

using var sdk = OpenTelemetrySdk.Create(builder => builder
    .ConfigureResource(r => r.AddService("my-console-app"))
    .WithTracing(tracing => tracing
        .AddSource("MyApp")
        .AddHttpClientInstrumentation()
        .AddAzureMonitorTraceExporter(
            configure: o =>
            {
                o.ConnectionString = connStr;
                o.TracesPerSecond = null;
                o.SamplingRatio = 0.5F;
            },
            credential: credential))
    .WithMetrics(metrics => metrics
        .AddMeter("MyApp")
        .AddAzureMonitorMetricExporter(
            configure: o => o.ConnectionString = connStr,
            credential: credential)));
// Logging intentionally omitted — only traces and metrics exported

// sdk.Dispose() flushes all signals on exit.
```

## See also

- UseAzureMonitorExporter (all signals)(see in UseAzureMonitorExporter.md)
- WithTracing(see in WithTracing.md) · WithMetrics(see in WithMetrics.md) · WithLogging(see in WithLogging.md)
