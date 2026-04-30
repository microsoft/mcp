---
title: OpenTelemetry Pipeline for Python
category: concept
applies-to: python
---

# OpenTelemetry Pipeline for Python

**Category:** Concept  
**Applies to:** Python

This document explains how OpenTelemetry works in Python applications and how Azure Monitor integrates with it.

## OpenTelemetry Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Your Python Application                  │
├─────────────────────────────────────────────────────────────┤
│  Instrumentation Layer                                      │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐            │
│  │   Django    │ │    Flask    │ │   FastAPI   │            │
│  │ Instrumentor│ │ Instrumentor│ │ Instrumentor│            │
│  └─────────────┘ └─────────────┘ └─────────────┘            │
├─────────────────────────────────────────────────────────────┤
│  OpenTelemetry SDK                                          │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐            │
│  │   Traces    │ │   Metrics   │ │    Logs     │            │
│  │  Provider   │ │  Provider   │ │  Provider   │            │
│  └─────────────┘ └─────────────┘ └─────────────┘            │
├─────────────────────────────────────────────────────────────┤
│  Exporters                                                  │
│  ┌ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┐  │
│  │ Azure Monitor OpenTelemetry Distro (Thin Wrapper)     │  │
│  │ ┌───────────────────────────────────────────────────┐ │  │
│  │ │     Azure Monitor OpenTelemetry Exporter          │ │  │
│  │ └───────────────────────────────────────────────────┘ │  │
│  │ • Sets up TracerProvider, LoggerProvider, etc.        │  │
│  │ • Attaches Flask/Django/FastAPI/Requests instrumentors│  │
│  │ • Reads APPLICATIONINSIGHTS_CONNECTION_STRING env var │  │
│  │ • Detects Azure App Service/Functions/AKS metadata    │  │
│  └ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │  Azure Monitor  │
                    │  (App Insights) │
                    └─────────────────┘
```

## Core Components

### 1. Instrumentors
Instrumentors automatically capture telemetry from libraries:
- `FlaskInstrumentor` - Captures Flask HTTP requests
- `DjangoInstrumentor` - Captures Django requests and middleware
- `FastAPIInstrumentor` - Captures FastAPI/Starlette requests
- `RequestsInstrumentor` - Captures outbound HTTP calls

### 2. Providers
Providers manage the lifecycle of telemetry:
- **TracerProvider**: Manages trace/span creation
- **MeterProvider**: Manages metrics collection
- **LoggerProvider**: Manages log correlation

### 3. Exporters
Exporters send telemetry to backends:
- **Azure Monitor Exporter**: Sends to Application Insights
- **Console Exporter**: Prints to stdout (for debugging)
- **OTLP Exporter**: Sends to any OTLP-compatible backend

## How Azure Monitor Distro Works

The `azure-monitor-opentelemetry` package simplifies setup by:

1. **Auto-configuring providers** - Sets up trace, metric, and log providers
2. **Auto-instrumenting** - Enables bundled instrumentations automatically
3. **Configuring export** - Sets up the Azure Monitor exporter
4. **Resource detection** - Detects Azure resource metadata

### One-Line Setup
```python
from azure.monitor.opentelemetry import configure_azure_monitor

configure_azure_monitor()  # That's it!
```

This replaces what would otherwise be 50+ lines of manual configuration.

## Signal Types

### Traces
Distributed traces track requests across services:
```python
from opentelemetry import trace

tracer = trace.get_tracer(__name__)

with tracer.start_as_current_span("my-operation") as span:
    span.set_attribute("user.id", user_id)
    # ... operation code
```

### Metrics
Metrics capture aggregated measurements:
```python
from opentelemetry import metrics

meter = metrics.get_meter(__name__)
request_counter = meter.create_counter("requests")

request_counter.add(1, {"endpoint": "/api/users"})
```

### Logs
Logs are correlated with traces:
```python
import logging

logger = logging.getLogger(__name__)
logger.info("Processing request")  # Automatically correlated with active span
```

## Context Propagation

OpenTelemetry automatically propagates trace context:
- Across HTTP calls (via W3C Trace Context headers)
- Between services (distributed tracing)
- To logs (correlation IDs)

## Sampling

Control telemetry volume with sampling:

```bash
# Sample 10% of traces
export OTEL_TRACES_SAMPLER=traceidratio
export OTEL_TRACES_SAMPLER_ARG=0.1
```

## Best Practices

1. **Initialize First**: Configure OpenTelemetry before importing instrumented libraries
2. **Use Semantic Conventions**: Follow OpenTelemetry naming standards for attributes
3. **Enrich Spans**: Add business context via custom attributes
4. **Handle Errors**: Record exceptions on spans for better debugging
5. **Configure Sampling**: Balance observability needs with costs

## Links

- [OpenTelemetry Python Documentation](https://opentelemetry.io/docs/instrumentation/python/)
- [OpenTelemetry Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/)
- [Azure Monitor OpenTelemetry](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-overview)
