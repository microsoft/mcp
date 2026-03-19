---
title: LogProcessors
category: api-reference
applies-to: 1.x
---

# Log Processors — Enrichment & Redaction

## Key concepts

- Subclass `BaseProcessor<LogRecord>`. Only `OnEnd` is called (not `OnStart`).
- Register via `.AddProcessor<T>()` — see WithLogging.md.
- **Log filtering:** Use `ILoggingBuilder.AddFilter<OpenTelemetryLoggerProvider>()` — processor-level filtering is not reliable because `CompositeProcessor` iterates all processors regardless and `LogRecord` has no `Recorded` flag.

## Filtering — use `AddFilter`

```csharp
builder.Logging.AddFilter<OpenTelemetryLoggerProvider>("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter<OpenTelemetryLoggerProvider>("System.Net.Http", LogLevel.Warning);
```

This prevents log records from entering the OpenTelemetry pipeline entirely.

## Enrichment — add attributes

```csharp
using OpenTelemetry;
using OpenTelemetry.Logs;

public class LogEnrichmentProcessor : BaseProcessor<LogRecord>
{
    public override void OnEnd(LogRecord data)
    {
        var attributes = data.Attributes?.ToList()
            ?? [];

        attributes.Add(new("deployment.environment", "production"));
        attributes.Add(new("host.name", Environment.MachineName));

        data.Attributes = attributes;

        base.OnEnd(data);
    }
}
```

## Redaction — mask sensitive data

```csharp
using System.Linq;
using OpenTelemetry;
using OpenTelemetry.Logs;

public class RedactionProcessor : BaseProcessor<LogRecord>
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "token", "secret", "authorization", "api_key"
    };

    public override void OnEnd(LogRecord data)
    {
        if (data.Attributes != null)
        {
            data.Attributes = data.Attributes.Select(attr =>
                SensitiveKeys.Contains(attr.Key)
                    ? new KeyValuePair<string, object?>(attr.Key, "[REDACTED]")
                    : attr).ToList();
        }

        base.OnEnd(data);
    }
}
```

## Inspectable `LogRecord` properties

`CategoryName`, `LogLevel`, `EventId`, `FormattedMessage` (when `IncludeFormattedMessage = true`), `Attributes`, `TraceId`, `SpanId`, `Timestamp`.

## Migration from Application Insights 2.x

If the old `ITelemetryInitializer` or `ITelemetryProcessor` touched `TraceTelemetry` or `EventTelemetry`, those now map to `LogRecord`:

| 2.x Property | LogRecord equivalent |
| --- | --- |
| `trace.Properties["key"]` / `GlobalProperties["key"]` | Add to `data.Attributes` |
| `trace.SeverityLevel` | `data.LogLevel` |
| `trace.Message` | `data.FormattedMessage` or `data.Attributes` |
| `if (item is TraceTelemetry)` type check | Check `data.CategoryName` or `data.LogLevel` |
| Severity-based filtering in processor | `ILoggingBuilder.AddFilter<OpenTelemetryLoggerProvider>()` |

## Telemetry type mapping

| Source | Application Insights telemetry type |
| --- | --- |
| `ILogger` logs | `TraceTelemetry` |
| `ILogger` logs with `microsoft.custom_event.name` attribute | `EventTelemetry` |

See ActivityProcessors.md for `ActivityKind` → `RequestTelemetry`/`DependencyTelemetry` mapping.
