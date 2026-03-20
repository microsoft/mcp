---
title: OpenTelemetry Pipeline
category: concept
applies-to: 3.x
---

# OpenTelemetry Pipeline

**Category:** Concept  
**Applies to:** 3.x

## Overview

The OpenTelemetry pipeline is the data flow path for telemetry signals (traces, metrics, logs) from your application to observability backends. Understanding this pipeline is essential for Azure Monitor integration.

## Pipeline Components

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Instrumentation │ ──▶ │   Processors    │ ──▶ │    Exporters    │
│  (Sources)       │     │   (Transform)   │     │   (Backends)    │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

### 1. Instrumentation (Sources)
- **ActivitySource** - Creates spans/traces
- **Meter** - Creates metrics
- **ILogger** - Creates logs (via OpenTelemetry.Extensions.Logging)

### 2. Processors (Transform)
- **BaseProcessor<Activity>** - Enrich or filter spans
- **BaseProcessor<LogRecord>** - Enrich or filter logs
- Run in pipeline order before export

### 3. Exporters (Backends)
- **Azure Monitor Exporter** - Sends to Application Insights
- **OTLP Exporter** - Sends to any OTLP-compatible backend
- **Console Exporter** - Debug output

## Wiring Up the Pipeline

See the AddOpenTelemetry API reference(see in AddOpenTelemetry.md) for full setup examples showing how sources, processors, and exporters are composed via the builder API.

## Key Differences from Application Insights 2.x

| 2.x Concept | 3.x Equivalent |
|-------------|----------------|
| TelemetryClient | ActivitySource / Meter / ILogger |
| ITelemetryInitializer | BaseProcessor<Activity>.OnStart |
| ITelemetryProcessor | BaseProcessor<Activity>.OnEnd |
| TelemetryChannel | Exporter |
| TelemetryConfiguration | OpenTelemetryBuilder |

## See Also

- Application Insights for ASP.NET Core(see in appinsights-aspnetcore.md)
- AddOpenTelemetry API(see in AddOpenTelemetry.md)
