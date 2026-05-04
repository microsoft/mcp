---
title: Application Insights 2.x to 3.x Migration
category: migration
applies-to: 3.x
---

# App Insights 2.x → 3.x Code Migration

## What changed

3.x uses OpenTelemetry under the hood. The main entry point is the same — `AddApplicationInsightsTelemetry()` — but several options and extension methods were removed.

Key changes:
- `InstrumentationKey` → use `ConnectionString`
- `EnableAdaptiveSampling` → use `TracesPerSecond` (default `5`) or `SamplingRatio`
- Logging is automatic — `ApplicationInsightsLoggerProvider` was removed
- New: `Credential` for AAD authentication, `EnableTraceBasedLogsSampler` for log sampling control

## Before / after

**2.x**
```csharp
using Microsoft.Extensions.DependencyInjection;

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.InstrumentationKey = "your-ikey";          // Removed in 3.x
    options.EnableAdaptiveSampling = false;             // Removed in 3.x
    options.DeveloperMode = true;                       // Removed in 3.x
});
```

**3.x**
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
    options.SamplingRatio = 1.0f;       // No sampling (collect everything)
    // DeveloperMode — no replacement needed, remove the line
});
```

Or set `APPLICATIONINSIGHTS_CONNECTION_STRING` as an environment variable and call `AddApplicationInsightsTelemetry()` with no arguments.

## Property changes

| Property | Status | Action required |
|---|---|---|
| `ConnectionString` | Unchanged | None. |
| `ApplicationVersion` | Unchanged | None. |
| `EnableQuickPulseMetricStream` | Unchanged | None. Default `true`. |
| `EnablePerformanceCounterCollectionModule` | Unchanged | None. Default `true`. |
| `EnableDependencyTrackingTelemetryModule` | Unchanged | None. Default `true`. |
| `EnableRequestTrackingTelemetryModule` | Unchanged | None. Default `true`. |
| `AddAutoCollectedMetricExtractor` | Unchanged | None. Default `true`. |
| `EnableAuthenticationTrackingJavaScript` | Unchanged | None. Default `false`. |
| `InstrumentationKey` | **Removed** | Use `ConnectionString`. |
| `EnableAdaptiveSampling` | **Removed** | Use `TracesPerSecond` or `SamplingRatio`. |
| `DeveloperMode` | **Removed** | Delete the line. |
| `EndpointAddress` | **Removed** | Endpoint is now part of `ConnectionString`. |
| `EnableHeartbeat` | **Removed** | Delete the line; heartbeat is automatic. |
| `EnableDebugLogger` | **Removed** | Delete the line. |
| `RequestCollectionOptions` | **Removed** | Delete the line. |
| `DependencyCollectionOptions` | **Removed** | Delete the line. |
| `TelemetryInitializers` | **Removed** | Delete the line. |
| `Credential` | **New** | `TokenCredential`, default `null`. Set for AAD auth. |
| `TracesPerSecond` | **New** | `double?`, effective default `5`. Rate-limited sampling. |
| `SamplingRatio` | **New** | `float?`, default `null`. Fixed-rate sampling (0.0–1.0). |
| `EnableTraceBasedLogsSampler` | **New** | `bool?`, effective default `true`. Logs follow parent trace sampling. |

## Removed extension methods

| Method | Replacement |
|---|---|
| `AddApplicationInsightsTelemetry(string instrumentationKey)` | Use parameterless overload + `ConnectionString` in options or env var. |
| `UseApplicationInsights()` (all `IWebHostBuilder` overloads) | Use `AddApplicationInsightsTelemetry()` on `IServiceCollection`. |
| `AddApplicationInsightsTelemetryProcessor<T>()` | Use OpenTelemetry processors. |
| `ConfigureTelemetryModule<T>()` | Removed; module functionality is built-in. |

## Removed interfaces and classes

| Type | Notes |
|---|---|
| `ITelemetryInitializer` | Removed. Convert to `BaseProcessor<Activity>` with `OnStart`. Register via `.AddProcessor<T>()` in the OpenTelemetry pipeline. |
| `ITelemetryProcessor` | Removed. Convert to `BaseProcessor<Activity>` with `OnEnd`. To drop telemetry, clear `data.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded`. Register via `.AddProcessor<T>()`. |
| `ApplicationInsightsLoggerProvider` | Logging is now automatic. No replacement needed. |
| `ExceptionTrackingMiddleware` | Exception tracking is built-in. |

## TelemetryClient changes

| Change | Details |
|---|---|
| `TrackEvent` | 3-param overload `(string, IDictionary<string,string>, IDictionary<string,double>)` **removed** — metrics dict dropped. Use 2-param overload and track metrics separately via `TrackMetric()`. |
| `TrackException` | 3-param overload with `IDictionary<string,double>` **removed**. Use 2-param overload and track metrics separately via `TrackMetric()`. |
| `TrackAvailability` | 8-param overload with trailing `IDictionary<string,double>` **removed**. Use 7-param overload and track metrics separately via `TrackMetric()`. |
| `TrackPageView` | **Removed entirely** (both overloads). Use `TrackEvent` or `TrackRequest` instead. |
| `GetMetric` | `MetricConfiguration` and `MetricAggregationScope` params **removed** from all overloads. Use simplified `GetMetric(metricId, ...)`. |
| parameterless `TelemetryClient()` | **Removed**. Use `TelemetryClient(TelemetryConfiguration)` via DI. |
| `client.InstrumentationKey` | **Removed**. Use `TelemetryConfiguration.ConnectionString`. |
| `TrackTrace`, `TrackMetric`, `TrackRequest`, `TrackDependency` (full overload), `Flush` | **Unchanged** — no action needed. |

## Middleware telemetry access

In 2.x, middleware could access `RequestTelemetry` via `HttpContext.Features`:

```csharp
// 2.x
var requestTelemetry = context.Features.Get<RequestTelemetry>();
requestTelemetry?.Properties.Add("ResponseBody", responseBody);
```

In 3.x, `RequestTelemetry` is no longer placed in `HttpContext.Features`. Use `Activity.Current` instead:

```csharp
// 3.x
using System.Diagnostics;

