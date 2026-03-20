# Basic Azure Monitor Setup for Python Console/Script Applications

This guide shows how to add Azure Monitor OpenTelemetry to standalone Python scripts, console applications, or background workers.

## Prerequisites

- Python 3.8 or higher
- Python script or console application
- Azure Application Insights resource

## Step 1: Install Packages

```bash
pip install azure-monitor-opentelemetry>=1.8.3
```

Or add to your `requirements.txt`:
```
azure-monitor-opentelemetry>=1.8.3
```

## Step 2: Initialize at Startup

Update your main script file (e.g., `app.py`, `main.py`):

```python
# IMPORTANT: Configure Azure Monitor at the very top
from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

# Now import your other libraries
import time
import logging

# Your application code
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def main():
    logger.info("Application started")
    # Your logic here
    time.sleep(1)
    logger.info("Application completed")

if __name__ == "__main__":
    main()
```

## Step 3: Configure Connection String

Create a `.env` file:
```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
```

Load environment variables:
```python
from dotenv import load_dotenv
load_dotenv()

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

# ... rest of your app
```

## What Gets Auto-Instrumented

The Azure Monitor Distro automatically captures:
- ✅ Basic Python logging (via logging module)
- ✅ Exceptions and error stack traces
- ✅ Application lifecycle events

**Note**: Unlike web frameworks, console apps don't have automatic HTTP request tracing. You need to add library-specific instrumentations.

## ⚠️ Important: Instrumentation Order

**CRITICAL**: The correct initialization order is:
1. Import standard libraries and your dependencies (requests, httpx, etc.) **FIRST**
2. Configure logging with `logging.basicConfig()` **SECOND**
3. Import and call `configure_azure_monitor()` **THIRD**
4. Get logger with `logging.getLogger(__name__)` **FOURTH**
5. Import and call instrumentor `.instrument()` methods **LAST**

