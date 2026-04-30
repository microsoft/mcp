---
title: ILogger and Log Filtering Migration (2.x to 3.x)
category: migration
applies-to: 3.x
---

# ILogger and Log Filtering Migration (2.x → 3.x)

## What changed

In 2.x, `AddApplicationInsightsTelemetry()` already captured ILogger output automatically. However, some codebases also called `AddApplicationInsights()` on `ILoggingBuilder` explicitly — for example, to configure `ApplicationInsightsLoggerOptions` or to add category-level log filters targeting `ApplicationInsightsLoggerProvider`.

In 3.x, ILogger capture remains automatic. The difference is that the underlying provider is now `OpenTelemetryLoggerProvider` (from the OpenTelemetry SDK). Any explicit `AddApplicationInsights()` calls and filters targeting `ApplicationInsightsLoggerProvider` must be updated.

| Aspect | 2.x | 3.x |
|---|---|---|
| Logger provider | `ApplicationInsightsLoggerProvider` (explicit) | `OpenTelemetryLoggerProvider` (automatic) |
| Registration | `loggingBuilder.AddApplicationInsights()` | Not needed — automatic |
| Category filters | `AddFilter<ApplicationInsightsLoggerProvider>(...)` | `AddFilter<OpenTelemetryLoggerProvider>(...)` or plain `AddFilter(...)` |
| Advanced filtering | `ITelemetryProcessor` | `BaseProcessor<LogRecord>` via `ConfigureOpenTelemetryLoggerProvider` |

## Before / after

**2.x**
```csharp
using Microsoft.Extensions.Logging.ApplicationInsights;

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddApplicationInsights(options =>
    {
        options.TrackExceptionsAsExceptionTelemetry = true;
        options.IncludeScopes = true;
    });
    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft.AspNetCore", LogLevel.Warning);
    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("MyApp", LogLevel.Information);
});
```

**3.x**
```csharp
using OpenTelemetry.Logs; // Required for OpenTelemetryLoggerProvider

// Logging is automatic — just configure filters targeting the OTel provider
builder.Logging.AddFilter<OpenTelemetryLoggerProvider>("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter<OpenTelemetryLoggerProvider>("MyApp", LogLevel.Information);
```

## Migration steps

1. **Remove `AddApplicationInsights()`** — delete the call and its options lambda (`TrackExceptionsAsExceptionTelemetry`, `IncludeScopes` no longer exist).
2. **Replace log filters** — change `AddFilter<ApplicationInsightsLoggerProvider>` to `AddFilter<OpenTelemetryLoggerProvider>`, or use a plain `AddFilter(category, level)`. Add `using OpenTelemetry.Logs;` if targeting the provider specifically.
3. **Update usings** — remove `using Microsoft.Extensions.Logging.ApplicationInsights;`.
4. **Remove the package** — `Microsoft.Extensions.Logging.ApplicationInsights` is no longer needed.