var activity = Activity.Current;
activity?.SetTag("ResponseBody", responseBody);
```

This applies to any code that accessed `RequestTelemetry` or `DependencyTelemetry` via `HttpContext.Features.Get<T>()`.

## Manually constructed telemetry objects

Telemetry types (`RequestTelemetry`, `DependencyTelemetry`, `TraceTelemetry`, `EventTelemetry`, `ExceptionTelemetry`, `MetricTelemetry`, `AvailabilityTelemetry`) still exist in 3.x and can be passed to `TelemetryClient.Track*(...)`. However:

- `ISupportProperties` — **Removed**. Use the typed `Properties` dictionary directly on each telemetry class.
- Type checks (`telemetry is RequestTelemetry`) in custom middleware or filters — these still compile but may not match auto-collected telemetry in contexts where the 3.x SDK uses `Activity` internally. Prefer `Activity.Current?.SetTag()` for enrichment.
- `new DependencyTelemetry(...)` with `StartOperation<DependencyTelemetry>()` — **Still works**. The dependency is correlated automatically.

## Migration steps

1. Update the package:
   ```xml
   <PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="3.*" />
   ```

2. Find and replace in your code:
   - `InstrumentationKey = "..."` → `ConnectionString = "InstrumentationKey=...;IngestionEndpoint=..."`
   - `EnableAdaptiveSampling = false` → `SamplingRatio = 1.0f` (or set `TracesPerSecond`)
   - Delete any lines setting `DeveloperMode`, `EndpointAddress`, `EnableHeartbeat`, `EnableDebugLogger`, `RequestCollectionOptions`, `DependencyCollectionOptions`, or `TelemetryInitializers`
   - Delete calls to `UseApplicationInsights()`, `AddApplicationInsightsTelemetryProcessor<T>()`, `ConfigureTelemetryModule<T>()`

3. Migrate custom telemetry types:
   - Convert each `ITelemetryInitializer` implementation to a `BaseProcessor<Activity>` with `OnStart`. Register via `.AddProcessor<T>()` in the OpenTelemetry pipeline.
   - Convert each `ITelemetryProcessor` implementation to a `BaseProcessor<Activity>` with `OnEnd`. To drop telemetry, clear the Recorded flag: `data.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded`. Register via `.AddProcessor<T>()`.
   - `TelemetryClient` mostly works in 3.x but has breaking changes: remove `IDictionary<string, double> metrics` parameter from `TrackEvent`/`TrackException`/`TrackAvailability` calls (track metrics separately via `TrackMetric()`). Replace `TrackPageView` with `TrackEvent` or `TrackRequest`. Remove `GetMetric` overloads that take `MetricConfiguration`/`MetricAggregationScope`.

4. Build and verify — the `Enable*` flags (`EnableQuickPulseMetricStream`, `EnableDependencyTrackingTelemetryModule`, etc.) still work with the same defaults. No changes needed for those.

## Behavior notes

- `TracesPerSecond` is the default sampling mode (effective default `5`). No configuration needed for most apps.
- Connection string resolution order: `ApplicationInsightsServiceOptions.ConnectionString` → `APPLICATIONINSIGHTS_CONNECTION_STRING` env var → `ApplicationInsights:ConnectionString` in config.

## See also

- No-code-change migration(see in appinsights-2x-to-3x-no-code-change.md)
- AddApplicationInsightsTelemetry API reference(see in AddApplicationInsightsTelemetry.md)
- TelemetryClient breaking changes(see in TelemetryClient.md)
