---
title: OtlpExporter
category: api-reference
applies-to: 1.x
---

# OTLP Exporter

Export traces, metrics, and logs to any OpenTelemetry Protocol (OTLP) compatible backend (e.g. Jaeger, Grafana Tempo, Aspire Dashboard, custom collectors).

## Package

```
OpenTelemetry.Exporter.OpenTelemetryProtocol
```

## Setup

```csharp
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;

// Azure Monitor is already configured via AddApplicationInsightsTelemetry.
// Add OTLP as a secondary exporter:
builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
    tracing.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri("http://localhost:4317");
        options.Protocol = OtlpExportProtocol.Grpc;
    }));

builder.Services.ConfigureOpenTelemetryMeterProvider(metrics =>
    metrics.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri("http://localhost:4317");
    }));

builder.Services.ConfigureOpenTelemetryLoggerProvider(logging =>
    logging.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri("http://localhost:4317");
    }));
```
    .WithTracing(tracing => tracing.AddOtlpExporter())
    .WithMetrics(metrics => metrics.AddOtlpExporter());
```

### Environment variable configuration

Instead of code, configure via environment variables:

| Variable | Default | Description |
|---|---|---|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | `http://localhost:4317` | OTLP endpoint URL |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | `grpc` | Protocol: `grpc` or `http/protobuf` |
| `OTEL_EXPORTER_OTLP_HEADERS` | — | Headers as `key=value` pairs, comma-separated |
| `OTEL_EXPORTER_OTLP_TIMEOUT` | `10000` | Timeout in milliseconds |

## Options

| Option | Default | Description |
|---|---|---|
| `Endpoint` | `http://localhost:4317` | Backend URL. Use port `4317` for gRPC, `4318` for HTTP. |
| `Protocol` | `Grpc` | `OtlpExportProtocol.Grpc` or `OtlpExportProtocol.HttpProtobuf` |
| `Headers` | `null` | Custom headers (e.g. auth tokens). Format: `"key=value"` |
| `TimeoutMilliseconds` | `10000` | Export timeout. |
| `ExportProcessorType` | `Batch` | `Batch` or `Simple`. Use `Simple` for debugging only. |
| `BatchExportProcessorOptions` | default | Batch size, delay, queue size for batch processor. |

## Common backends

| Backend | Endpoint | Protocol |
|---|---|---|
| Aspire Dashboard | `http://localhost:4317` | gRPC |
| Jaeger | `http://localhost:4317` | gRPC |
| Grafana Tempo | `http://localhost:4317` | gRPC |
| Grafana Cloud | `https://otlp-gateway-*.grafana.net/otlp` | HTTP (`http/protobuf`) |
| Seq | `http://localhost:5341/ingest/otlp/v1/traces` | HTTP |

## Notes

- OTLP exporter works **alongside** Azure Monitor — data is sent to both destinations.
- For local development with Aspire Dashboard, use `http://localhost:4317` with gRPC.
- `AddOtlpExporter()` with no arguments uses environment variables or defaults.
- Each signal (traces, metrics, logs) needs its own `AddOtlpExporter()` call.
- **Non-DI usage:** Use `config.ConfigureOpenTelemetryBuilder(otel => otel.WithTracing(t => t.AddOtlpExporter(...)))` on `TelemetryConfiguration`. See TelemetryClient.md.