**Incorrect Order** (logs won't appear in Application Insights):
```python
from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()  # ❌ Before logging config
logging.basicConfig(level=logging.INFO)
LoggingInstrumentor().instrument()
```

**Correct Order** (logs will appear in Application Insights):
```python
import logging
import requests

logging.basicConfig(level=logging.INFO)  # ✅ 1. Configure logging after regular imports

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()  # ✅ 2. Then configure Azure Monitor

logger = logging.getLogger(__name__)  # ✅ 3. Get logger after configure_azure_monitor

from opentelemetry.instrumentation.logging import LoggingInstrumentor
LoggingInstrumentor().instrument()  # ✅ 4. Finally instrument
```

This order applies to all instrumentors (HTTPXClientInstrumentor, URLLib3Instrumentor, AioHttpClientInstrumentor, AsyncioInstrumentor, etc.).

## Adding Library-Specific Instrumentations

### For HTTP Clients (requests, httpx, urllib3)

```bash
pip install opentelemetry-instrumentation-requests
pip install opentelemetry-instrumentation-httpx
```

```python
import logging
import requests
import httpx

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

logger = logging.getLogger(__name__)

from opentelemetry.instrumentation.requests import RequestsInstrumentor
from opentelemetry.instrumentation.httpx import HTTPXClientInstrumentor
from opentelemetry.instrumentation.logging import LoggingInstrumentor

RequestsInstrumentor().instrument()
HTTPXClientInstrumentor().instrument()
LoggingInstrumentor().instrument()

# Now these calls are automatically traced
response = requests.get("https://api.example.com/data")
```

### For Database Clients (psycopg2, pymongo, redis)

```bash
pip install opentelemetry-instrumentation-psycopg2
pip install opentelemetry-instrumentation-pymongo
pip install opentelemetry-instrumentation-redis
```

```python
import logging
import psycopg2

logging.basicConfig(level=logging.INFO)

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

logger = logging.getLogger(__name__)

from opentelemetry.instrumentation.psycopg2 import Psycopg2Instrumentor
Psycopg2Instrumentor().instrument()

# Database queries are now traced
```

### For Async Operations

```bash
pip install opentelemetry-instrumentation-asyncio
```

```python
import logging
import asyncio

logging.basicConfig(level=logging.INFO)

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

logger = logging.getLogger(__name__)

from opentelemetry.instrumentation.asyncio import AsyncioInstrumentor
AsyncioInstrumentor().instrument()

# Async tasks are now traced
```

## Adding Custom Tracing

Use OpenTelemetry APIs to create custom spans for your business logic:

```python
from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

from opentelemetry import trace

tracer = trace.get_tracer(__name__)

def process_data(data):
    with tracer.start_as_current_span("process_data") as span:
        span.set_attribute("data.size", len(data))
        
        # Your processing logic
        result = do_something(data)
        
        span.set_attribute("result.count", len(result))
        return result

def do_something(data):
    # This automatically becomes a child span
    with tracer.start_as_current_span("do_something"):
        # Processing logic
        return [item * 2 for item in data]
```

## Example: Complete Console Application

```python
"""Example console application with Azure Monitor."""
from dotenv import load_dotenv
load_dotenv()

import logging
import requests
import time

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

logger = logging.getLogger(__name__)

from opentelemetry.instrumentation.requests import RequestsInstrumentor
from opentelemetry.instrumentation.logging import LoggingInstrumentor
RequestsInstrumentor().instrument()
LoggingInstrumentor().instrument()

from opentelemetry import trace

tracer = trace.get_tracer(__name__)

def fetch_data(url):
    """Fetch data from API - automatically traced."""
    logger.info(f"Fetching data from {url}")
    response = requests.get(url, timeout=10)
    response.raise_for_status()
    return response.json()

def process_batch(items):
    """Process items with custom tracing."""
    with tracer.start_as_current_span("process_batch") as span:
        span.set_attribute("batch.size", len(items))
        
        results = []
        for i, item in enumerate(items):
            with tracer.start_as_current_span(f"process_item_{i}"):
                time.sleep(0.1)  # Simulate processing
                results.append(item * 2)
        
        span.set_attribute("results.count", len(results))
        logger.info(f"Processed {len(results)} items")
        return results

def main():
    """Main application entry point."""
    with tracer.start_as_current_span("main") as span:
        logger.info("Application started")
        
        try:
            # Fetch data (HTTP call is auto-traced)
            data = fetch_data("https://jsonplaceholder.typicode.com/posts")
            span.add_event("data_fetched", {"count": len(data)})
            
            # Process data (custom spans)
            results = process_batch(data[:10])
            
            logger.info(f"Application completed successfully. Processed {len(results)} items")
            span.set_status(trace.Status(trace.StatusCode.OK))
            
        except Exception as e:
            logger.error(f"Application failed: {e}", exc_info=True)
            span.record_exception(e)
            span.set_status(trace.Status(trace.StatusCode.ERROR, str(e)))
            raise

if __name__ == "__main__":
    main()
```

## Common Use Cases

### Batch Processing Scripts
```python
with tracer.start_as_current_span("batch_job") as span:
    span.set_attribute("job.type", "daily_report")
    process_records()
```

### CLI Tools
```python
import click

@click.command()
@click.option('--input', required=True)
def cli_tool(input):
    with tracer.start_as_current_span("cli_execution") as span:
        span.set_attribute("cli.input", input)
        # Process CLI logic
```

### Background Workers
```python
while True:
    with tracer.start_as_current_span("worker_iteration"):
        process_queue()
        time.sleep(60)
```

## Available Instrumentations

| Library | Instrumentation Package | What's Traced |
|---------|------------------------|---------------|
| requests | `opentelemetry-instrumentation-requests` | HTTP requests |
| httpx | `opentelemetry-instrumentation-httpx` | HTTP/2 requests |
| urllib3 | `opentelemetry-instrumentation-urllib3` | Low-level HTTP |
| psycopg2 | `opentelemetry-instrumentation-psycopg2` | PostgreSQL queries |
| pymongo | `opentelemetry-instrumentation-pymongo` | MongoDB operations |
| redis | `opentelemetry-instrumentation-redis` | Redis commands |
| asyncio | `opentelemetry-instrumentation-asyncio` | Async tasks |
| logging | Built-in | Log records |

## Configuration Options

### Control Sampling

Sample only 10% of traces to reduce costs:

```python
import os
os.environ["OTEL_TRACES_SAMPLER"] = "traceidratio"
os.environ["OTEL_TRACES_SAMPLER_ARG"] = "0.1"

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()
```

### Set Service Name

```python
import os
os.environ["OTEL_SERVICE_NAME"] = "my-batch-processor"

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()
```

### Add Resource Attributes

```python
import os
os.environ["OTEL_RESOURCE_ATTRIBUTES"] = "deployment.environment=production,service.version=1.2.3"

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()
```

## Viewing Telemetry

Once configured, view your console app telemetry in Azure Portal:
1. Go to your Application Insights resource
2. Navigate to **Transaction search** for individual executions
3. Check **Performance** for operation timing
4. Use **Failures** to track exceptions
5. Create custom queries in **Logs** for batch job analytics

## Next Steps

- OpenTelemetry Pipeline Concepts(see in opentelemetry-pipeline-python.md)
- Azure Monitor Python Overview(see in azure-monitor-python.md)
- [OpenTelemetry Python Documentation](https://opentelemetry.io/docs/instrumentation/python/)
