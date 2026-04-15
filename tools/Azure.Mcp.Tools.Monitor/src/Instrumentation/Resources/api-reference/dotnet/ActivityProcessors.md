---
title: ActivityProcessors
category: api-reference
applies-to: 1.x
---

# Activity Processors — Filtering & Enrichment

## Key concepts

- Subclass `BaseProcessor<Activity>` to create a custom processor.
- **`OnStart`** = enrichment (add tags early). **`OnEnd`** = filtering (all data finalized).
- Register via `.AddProcessor<T>()` — see WithTracing.md.

## Filtering — drop spans

Clear the `Recorded` flag to prevent export. Export processors (`BatchActivityExportProcessor`, `SimpleActivityExportProcessor`) check `Activity.Recorded` and skip when `false`.

```csharp
using System.Diagnostics;
using OpenTelemetry;

public class HealthCheckFilterProcessor : BaseProcessor<Activity>
{
    private static readonly HashSet<string> SuppressedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health", "/ready", "/healthz", "/liveness"
    };

    public override void OnEnd(Activity data)
    {
        var path = data.GetTagItem("url.path")?.ToString()
                ?? data.GetTagItem("http.route")?.ToString();

        if (path != null && SuppressedPaths.Contains(path))
        {
            data.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }
    }
}
```

> **Important:** Do not rely on skipping `base.OnEnd()` — `CompositeProcessor` iterates all processors regardless. The `Recorded` flag is the correct filtering mechanism.

## Enrichment — add tags

Use `OnStart` to attach tags before the span completes:

```csharp
using System.Diagnostics;
using OpenTelemetry;

public class EnrichmentProcessor : BaseProcessor<Activity>
{
    public override void OnStart(Activity data)
    {
        data.SetTag("deployment.environment", "production");
        data.SetTag("cloud.region", "eastus");
    }
}
```

## Inspectable `Activity` properties

`DisplayName`, `Source.Name`, `Kind`, `Status`, `Duration`, `Tags` (via `GetTagItem()`), `Events`, `Links`, `ParentId`.

## Migration from Application Insights 2.x

> **Key difference:** A single `ITelemetryInitializer` or `ITelemetryProcessor` handled all signal types (requests, dependencies, traces, events). In OpenTelemetry, traces, logs, and metrics are separate pipelines — each processor only sees its own signal. If the old initializer/processor touched both signals (e.g., checked for `RequestTelemetry` and `TraceTelemetry`), you need two OpenTelemetry processors: `BaseProcessor<Activity>` for traces + `BaseProcessor<LogRecord>` for logs. If it only touched trace types (`RequestTelemetry`, `DependencyTelemetry`), a single `BaseProcessor<Activity>` is sufficient. Any `if (telemetry is MetricTelemetry) return;` guards can be removed — metrics have their own pipeline and never reach activity or log processors.

| 2.x Pattern | 3.x Equivalent |
| --- | --- |
| `ITelemetryInitializer.Initialize(ITelemetry)` | `BaseProcessor<Activity>.OnStart(Activity)` — enrich traces |
| `ITelemetryInitializer` touching `TraceTelemetry`/`EventTelemetry` | `BaseProcessor<LogRecord>.OnEnd(LogRecord)` — enrich logs (see LogProcessors.md) |
| `ITelemetryProcessor.Process(ITelemetry)` | `BaseProcessor<Activity>.OnEnd(Activity)` — filter traces |
| `ITelemetryProcessor` filtering `TraceTelemetry` by severity | `ILoggingBuilder.AddFilter<OpenTelemetryLoggerProvider>()` — filter logs (see LogProcessors.md) |
| `services.AddApplicationInsightsTelemetryProcessor<T>()` | `.AddProcessor<T>()` on `TracerProviderBuilder` |
| Not calling `next.Process(item)` to drop | `data.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;` |

### Property mapping

| 2.x Property | Activity equivalent | LogRecord equivalent |
| --- | --- | --- |
| `telemetry.Context.GlobalProperties["key"]` | `data.SetTag("key", value)` | Add to `data.Attributes` |
| `telemetry.Properties["key"]` | `data.SetTag("key", value)` | Add to `data.Attributes` |
| `request.Url.AbsolutePath` | `data.GetTagItem("url.path")` | N/A |
| `request.ResponseCode` / `dependency.ResultCode` | `data.GetTagItem("http.response.status_code")` | N/A |
| `request.Success = false` | `data.SetStatus(ActivityStatusCode.Error)` | N/A |
| `request.Name` / `dependency.Name` | `data.DisplayName` (get/set) | N/A |
| `dependency.Type` | `data.GetTagItem("db.system")` or `data.GetTagItem("rpc.system")` | N/A |
| `dependency.Target` | `data.GetTagItem("server.address")` | N/A |
| `dependency.Duration` | `data.Duration` | N/A |
| `trace.SeverityLevel` | N/A | `data.LogLevel` |
| `trace.Message` | N/A | `data.FormattedMessage` or `data.Attributes` |
| `telemetry.Context.User.AuthenticatedUserId` | `data.SetTag("enduser.id", value)` | N/A |
| `telemetry.Context.Operation.Id` | `data.TraceId` (read-only, set by framework) | `data.TraceId` |
| `telemetry.Context.Cloud.RoleName` | Not a processor concern — use `ConfigureResource(r => r.AddService("name"))` | Same |

### Telemetry type mapping

Old processors often check `if (telemetry is RequestTelemetry)` or `if (telemetry is DependencyTelemetry)`. In 3.x, check `Activity.Kind`:

| `ActivityKind` | Application Insights telemetry type |
| --- | --- |
| `Server` | `RequestTelemetry` |
| `Consumer` | `RequestTelemetry` (message-driven) or `DependencyTelemetry` (when acting as downstream call) |
| `Client` | `DependencyTelemetry` |
| `Producer` | `DependencyTelemetry` |
| `Internal` | `DependencyTelemetry` (if parent is `Server`/`Consumer`) |

Other signal types:

| Source | Application Insights telemetry type |
| --- | --- |
| `ILogger` logs | `TraceTelemetry` |
| `ILogger` logs with `microsoft.custom_event.name` attribute | `EventTelemetry` |
| Exceptions logged via `ILogger.LogError` / `LogCritical` | `ExceptionTelemetry` — now a `LogRecord` with exception attributes |
| Exceptions on spans | `ExceptionTelemetry` — now an `Activity.Event` named `exception` (check `data.Events`) |
| `Meter` measurements | `MetricTelemetry` |
