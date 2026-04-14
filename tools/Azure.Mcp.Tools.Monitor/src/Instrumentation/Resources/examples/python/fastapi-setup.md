# Basic Azure Monitor Setup for FastAPI

This guide shows how to add Azure Monitor OpenTelemetry to a FastAPI application.

## Prerequisites

- Python 3.8 or higher
- FastAPI application
- Azure Application Insights resource

## Step 1: Install Packages

```bash
pip install azure-monitor-opentelemetry fastapi uvicorn
```

Or add to your `requirements.txt`:
```
azure-monitor-opentelemetry
fastapi
uvicorn[standard]
```

## Step 2: Initialize at Startup

Update your main application file (e.g., `main.py` or `app.py`):

```python
# IMPORTANT: Configure Azure Monitor BEFORE importing FastAPI
from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

# Now import FastAPI
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

app = FastAPI(title="My API")

@app.get("/")
async def root():
    return {"message": "Hello, World!"}

@app.get("/api/users/{user_id}")
async def get_user(user_id: int):
    # This request is automatically tracked
    return {"id": user_id, "name": f"User {user_id}"}
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

from fastapi import FastAPI
# ... rest of your app
```

## What Gets Auto-Instrumented

The Azure Monitor Distro automatically captures:
- ✅ All HTTP requests to your FastAPI routes
- ✅ Request duration, status codes, and paths
- ✅ Path parameters and query strings
- ✅ Exceptions and error details
- ✅ Outbound HTTP calls (httpx, requests, aiohttp)
- ✅ Database queries (asyncpg, psycopg2)

## Step 4: Add Custom Telemetry (Optional)

```python
from fastapi import FastAPI, Request
from opentelemetry import trace

app = FastAPI()
tracer = trace.get_tracer(__name__)

class Order(BaseModel):
    item: str
    quantity: int

@app.post("/api/orders")
async def create_order(order: Order, request: Request):
    with tracer.start_as_current_span("create-order") as span:
        span.set_attribute("order.item", order.item)
        span.set_attribute("order.quantity", order.quantity)
        
        # Your business logic
        order_id = await save_order(order)
        
        span.set_attribute("order.id", order_id)
        return {"id": order_id, "status": "created"}

@app.get("/api/orders/{order_id}")
async def get_order(order_id: str):
    span = trace.get_active_span()
    span.set_attribute("order.id", order_id)
    
    order = await fetch_order(order_id)
    if not order:
        span.set_attribute("order.found", False)
        raise HTTPException(status_code=404, detail="Order not found")
    
    return order
```

## Step 5: Add Middleware for Custom Context

```python
from fastapi import FastAPI, Request
from opentelemetry import trace

app = FastAPI()

@app.middleware("http")
async def add_custom_context(request: Request, call_next):
    span = trace.get_active_span()
    
    # Add custom attributes to every request
    if span:
        span.set_attribute("http.user_agent", request.headers.get("user-agent", ""))
        span.set_attribute("custom.request_id", request.headers.get("x-request-id", ""))
    
    response = await call_next(request)
    return response
```

## Step 6: Add Logging Integration

```python
import logging
from fastapi import FastAPI

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI()

@app.get("/api/process/{item_id}")
async def process_item(item_id: str):
    logger.info(f"Processing item {item_id}")  # Correlated with trace
    
    try:
        result = await do_processing(item_id)
        logger.info(f"Successfully processed item {item_id}")
        return result
    except Exception as e:
        logger.error(f"Failed to process item {item_id}: {e}")
        raise
```

## Complete Example

```python
# main.py
import os
import logging
from dotenv import load_dotenv

# Load environment variables first
load_dotenv()

# Configure Azure Monitor BEFORE importing FastAPI
from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

# Now import FastAPI and other dependencies
from fastapi import FastAPI, HTTPException, Request
from pydantic import BaseModel
from opentelemetry import trace

# Setup logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Create FastAPI app
app = FastAPI(
    title="My API",
    description="API with Azure Monitor integration",
    version="1.0.0"
)
tracer = trace.get_tracer(__name__)

class Item(BaseModel):
    name: str
    price: float

@app.get("/")
async def root():
    return {"status": "healthy"}

@app.get("/api/items")
async def list_items():
    with tracer.start_as_current_span("list-items") as span:
        items = [
            {"id": 1, "name": "Item 1", "price": 10.0},
            {"id": 2, "name": "Item 2", "price": 20.0}
        ]
        span.set_attribute("items.count", len(items))
        logger.info(f"Returning {len(items)} items")
        return items

@app.post("/api/items")
async def create_item(item: Item):
    with tracer.start_as_current_span("create-item") as span:
        span.set_attribute("item.name", item.name)
        span.set_attribute("item.price", item.price)
        logger.info(f"Creating item: {item.name}")
        return {"id": 1, **item.dict()}

@app.exception_handler(Exception)
async def global_exception_handler(request: Request, exc: Exception):
    logger.error(f"Unhandled error: {exc}")
    return {"error": "Internal server error"}
```

## Running the Application

```bash
# Set connection string
export APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=..."

# Run with uvicorn
uvicorn main:app --reload
```

Or programmatically:
```python
if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
```

## Verification

1. Make requests to your FastAPI endpoints
2. Use the auto-generated docs at `/docs` to test
3. Go to Azure Portal → Application Insights
4. Check "Transaction search" for your requests
5. View "Live Metrics" for real-time data

## Links

- [FastAPI Documentation](https://fastapi.tiangolo.com/)
- [Azure Monitor Python](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable?tabs=python)
- [OpenTelemetry FastAPI Instrumentation](https://opentelemetry-python-contrib.readthedocs.io/en/latest/instrumentation/fastapi/fastapi.html)
