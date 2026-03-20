---
title: WorkerService 2.x to 3.x Code Migration
category: migration
applies-to: 3.x
---

# Worker Service 2.x → 3.x Code Migration

## What changed

3.x uses OpenTelemetry under the hood. The main entry point is the same — `AddApplicationInsightsTelemetryWorkerService()` — but several options and an extension method were removed.

Key changes:
- `InstrumentationKey` → use `ConnectionString`
- `EnableAdaptiveSampling` → use `TracesPerSecond` (default `5`) or `SamplingRatio`
- Logging is automatic — no additional logger provider needed
- New: `Credential` for AAD authentication, `EnableTraceBasedLogsSampler` for log sampling control

## Before / after

**2.x**
```csharp
using Microsoft.Extensions.DependencyInjection;

builder.ConfigureServices(services =>
{
    services.AddApplicationInsightsTelemetryWorkerService(options =>
    {
        options.InstrumentationKey = "your-ikey";          // Removed in 3.x
        options.EnableAdaptiveSampling = false;             // Removed in 3.x
        options.DeveloperMode = true;                       // Removed in 3.x
    });
});
```

**3.x**
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.WorkerService;

builder.ConfigureServices(services =>
{
    services.AddApplicationInsightsTelemetryWorkerService(options =>
    {
        options.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
        options.SamplingRatio = 1.0f;       // No sampling (collect everything)
        // DeveloperMode — no replacement needed, remove the line
    });
});
```

Or set `APPLICATIONINSIGHTS_CONNECTION_STRING` as an environment variable and call `AddApplicationInsightsTelemetryWorkerService()` with no arguments.

## Property changes

| Property | Status | Action required |
|---|---|---|
| `ConnectionString` | Unchanged | None. |
| `ApplicationVersion` | Unchanged | None. |
| `EnableQuickPulseMetricStream` | Unchanged | None. Default `true`. |
| `EnablePerformanceCounterCollectionModule` | Unchanged | None. Default `true`. |
| `EnableDependencyTrackingTelemetryModule` | Unchanged | None. Default `true`. |
| `AddAutoCollectedMetricExtractor` | Unchanged | None. Default `true`. |
| `InstrumentationKey` | **Removed** | Use `ConnectionString`. |
| `EnableAdaptiveSampling` | **Removed** | Use `TracesPerSecond` or `SamplingRatio`. |
| `DeveloperMode` | **Removed** | Delete the line. |
| `EndpointAddress` | **Removed** | Endpoint is now part of `ConnectionString`. |
| `EnableHeartbeat` | **Removed** | Delete the line. |
| `EnableDebugLogger` | **Removed** | Delete the line. |
| `DependencyCollectionOptions` | **Removed** | Delete the line. |
| `EnableEventCounterCollectionModule` | **Removed** | Delete the line. |
| `EnableAppServicesHeartbeatTelemetryModule` | **Removed** | Delete the line; heartbeat is automatic. |
| `EnableAzureInstanceMetadataTelemetryModule` | **Removed** | Delete the line; resource detection is automatic. |
| `EnableDiagnosticsTelemetryModule` | **Removed** | Delete the line. |
| `Credential` | **New** | `TokenCredential`, default `null`. Set for AAD auth. |
| `TracesPerSecond` | **New** | `double?`, effective default `5`. Rate-limited sampling. |
| `SamplingRatio` | **New** | `float?`, default `null`. Fixed-rate sampling (0.0–1.0). |
| `EnableTraceBasedLogsSampler` | **New** | `bool?`, effective default `true`. Logs follow parent trace sampling. |

## Removed extension methods

| Method | Replacement |
|---|---|
| `AddApplicationInsightsTelemetryWorkerService(string instrumentationKey)` | Use parameterless overload + `ConnectionString` in options or env var. |

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

## Migration steps

1. Update the package:
   ```xml
   <PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="3.*" />
   ```

2. Find and replace in your code:
   - `InstrumentationKey = "..."` → `ConnectionString = "InstrumentationKey=...;IngestionEndpoint=..."`
   - `EnableAdaptiveSampling = false` → `SamplingRatio = 1.0f` (or set `TracesPerSecond`)
   - Delete any lines setting `DeveloperMode`, `EndpointAddress`, `EnableHeartbeat`, `EnableDebugLogger`, `DependencyCollectionOptions`, `EnableEventCounterCollectionModule`, `EnableAppServicesHeartbeatTelemetryModule`, `EnableAzureInstanceMetadataTelemetryModule`, or `EnableDiagnosticsTelemetryModule`
   - Replace `AddApplicationInsightsTelemetryWorkerService("your-ikey")` with `AddApplicationInsightsTelemetryWorkerService()` and set `ConnectionString` via options or env var

3. Migrate TelemetryClient breaking calls:
   - Remove `IDictionary<string, double> metrics` parameter from `TrackEvent`/`TrackException`/`TrackAvailability` calls (track metrics separately via `TrackMetric()`). Replace `TrackPageView` with `TrackEvent` or `TrackRequest`. Remove `GetMetric` overloads that take `MetricConfiguration`/`MetricAggregationScope`.

4. Build and verify — the `Enable*` flags (`EnableQuickPulseMetricStream`, `EnableDependencyTrackingTelemetryModule`, etc.) still work with the same defaults. No changes needed for those.

## Behavior notes

- `TracesPerSecond` is the default sampling mode (effective default `5`). No configuration needed for most apps.
- Connection string resolution order: `ApplicationInsightsServiceOptions.ConnectionString` → `APPLICATIONINSIGHTS_CONNECTION_STRING` env var → `ApplicationInsights:ConnectionString` in config.

## See also

- No-code-change migration(see in workerservice-2x-to-3x-no-code-change.md)
- AddApplicationInsightsTelemetryWorkerService API reference(see in AddApplicationInsightsTelemetryWorkerService.md)
- TelemetryClient breaking changes(see in TelemetryClient.md)
