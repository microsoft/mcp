---
title: AddApplicationInsightsTelemetryWorkerService
category: api-reference
applies-to: 3.x
source: NETCORE/src/Microsoft.ApplicationInsights.WorkerService/ApplicationInsightsExtensions.cs
---

# AddApplicationInsightsTelemetryWorkerService

**Package:** `Microsoft.ApplicationInsights.WorkerService` (3.x)

## Signatures

```csharp
using Microsoft.Extensions.DependencyInjection;

// 1. Parameterless — reads config from appsettings.json / env vars
public static IServiceCollection AddApplicationInsightsTelemetryWorkerService(
    this IServiceCollection services);

// 2. IConfiguration — binds "ApplicationInsights" section
public static IServiceCollection AddApplicationInsightsTelemetryWorkerService(
    this IServiceCollection services,
    IConfiguration configuration);

// 3. Action delegate — configure options inline
public static IServiceCollection AddApplicationInsightsTelemetryWorkerService(
    this IServiceCollection services,
    Action<ApplicationInsightsServiceOptions> options);

// 4. Options instance — pass a pre-built options object
public static IServiceCollection AddApplicationInsightsTelemetryWorkerService(
    this IServiceCollection services,
    ApplicationInsightsServiceOptions options);
```

All overloads return `IServiceCollection`. Overloads 2–4 call overload 1 internally, then apply configuration.

## ApplicationInsightsServiceOptions

Namespace: `Microsoft.ApplicationInsights.WorkerService`

| Property | Type | Default | Description |
|---|---|---|---|
| `ConnectionString` | `string` | `null` | Connection string for Application Insights. Can also be set via env var or config (see below). |
| `Credential` | `TokenCredential` | `null` | AAD credential for token-based authentication. When null, the instrumentation key from the connection string is used. |
| `ApplicationVersion` | `string` | Entry assembly version | Application version reported with telemetry. |
| `EnableQuickPulseMetricStream` | `bool` | `true` | Enables Live Metrics. |
| `EnablePerformanceCounterCollectionModule` | `bool` | `true` | Enables performance counter collection. |
| `EnableDependencyTrackingTelemetryModule` | `bool` | `true` | Enables HTTP and SQL dependency tracking. |
| `AddAutoCollectedMetricExtractor` | `bool` | `true` | Enables standard metric extraction. |
| `TracesPerSecond` | `double?` | `null` (effective: `5`) | Rate-limited sampling — targets this many traces per second. Must be ≥ 0. |
| `SamplingRatio` | `float?` | `null` | Fixed-rate sampling (0.0–1.0, where 1.0 = no sampling). When set, overrides `TracesPerSecond`. |
| `EnableTraceBasedLogsSampler` | `bool?` | `null` (effective: `true`) | When true, logs are sampled with their parent trace. Set `false` to collect all logs. |

**Note:** Unlike the ASP.NET Core package, WorkerService does not include `EnableRequestTrackingTelemetryModule` or `EnableAuthenticationTrackingJavaScript`.

## Minimal example

```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddApplicationInsightsTelemetryWorkerService();
});
var host = builder.Build();
await host.RunAsync();
```

Set the connection string via environment variable:
```
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=...;IngestionEndpoint=...
```

Or in `appsettings.json`:
```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=...;IngestionEndpoint=..."
  }
}
```

## Full example

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.WorkerService;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddApplicationInsightsTelemetryWorkerService(options =>
    {
        options.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
        options.EnableQuickPulseMetricStream = true;
        options.SamplingRatio = 0.5f;    // Collect 50% of telemetry
    });
});
var host = builder.Build();
await host.RunAsync();
```

## Behavior notes

- Connection string resolution order: `ApplicationInsightsServiceOptions.ConnectionString` → env var `APPLICATIONINSIGHTS_CONNECTION_STRING` → config key `ApplicationInsights:ConnectionString`.
- `TracesPerSecond` is the default sampling mode (effective default `5`). Set `SamplingRatio` for fixed-rate sampling instead.
- Additional OTel sources/meters can be added: `services.AddOpenTelemetry().WithTracing(t => t.AddSource("MySource"))`.
## See also

- UseAzureMonitor (distro)(see in UseAzureMonitor.md)
- UseAzureMonitorExporter(see in UseAzureMonitorExporter.md)
- Worker Service 2.x → 3.x Migration(see in workerservice-2x-to-3x-code-migration.md)
