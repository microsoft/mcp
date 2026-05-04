# Basic Azure Monitor Setup for Django

This guide shows how to add Azure Monitor OpenTelemetry to a Django application.

## Prerequisites

- Python 3.8 or higher
- Django application
- Azure Application Insights resource

## Step 1: Install Packages

```bash
pip install azure-monitor-opentelemetry django
```

Or add to your `requirements.txt`:
```
azure-monitor-opentelemetry
django
```

## Step 2: Initialize at Startup

For Django, you have two options for initialization:

### Option A: In manage.py (Recommended for Development)

```python
#!/usr/bin/env python
"""Django's command-line utility for administrative tasks."""
import os
import sys

def main():
    """Run administrative tasks."""
    os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'mysite.settings')
    
    # Configure Azure Monitor after Django settings are loaded
    from azure.monitor.opentelemetry import configure_azure_monitor
    configure_azure_monitor()
    
    try:
        from django.core.management import execute_from_command_line
    except ImportError as exc:
        raise ImportError(
            "Couldn't import Django. Are you sure it's installed?"
        ) from exc
    execute_from_command_line(sys.argv)

if __name__ == '__main__':
    main()
```

### Option B: In wsgi.py (Recommended for Production)

```python
"""
WSGI config for mysite project.
"""
import os

# Configure Azure Monitor BEFORE Django loads
from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

from django.core.wsgi import get_wsgi_application

os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'mysite.settings')

application = get_wsgi_application()
```

### Option C: In asgi.py (For async Django/Channels)

```python
"""
ASGI config for mysite project.
"""
import os

# Configure Azure Monitor BEFORE Django loads
from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

from django.core.asgi import get_asgi_application

os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'mysite.settings')

application = get_asgi_application()
```

## Step 3: Configure Connection String

Add to your `.env` file or environment:
```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
```

Load in `settings.py`:
```python
from dotenv import load_dotenv
load_dotenv()
```

## What Gets Auto-Instrumented

The Azure Monitor Distro automatically captures:
- ✅ All HTTP requests to your Django views
- ✅ Request duration, status codes, and paths
- ✅ Middleware processing time
- ✅ Exceptions and error details
- ✅ Database queries (PostgreSQL via psycopg2)
- ✅ Outbound HTTP calls

## Step 4: Add Custom Telemetry (Optional)

### In Views

```python
# views.py
from django.http import JsonResponse
from opentelemetry import trace

tracer = trace.get_tracer(__name__)

def order_detail(request, order_id):
    with tracer.start_as_current_span("fetch-order") as span:
        span.set_attribute("order.id", order_id)
        
        try:
            order = Order.objects.get(id=order_id)
            span.set_attribute("order.status", order.status)
            return JsonResponse({"order": order.to_dict()})
        except Order.DoesNotExist:
            span.set_attribute("order.found", False)
            return JsonResponse({"error": "Not found"}, status=404)

def process_order(request):
    span = trace.get_active_span()
    span.set_attribute("user.id", request.user.id)
    
    # Your processing logic
    return JsonResponse({"status": "processed"})
```

### In Class-Based Views

```python
from django.views import View
from django.http import JsonResponse
from opentelemetry import trace

tracer = trace.get_tracer(__name__)

class OrderView(View):
    def get(self, request, order_id):
        with tracer.start_as_current_span("get-order") as span:
            span.set_attribute("order.id", order_id)
            order = Order.objects.get(id=order_id)
            return JsonResponse(order.to_dict())
    
    def post(self, request):
        with tracer.start_as_current_span("create-order") as span:
            # Create order logic
            order = Order.objects.create(**request.POST)
            span.set_attribute("order.id", order.id)
            return JsonResponse({"id": order.id}, status=201)
```

## Step 5: Add Logging Integration

```python
# settings.py
LOGGING = {
    'version': 1,
    'disable_existing_loggers': False,
    'handlers': {
        'console': {
            'class': 'logging.StreamHandler',
        },
    },
    'root': {
        'handlers': ['console'],
        'level': 'INFO',
    },
    'loggers': {
        'django': {
            'handlers': ['console'],
            'level': 'INFO',
            'propagate': False,
        },
    },
}
```

```python
# views.py
import logging

logger = logging.getLogger(__name__)

def my_view(request):
    logger.info("Processing request")  # Automatically correlated with trace
    # ...
```

## Complete Example Structure

```
mysite/
├── manage.py           # Add configure_azure_monitor() here
├── requirements.txt
├── .env
└── mysite/
    ├── __init__.py
    ├── settings.py
    ├── urls.py
    ├── wsgi.py         # Or add configure_azure_monitor() here
    └── asgi.py
```

### manage.py
```python
#!/usr/bin/env python
import os
import sys
from dotenv import load_dotenv

load_dotenv()

from azure.monitor.opentelemetry import configure_azure_monitor
configure_azure_monitor()

def main():
    os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'mysite.settings')
    from django.core.management import execute_from_command_line
    execute_from_command_line(sys.argv)

if __name__ == '__main__':
    main()
```

## Running the Application

```bash
# Set connection string
export APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=..."

# Run Django
python manage.py runserver
```

Or for production with Gunicorn:
```bash
gunicorn mysite.wsgi:application
```

## Verification

1. Make requests to your Django endpoints
2. Go to Azure Portal → Application Insights
3. Check "Transaction search" for your requests
4. View database dependency calls

## Links

- [Django Documentation](https://docs.djangoproject.com/)
- [Azure Monitor Python](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable?tabs=python)
