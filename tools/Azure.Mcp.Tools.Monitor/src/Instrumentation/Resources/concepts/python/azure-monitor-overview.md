---
title: Azure Monitor OpenTelemetry for Python
category: concept
applies-to: python
---

# Azure Monitor for Python

**Category:** Concept  
**Applies to:** Python

Azure Monitor OpenTelemetry Distro for Python provides automatic instrumentation and telemetry collection for Python applications.

## Key Features

- **Automatic Instrumentation**: Captures HTTP requests, database calls, and external dependencies
- **Custom Telemetry**: Track custom events, metrics, and traces
- **Performance Monitoring**: Monitor response times, throughput, and failures
- **Dependency Tracking**: Understand outgoing calls to databases, APIs, and services
- **Distributed Tracing**: Follow requests across microservices

## Supported Frameworks

The Azure Monitor Distro automatically instruments:
- **Django** - Full-featured web framework
- **Flask** - Lightweight WSGI framework
- **FastAPI** - Modern async API framework
- **Requests** - HTTP library
- **urllib/urllib3** - Standard HTTP libraries
- **Psycopg2** - PostgreSQL adapter

Additional instrumentations available via OpenTelemetry contrib packages.

## Installation

```bash
pip install azure-monitor-opentelemetry
```

## Basic Setup

```python
from azure.monitor.opentelemetry import configure_azure_monitor

# Configure Azure Monitor - must be called before importing frameworks
configure_azure_monitor()

# Now import and use your framework
from flask import Flask
app = Flask(__name__)
```

## Configuration Options

The `configure_azure_monitor()` function uses environment variables:

| Variable | Description |
|----------|-------------|
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Required. Your App Insights connection string |
| `OTEL_SERVICE_NAME` | Optional. Your application name |
| `OTEL_RESOURCE_ATTRIBUTES` | Optional. Custom resource attributes |
| `OTEL_TRACES_SAMPLER` | Optional. Sampling strategy |
| `OTEL_TRACES_SAMPLER_ARG` | Optional. Sampling rate (0.0-1.0) |

## What Gets Instrumented

### Automatically Captured (Distro Bundled)
- HTTP requests via Django, Flask, FastAPI
- HTTP client calls via requests, urllib, urllib3
- PostgreSQL queries via psycopg2
- Azure SDK calls via azure-core

### Manual Instrumentation Required
For other libraries (Redis, MongoDB, Celery, etc.), install the OpenTelemetry instrumentation package:

```bash
pip install opentelemetry-instrumentation-redis
```

### Custom Telemetry
```python
from opentelemetry import trace

# Get current tracer
tracer = trace.get_tracer(__name__)

# Create custom span
with tracer.start_as_current_span("custom-operation") as span:
    span.set_attribute("custom.attribute", "value")
    # Your business logic here
```

## Best Practices

1. **Initialize Early**: Call `configure_azure_monitor()` before importing frameworks
2. **Use Environment Variables**: Store connection string in `.env` file
3. **Enable Sampling**: For high-traffic apps, configure sampling to manage costs
4. **Add Context**: Use custom attributes to enrich telemetry
5. **Monitor Performance**: Set up alerts in Azure Monitor for key metrics

## Connection String

Get your connection string from Azure Portal:
1. Navigate to your Application Insights resource
2. Go to "Overview" section
3. Copy the "Connection String" value

Set it as an environment variable:
```bash
export APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=..."
```

Or in `.env` file:
```
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=...
```

## Links

- [Azure Monitor OpenTelemetry Distro](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable?tabs=python)
- [OpenTelemetry Python](https://opentelemetry.io/docs/instrumentation/python/)
- [Azure SDK for Python](https://github.com/Azure/azure-sdk-for-python)
