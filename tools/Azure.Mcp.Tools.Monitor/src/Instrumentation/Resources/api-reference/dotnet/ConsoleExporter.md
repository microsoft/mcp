---
title: ConsoleExporter
category: api-reference
applies-to: 1.x
---

# Console Exporter

Writes traces, metrics, and logs to the console (stdout). Useful for **local development and debugging only** — not for production.

## Package

```
OpenTelemetry.Exporter.Console
```

## Setup

```csharp
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;

builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
    tracing.AddConsoleExporter());

builder.Services.ConfigureOpenTelemetryMeterProvider(metrics =>
    metrics.AddConsoleExporter());

builder.Services.ConfigureOpenTelemetryLoggerProvider(logging =>
    logging.AddConsoleExporter());
```

## Options

| Option | Default | Description |
|---|---|---|
| `Targets` | `Console` | `ConsoleExporterOutputTargets.Console` or `Debug` (writes to `System.Diagnostics.Debug`). |

## Notes

- **Development only** — do not use in production. Adds significant overhead.
- Useful for verifying that spans, metrics, and log records are being generated correctly.
- Console exporter works alongside Azure Monitor — both receive the same telemetry.
- Each signal needs its own `AddConsoleExporter()` call.
- For production multi-destination export, use OTLP exporter instead.
- **Non-DI usage:** Use `config.ConfigureOpenTelemetryBuilder(otel => otel.WithTracing(t => t.AddConsoleExporter()))` on `TelemetryConfiguration`. See TelemetryClient.md.
