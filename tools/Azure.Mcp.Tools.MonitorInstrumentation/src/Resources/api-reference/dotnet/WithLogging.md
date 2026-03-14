---
title: WithLogging
category: api-reference
applies-to: 1.x
---

# .WithLogging()

## API surface

| Method | Purpose |
| --- | --- |
| `.WithLogging()` | Enable logging with defaults |
| `.WithLogging(Action<LoggerProviderBuilder>)` | Enable logging and configure the provider builder |
| `.WithLogging(Action<LoggerProviderBuilder>?, Action<OpenTelemetryLoggerOptions>?)` | Configure both provider and logger options |
| `ILoggingBuilder.AddOpenTelemetry()` | Register OpenTelemetry logger without the host builder |
| `ILoggingBuilder.AddOpenTelemetry(Action<OpenTelemetryLoggerOptions>?)` | Register with options configuration |
| `builder.AddProcessor(BaseProcessor<LogRecord>)` | Add a log processor instance |
| `builder.AddProcessor<T>()` | Add a processor resolved from DI (singleton) |
| `builder.AddProcessor(Func<IServiceProvider, BaseProcessor<LogRecord>>)` | Add a processor via factory |

See also: ConfigureResource.md for resource configuration

## When to use

- Route `ILogger` log records through the OpenTelemetry pipeline in an ASP.NET Core or Worker Service app.
- Configure `OpenTelemetryLoggerOptions` (formatted messages, scopes, state parsing) alongside the provider.
- Use `AddOpenTelemetry()` on `ILoggingBuilder` when you need logging without the full `AddOpenTelemetry()` builder (e.g., non-host `ServiceCollection`).
- Call multiple times safely — only one `LoggerProvider` is created per `IServiceCollection`.

## Minimal example

```csharp
using OpenTelemetry;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithLogging(logging => logging
        .AddOtlpExporter());

var app = builder.Build();
app.Run();
```

## Full example

```csharp
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("order-api"))
    .WithLogging(
        configureBuilder: logging => logging
            .AddProcessor<MyLogEnrichmentProcessor>()
            .AddOtlpExporter(),
        configureOptions: options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
        });

var app = builder.Build();
app.Run();
```

### Alternative: ILoggingBuilder.AddOpenTelemetry()

Use this when you don't have an `IOpenTelemetryBuilder` — for example, a bare `ServiceCollection`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

var services = new ServiceCollection();

services.AddLogging(logging => logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
    }));
```

## Key parameters

`OpenTelemetryLoggerOptions` properties:

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `IncludeFormattedMessage` | `bool` | `false` | Include the formatted log message on `LogRecord` |
| `IncludeScopes` | `bool` | `false` | Include log scopes on `LogRecord` |
| `ParseStateValues` | `bool` | `false` | Parse state into `LogRecord.Attributes` (sets `State` to `null`) |

## Behavior notes

- `WithLogging()` automatically registers an `ILoggerProvider` named `"OpenTelemetry"`.
- `AddOpenTelemetry()` on `ILoggingBuilder` does **not** support all `IServiceCollection` features available to tracing/metrics (e.g., `ConfigureServices` extensions).
- When `IncludeFormattedMessage` is `false`, the formatted message is still included if no message template is found.
- When `ParseStateValues` is `true`, `LogRecord.State` is always `null` — use `LogRecord.Attributes` instead.
- State parsing happens automatically (regardless of `ParseStateValues`) if the logged state implements `IReadOnlyList<KeyValuePair<string, object>>`.
- Processors run in registration order. Export processors should be added last.
- `OpenTelemetryLoggerOptions` reloading is disabled internally to prevent unwanted re-creation of processors/exporters during `IConfiguration` reloads.
