---
title: UseAzureMonitorExporter
category: api-reference
applies-to: 3.x
source: src/OpenTelemetryBuilderExtensions.cs
---

# UseAzureMonitorExporter

**Namespace:** `Azure.Monitor.OpenTelemetry.Exporter`

## When to use

- You want Azure Monitor export **without** the automatic instrumentation that `UseAzureMonitor` (distro) adds.
- You want a single call that registers exporters for **all three signals** (traces, metrics, logs).
- You want **Live Metrics** support (per-signal methods do not support it).
- You are adding Azure Monitor to a pipeline that already calls `AddOpenTelemetry()` with custom providers.

## Signatures

```csharp
public static IOpenTelemetryBuilder UseAzureMonitorExporter(
    this IOpenTelemetryBuilder builder);

public static IOpenTelemetryBuilder UseAzureMonitorExporter(
    this IOpenTelemetryBuilder builder,
    Action<AzureMonitorExporterOptions> configureAzureMonitor);
```

Automatically calls `WithTracing()`, `WithMetrics()`, and `WithLogging()`.
Sets the sampler, configures `IncludeFormattedMessage = true` on the logger,
and enables Live Metrics.

## Behavior notes

- **Cannot** be combined with per-signal methods (`AddAzureMonitorTraceExporter`, etc.) in the same app — a runtime exception is thrown.
- Chaining additional `WithTracing()` / `WithMetrics()` / `WithLogging()` calls adds configuration to the existing provider — it does not create a second one.
- See AzureMonitorExporter(see in AzureMonitorExporter.md) for the per-signal approach.

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
| `EnableLiveMetrics` | `bool` | `true` | Enables Live Metrics stream. |
| `EnableTraceBasedLogsSampler` | `bool` | `true` | When `true`, only logs correlated to sampled traces are exported. Logs without trace context are always exported. |

## Sampling

The exporter automatically configures a trace sampler — you do not need to set one yourself.

**Two modes**, controlled by `TracesPerSecond` and `SamplingRatio`:

| Mode | Activated when | Sampler | Example |
|---|---|---|---|
| Rate-limited (default) | `TracesPerSecond` is non-null | `RateLimitedSampler` — caps throughput to N traces/sec | `TracesPerSecond = 5.0` → ~5 traces/sec |
| Fixed-percentage | `TracesPerSecond` is `null` | `ApplicationInsightsSampler` — samples a fixed ratio | `SamplingRatio = 0.4F` → 40% of traces |

Because `TracesPerSecond` defaults to `5.0`, rate-limited sampling is active out of the box. To switch to fixed-percentage sampling:

```csharp
builder.Services.AddOpenTelemetry()
    .UseAzureMonitorExporter(o =>
    {
        o.TracesPerSecond = null;   // disable rate-limited sampling
        o.SamplingRatio = 0.25F;   // keep 25% of traces
    });
```

**Environment variable overrides** — `OTEL_TRACES_SAMPLER` and `OTEL_TRACES_SAMPLER_ARG`:

| `OTEL_TRACES_SAMPLER` | `OTEL_TRACES_SAMPLER_ARG` | Effect |
|---|---|---|
| `microsoft.rate_limited` | `5` | `TracesPerSecond = 5.0` |
| `microsoft.fixed_percentage` | `0.4` | `SamplingRatio = 0.4F`, `TracesPerSecond = null` |

Environment variables override `IConfiguration` values, which override code defaults.

**Log sampling** — when `EnableTraceBasedLogsSampler` is `true` (default), logs correlated to a sampled-out trace are also dropped. Logs without trace context are always exported.

## Minimal example

```csharp
using Azure.Monitor.OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

// Connection string from APPLICATIONINSIGHTS_CONNECTION_STRING env var
builder.Services.AddOpenTelemetry().UseAzureMonitorExporter();

var app = builder.Build();
app.Run();
```

## Full example

```csharp
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .UseAzureMonitorExporter(o =>
    {
        o.ConnectionString = builder.Configuration["AzureMonitor:ConnectionString"];
        o.Credential = new DefaultAzureCredential();
        o.TracesPerSecond = null;              // switch to fixed-percentage sampling
        o.SamplingRatio = 0.5F;               // keep 50% of traces
        o.EnableTraceBasedLogsSampler = false; // export all logs
    })
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("MyApp"));

var app = builder.Build();
app.Run();
```

### Non-hosted (console / CLI)

```csharp
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;

using var sdk = OpenTelemetrySdk.Create(builder => builder
    .UseAzureMonitorExporter()
    .WithTracing(tracing => tracing.AddSource("MyApp")));

// sdk.Dispose() flushes all signals on exit.
```

## See also

- UseAzureMonitor (distro)(see in UseAzureMonitor.md)
- Per-signal exporters (AzureMonitorExporter)(see in AzureMonitorExporter.md)
- AddOpenTelemetry(see in AddOpenTelemetry.md)
