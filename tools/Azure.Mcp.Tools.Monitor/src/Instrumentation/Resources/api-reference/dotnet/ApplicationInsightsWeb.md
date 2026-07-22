---
title: ApplicationInsightsWeb
category: api-reference
applies-to: 3.x
---

# Microsoft.ApplicationInsights.Web — API Reference

## Package

```
Microsoft.ApplicationInsights.Web
```

Target: .NET Framework 4.6.2+

## ApplicationInsights.config elements

| Element | Default | Description |
|---|---|---|
| `ConnectionString` | — | **Required.** Azure Monitor connection string. |
| `DisableTelemetry` | `false` | Disable all telemetry collection. |
| `ApplicationVersion` | — | Sets `service.version` resource attribute. |
| `TracesPerSecond` | `5.0` | Rate-limited sampling (traces per second). |
| `SamplingRatio` | — | Fixed-rate sampling (0.0–1.0). Overrides `TracesPerSecond` if set. |
| `EnableTraceBasedLogsSampler` | `true` | Logs follow parent trace sampling decision. |
| `StorageDirectory` | — | Directory for offline telemetry storage. |
| `DisableOfflineStorage` | `false` | Disable offline storage. |
| `EnableQuickPulseMetricStream` | `true` | Enable Live Metrics. |
| `EnablePerformanceCounterCollectionModule` | `true` | Collect performance counters. |
| `EnableDependencyTrackingTelemetryModule` | `true` | Track SQL/HTTP dependencies. |
| `EnableRequestTrackingTelemetryModule` | `true` | Track incoming HTTP requests. |
| `AddAutoCollectedMetricExtractor` | `true` | Extract standard metrics. |

## Removed in 3.x (from 2.x)

| Element/Section | Status |
|---|---|
| `<InstrumentationKey>` | **Removed** — use `<ConnectionString>` |
| `<TelemetryInitializers>` | **Removed** — initializers are now internal Activity Processors |
| `<TelemetryModules>` | **Removed** — modules are auto-configured internally |
| `<TelemetryProcessors>` | **Removed** — use `ConfigureOpenTelemetryBuilder` for custom processors |
| `<TelemetryChannel>` | **Removed** — export pipeline managed by OpenTelemetry |

## HTTP modules required in Web.config

### IIS Integrated mode (`system.webServer/modules`)

```xml
<add name="ApplicationInsightsWebTracking"
     type="Microsoft.ApplicationInsights.Web.ApplicationInsightsHttpModule, Microsoft.AI.Web"
     preCondition="managedHandler" />
<add name="TelemetryHttpModule"
     type="OpenTelemetry.Instrumentation.AspNet.TelemetryHttpModule, OpenTelemetry.Instrumentation.AspNet.TelemetryHttpModule"
     preCondition="integratedMode,managedHandler" />
```

### IIS Classic mode (`system.web/httpModules`)

```xml
<add name="ApplicationInsightsWebTracking"
     type="Microsoft.ApplicationInsights.Web.ApplicationInsightsHttpModule, Microsoft.AI.Web" />
```

> These are auto-configured by the NuGet package install.

## TelemetryClient usage

`TelemetryClient` works the same as in ASP.NET Core 3.x. See TelemetryClient.md for breaking changes.

Key difference: in classic ASP.NET, create via `new TelemetryClient(TelemetryConfiguration.CreateDefault())` or let `ApplicationInsightsHttpModule` initialize the configuration first.

```csharp
var client = new TelemetryClient(TelemetryConfiguration.CreateDefault());
client.TrackEvent("OrderCreated", new Dictionary<string, string> { ["OrderId"] = orderId });
client.TrackMetric("ProcessingTime", elapsed);
```

## ConfigureOpenTelemetryBuilder

The primary extensibility point in 3.x for classic ASP.NET. Add custom processors, exporters, or resource attributes:

```csharp
// In Global.asax.cs Application_Start()
var config = TelemetryConfiguration.CreateDefault();
config.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
config.ConfigureOpenTelemetryBuilder(otel =>
{
    otel.WithTracing(tracing =>
    {
        tracing.AddProcessor<MyFilterProcessor>();
        tracing.AddConsoleExporter();    // For debugging
    });
    otel.ConfigureResource(r => r.AddService("MyWebApp"));
});
```

## Notes

- The NuGet package install handles all configuration — no code changes needed for basic setup.
- `TelemetryCorrelationHttpModule` is **not needed** in 3.x — OpenTelemetry handles correlation natively.
- Connection string can be set in `ApplicationInsights.config`, in code, or via `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable.
- **Non-DI:** Classic ASP.NET does not use `IServiceCollection`. Use `TelemetryConfiguration.ConfigureOpenTelemetryBuilder` for extensibility.
