# Basic Azure Monitor Setup for Python

This guide shows how to add Azure Monitor OpenTelemetry to a Python application.

## Prerequisites

- Python 3.8 or higher
- pip
- Azure Application Insights resource

## Step 1: Install Package

```bash
pip install azure-monitor-opentelemetry
```

Or add to your `requirements.txt`:
```
azure-monitor-opentelemetry
```

## Step 2: Initialize at Startup

Add the following to your application entry point:

```python
from azure.monitor.opentelemetry import configure_azure_monitor

# Configure Azure Monitor - MUST be called before importing other libraries
configure_azure_monitor()

# Now import your application code
# ...
```

## Step 3: Configure Connection String

### Option A: Environment Variable (Recommended)

```bash
export APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://..."
```

### Option B: .env File

Create a `.env` file:
```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
```

Load it with python-dotenv:
```bash
pip install python-dotenv
```

```python
from dotenv import load_dotenv
load_dotenv()

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()
```

### Option C: Direct Configuration

```python
from azure.monitor.opentelemetry import configure_azure_monitor

configure_azure_monitor(
    connection_string="InstrumentationKey=..."
)
```

## Step 4: Optional Configuration

### Service Name
```bash
export OTEL_SERVICE_NAME="my-python-app"
```

### Resource Attributes
```bash
export OTEL_RESOURCE_ATTRIBUTES="deployment.environment=production,service.version=1.0.0"
```

### Sampling
```bash
export OTEL_TRACES_SAMPLER=traceidratio
export OTEL_TRACES_SAMPLER_ARG=0.1  # 10% sampling
```

## Step 5: Add Custom Telemetry (Optional)

```python
from opentelemetry import trace

tracer = trace.get_tracer(__name__)

def process_order(order_id: str):
    with tracer.start_as_current_span("process-order") as span:
        span.set_attribute("order.id", order_id)
        
        try:
            # Your business logic
            result = do_processing(order_id)
            span.set_attribute("order.status", "completed")
            return result
        except Exception as e:
            span.record_exception(e)
            span.set_attribute("order.status", "failed")
            raise
```

## Verification

After setup, you should see telemetry in Azure Portal:
1. Navigate to your Application Insights resource
2. Go to "Transaction search" or "Live Metrics"
3. Make some requests to your application
4. Verify data appears within a few minutes

## Complete Example

```python
# app.py
import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Configure Azure Monitor FIRST
from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

# Now import and run your application
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def main():
    logger.info("Application started")
    # Your application logic here
    logger.info("Application finished")

if __name__ == "__main__":
    main()
```

## Troubleshooting

### No Data in Azure Portal
1. Verify connection string is correct
2. Check for firewall/proxy blocking outbound HTTPS
3. Enable debug logging: `export OTEL_LOG_LEVEL=debug`

### Import Order Issues
Ensure `configure_azure_monitor()` is called before importing instrumented libraries.

## Links

- [Azure Monitor OpenTelemetry](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable?tabs=python)
- [Troubleshooting Guide](https://learn.microsoft.com/troubleshoot/azure/azure-monitor/app-insights/telemetry/opentelemetry-troubleshooting-python)
