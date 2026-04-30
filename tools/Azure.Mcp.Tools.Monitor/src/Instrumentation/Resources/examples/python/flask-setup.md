# Basic Azure Monitor Setup for Flask

This guide shows how to add Azure Monitor OpenTelemetry to a Flask application.

## Prerequisites

- Python 3.8 or higher
- Flask application
- Azure Application Insights resource

## Step 1: Install Packages

```bash
pip install azure-monitor-opentelemetry flask
```

Or add to your `requirements.txt`:
```
azure-monitor-opentelemetry
flask
```

## Step 2: Initialize at Startup

Update your main application file (e.g., `app.py`):

```python
# IMPORTANT: Configure Azure Monitor BEFORE importing Flask
from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

# Now import Flask
from flask import Flask, jsonify, request

app = Flask(__name__)

@app.route('/')
def hello():
    return jsonify({"message": "Hello, World!"})

@app.route('/api/users')
def get_users():
    # This request is automatically tracked
    return jsonify([
        {"id": 1, "name": "Alice"},
        {"id": 2, "name": "Bob"}
    ])

if __name__ == '__main__':
    app.run(debug=True)
```

## Step 3: Configure Connection String

Create a `.env` file:
```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
FLASK_ENV=development
```

Load environment variables:
```python
from dotenv import load_dotenv
load_dotenv()

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

from flask import Flask
# ... rest of your app
```

## What Gets Auto-Instrumented

The Azure Monitor Distro automatically captures:
- ✅ All HTTP requests to your Flask routes
- ✅ Request duration, status codes, and paths
- ✅ Exceptions and error details
- ✅ Outbound HTTP calls (requests, urllib3)
- ✅ Database queries (psycopg2 for PostgreSQL)

## Step 4: Add Custom Telemetry (Optional)

```python
from flask import Flask, jsonify
from opentelemetry import trace

app = Flask(__name__)
tracer = trace.get_tracer(__name__)

@app.route('/api/orders/<order_id>')
def get_order(order_id):
    # Add custom span for business logic
    with tracer.start_as_current_span("fetch-order") as span:
        span.set_attribute("order.id", order_id)
        
        # Simulate database fetch
        order = fetch_order_from_db(order_id)
        
        span.set_attribute("order.status", order.get("status"))
        return jsonify(order)

@app.route('/api/process', methods=['POST'])
def process_data():
    span = trace.get_active_span()
    
    # Add context to current request span
    span.set_attribute("user.id", request.headers.get("X-User-ID"))
    span.set_attribute("request.size", len(request.data))
    
    try:
        result = do_processing(request.json)
        return jsonify(result)
    except Exception as e:
        span.record_exception(e)
        return jsonify({"error": str(e)}), 500
```

## Step 5: Add Logging Integration

```python
import logging
from flask import Flask

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = Flask(__name__)

@app.route('/api/users/<user_id>')
def get_user(user_id):
    logger.info(f"Fetching user {user_id}")  # Correlated with trace
    
    user = fetch_user(user_id)
    
    if not user:
        logger.warning(f"User {user_id} not found")
        return jsonify({"error": "Not found"}), 404
    
    logger.info(f"Found user {user_id}")
    return jsonify(user)
```

## Complete Example

```python
# app.py
import os
import logging
from dotenv import load_dotenv

# Load environment variables first
load_dotenv()

# Configure Azure Monitor BEFORE importing Flask
from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

# Now import Flask and other dependencies
from flask import Flask, jsonify, request
from opentelemetry import trace

# Setup logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Create Flask app
app = Flask(__name__)
tracer = trace.get_tracer(__name__)

@app.route('/')
def index():
    logger.info("Index page accessed")
    return jsonify({"status": "healthy"})

@app.route('/api/items')
def list_items():
    with tracer.start_as_current_span("list-items") as span:
        items = [{"id": 1, "name": "Item 1"}, {"id": 2, "name": "Item 2"}]
        span.set_attribute("items.count", len(items))
        return jsonify(items)

@app.errorhandler(Exception)
def handle_error(error):
    logger.error(f"Unhandled error: {error}")
    return jsonify({"error": "Internal server error"}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
```

## Running the Application

```bash
# Set connection string
export APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=..."

# Run Flask
python app.py
```

Or with Flask CLI:
```bash
flask run
```

## Verification

1. Make requests to your Flask endpoints
2. Go to Azure Portal → Application Insights
3. Check "Transaction search" for your requests
4. View "Application map" for dependencies

## Links

- [Flask Documentation](https://flask.palletsprojects.com/)
- [Azure Monitor Python](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable?tabs=python)
