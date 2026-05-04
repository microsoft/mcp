---
title: TelemetryClient
category: api-reference
applies-to: 3.x
---

# TelemetryClient — Breaking Changes in 3.x

`TelemetryClient` still works in 3.x but several constructors, properties, and method overloads were **removed or changed**. Code that uses removed overloads will not compile after upgrading.

## Removed APIs

| API | Status | Migration |
| --- | --- | --- |
| `new TelemetryClient()` (parameterless) | **Removed** | Use `TelemetryClient(TelemetryConfiguration)` via DI (constructor injection). |
| `client.InstrumentationKey` | **Removed** | Use `TelemetryConfiguration.ConnectionString`. |
| `TrackPageView(string)` | **Removed** | Use `TrackEvent(name, properties)` or `TrackRequest`. |
| `TrackPageView(PageViewTelemetry)` | **Removed** | Use `TrackEvent(name, properties)` or `TrackRequest`. |

## Changed method signatures

The `IDictionary<string, double> metrics` parameter was **removed** from several Track methods. Code passing a metrics dictionary will not compile.

| Method | 2.x signature | 3.x signature | Fix |
| --- | --- | --- | --- |
| `TrackEvent` | `(string, IDictionary<string,string>, IDictionary<string,double>)` | `(string, IDictionary<string,string>)` | Remove metrics dict. Track metrics separately via `TrackMetric()`. |
| `TrackException` | `(Exception, IDictionary<string,string>, IDictionary<string,double>)` | `(Exception, IDictionary<string,string>)` | Remove metrics dict. Track metrics separately via `TrackMetric()`. |
| `TrackAvailability` | 8-param with trailing `IDictionary<string,double>` | 7-param — metrics removed | Remove metrics dict. Track metrics separately via `TrackMetric()`. |

### Example fix — TrackEvent

**2.x (breaks in 3.x):**
```csharp
_telemetryClient.TrackEvent("OrderCreated",
    new Dictionary<string, string> { ["OrderId"] = orderId },
    new Dictionary<string, double> { ["ProcessingTimeMs"] = elapsed });
```

**3.x:**
```csharp
_telemetryClient.TrackEvent("OrderCreated",
    new Dictionary<string, string> { ["OrderId"] = orderId });
_telemetryClient.TrackMetric("OrderProcessingTimeMs", elapsed);
```

## Other changed APIs

| API | Change | Migration |
| --- | --- | --- |
| `TrackDependency` (obsolete 5-param overload) | **Removed** | Use the full overload with `dependencyTypeName`, `target`, `data`, `startTime`, `duration`, `success`. |
| `GetMetric` (all overloads) | `MetricConfiguration` and `MetricAggregationScope` parameters **removed** | Call the simplified overload: `GetMetric(metricId)` or `GetMetric(metricId, dim1, ...)`. |
| `Track(ITelemetry)` | Internal routing changed — now delegates to specific Track methods | Review any direct `Track(ITelemetry)` calls; prefer specific Track methods. |
| `StartOperation<T>` / `StopOperation<T>` | Now uses OpenTelemetry Activities internally | No code change needed — API is the same. |

## Unchanged Track methods — no action needed

These methods and their signatures are **identical** in 3.x:

- `TrackEvent(string eventName)` — single-param overload
- `TrackEvent(string eventName, IDictionary<string, string> properties)` — 2-param overload (no metrics)
- `TrackException(Exception exception)` — single-param overload
- `TrackException(Exception exception, IDictionary<string, string> properties)` — 2-param overload (no metrics)
- `TrackTrace(string message)` and `TrackTrace(string, SeverityLevel)` and `TrackTrace(string, SeverityLevel, IDictionary<string,string>)`
- `TrackMetric(string name, double value)` and other `TrackMetric` overloads
- `TrackRequest(...)` — all overloads
- `TrackDependency(...)` — full overload (not the obsolete 5-param)
- `TrackAvailability(...)` — 7-param overload (without metrics dict)
- `Flush()` and `FlushAsync(CancellationToken)`

## TelemetryContext changes

Several sub-context classes are now **internal** in 3.x:

| Sub-context | Status | Previously accessible properties |
| --- | --- | --- |
| `Context.Cloud` | **Internal** | `RoleName`, `RoleInstance` — use `ConfigureResource(r => r.AddService("name"))` instead |
| `Context.Component` | **Internal** | `Version` — use `ApplicationVersion` in service options |
| `Context.Device` | **Internal** | `Type`, `Id`, `OperatingSystem`, etc. |
| `Context.Session` | **Internal** | `Id`, `IsFirst` |

These remain **public**: `Context.User` (`Id`, `AuthenticatedUserId`, `UserAgent`), `Context.Operation` (`Name`), `Context.Location` (`Ip`), `Context.GlobalProperties`.

Properties made internal on remaining public sub-contexts:
- `User.AccountId` — internal; set via properties dict on Track calls or custom processor
- `Operation.Id`, `Operation.ParentId` — internal; managed by OpenTelemetry correlation
- `Operation.CorrelationVector` — removed; no longer needed

## Extensibility — ConfigureOpenTelemetryBuilder

To register custom OpenTelemetry processors or instrumentations alongside TelemetryClient in 3.x:

```csharp
var config = TelemetryConfiguration.CreateDefault();
config.ConnectionString = "InstrumentationKey=...;IngestionEndpoint=...";
config.ConfigureOpenTelemetryBuilder(otel =>
{
    otel.WithTracing(t => t.AddProcessor<MyActivityProcessor>());
    otel.WithLogging(l => l.AddProcessor<MyLogProcessor>());
});
```

When using ASP.NET Core DI, use the service options + `ConfigureOpenTelemetryProvider` pattern instead — see ConfigureOpenTelemetryProvider.md.

## Quick decision guide

| Code pattern found | Action required? |
| --- | --- |
| `TrackEvent(name, props, metrics)` — 3 args | **Yes** — remove metrics dict, use `TrackMetric()` separately |
| `TrackException(ex, props, metrics)` — 3 args | **Yes** — remove metrics dict, use `TrackMetric()` separately |
| `TrackAvailability(..., metrics)` — 8 args | **Yes** — remove metrics dict, use `TrackMetric()` separately |
| `TrackPageView(...)` | **Yes** — replace with `TrackEvent()` or `TrackRequest()` |
| `GetMetric(..., MetricConfiguration, ...)` | **Yes** — remove config/scope params |
| `new TelemetryClient()` (no args) | **Yes** — use DI or `TelemetryClient(TelemetryConfiguration)` |
| `client.InstrumentationKey = ...` | **Yes** — use `TelemetryConfiguration.ConnectionString` |
| `Context.Cloud.RoleName = ...` | **Yes** — use `ConfigureResource` |
| `TrackEvent(name)` or `TrackEvent(name, props)` | No |
| `TrackTrace(...)` | No |
| `TrackMetric(...)` | No |
| `TrackRequest(...)` | No |
| `TrackDependency(...)` (full overload) | No |
| `TrackException(ex)` or `TrackException(ex, props)` | No |
| `Flush()` | No |
