# OpenTelemetry Pipeline for Node.js

## Overview

The OpenTelemetry pipeline is the data flow path for telemetry signals (traces, metrics, logs) from your Node.js application to observability backends like Azure Monitor.

## Pipeline Components

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Instrumentation │ ──▶ │   Processors    │ ──▶ │    Exporters    │
│  (Sources)       │     │   (Transform)   │     │   (Backends)    │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

### 1. Instrumentation (Sources)
- **HTTP Instrumentation** - Automatically captures HTTP requests/responses
- **Database Instrumentation** - Tracks database queries (MongoDB, PostgreSQL, MySQL, etc.)
- **Custom Spans** - Manual instrumentation using OpenTelemetry API
- **Logs** - Console logs and structured logging

### 2. Processors (Transform)
- **Span Processors** - Modify or filter spans before export
- **Batch Processors** - Batch telemetry for efficient export
- **Sampling** - Control volume of telemetry sent

### 3. Exporters (Backends)
- **Azure Monitor Exporter** - Sends to Application Insights
- **OTLP Exporter** - Sends to any OTLP-compatible backend
- **Console Exporter** - Debug output to console

## In Node.js with Express

```javascript
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');
const { trace } = require('@opentelemetry/api');

// Initialize with Azure Monitor exporter
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  },
  // Optional: Configure sampling
  samplingRatio: 1.0 // 100% of requests
});

// Your Express app
const express = require('express');
const app = express();

// HTTP requests are automatically instrumented
app.get('/api/users', async (req, res) => {
  // Get current span for custom attributes
  const span = trace.getActiveSpan();
  span?.setAttribute('user.role', 'admin');
  
  res.json({ users: [] });
});
```

## In Next.js

Next.js uses a special `instrumentation.js` hook instead of inline initialization. The key differences from Express/standard Node.js:

1. **Instrumentation hook**: Create `instrumentation.js` at the project root with a `register()` export
2. **Runtime check**: Guard with `process.env.NEXT_RUNTIME === 'nodejs'` to avoid Edge runtime
3. **Webpack externals**: Must externalize OpenTelemetry packages in `next.config.js` to prevent webpack from bundling Node.js-only modules
4. **Logging libraries (Next.js only)**: In Next.js, libraries like Bunyan and Winston must also be added to webpack externals because they have native/optional dependencies that webpack cannot resolve. This is **not** required in standard Node.js apps (Express, Fastify, etc.) where these libraries work out of the box.

```javascript
// instrumentation.js (project root)
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

export function register() {
    if (process.env.NEXT_RUNTIME === 'nodejs') {
        useAzureMonitor({
            azureMonitorExporterOptions: {
                connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
            }
        });
    }
}
```

See Next.js Setup Guide(see in basic-setup-nextjs.md) for the complete configuration including webpack externals.

## Automatic Instrumentation

The `@azure/monitor-opentelemetry` package automatically instruments:

- ✅ **HTTP/HTTPS** - Incoming and outgoing requests
- ✅ **Express** - Routes, middleware, error handlers
- ✅ **Next.js** - API routes, Server Components (via instrumentation hook)
- ✅ **MongoDB** - Queries and operations
- ✅ **MySQL/PostgreSQL** - Database queries
- ✅ **Redis** - Cache operations
- ✅ **DNS** - DNS lookups
- ✅ **File System** - I/O operations (when configured)

## Manual Instrumentation

For custom telemetry:

```javascript
const { trace } = require('@opentelemetry/api');

// Get tracer
const tracer = trace.getTracer('my-app');

// Create custom span
const span = tracer.startSpan('process-data');
try {
  // Your business logic
  span.setAttribute('record.count', 100);
  span.addEvent('Processing started');
  
  // ... do work ...
  
  span.addEvent('Processing completed');
} catch (error) {
  span.recordException(error);
  span.setStatus({ code: SpanStatusCode.ERROR });
} finally {
  span.end();
}
```

## Configuration Options

```javascript
useAzureMonitor({
  // Connection string
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  },
  
  // Sampling (0.0 to 1.0)
  samplingRatio: 0.5, // 50% of requests
  
  // Resource attributes
  resource: {
    attributes: {
      'service.name': 'my-express-api',
      'service.version': '1.0.0',
      'deployment.environment': 'production'
    }
  },
  
  // Instrumentation configuration
  instrumentationOptions: {
    http: { enabled: true },
    mongoDb: { enabled: true },
    express: { enabled: true }
  }
});
```

## Telemetry Types

### Traces (Spans)
- Request/response flows
- Database queries
- External API calls
- Custom operations

### Metrics
- Request counts
- Response times
- Error rates
- Custom counters/gauges

### Logs
- Console output (`console.log`, `console.error`)
- Structured logging frameworks (Winston, Bunyan)
- Exception traces

## Best Practices

1. **Initialize Early** - Call `useAzureMonitor()` before importing application code
2. **Use Environment Variables** - Store connection strings securely
3. **Enable Sampling** - For high-volume apps, use sampling to control costs
4. **Add Context** - Use custom attributes to enrich telemetry
5. **Handle Errors** - Always record exceptions in custom spans

## Comparison: Application Insights SDK vs OpenTelemetry

| Feature | Classic SDK | OpenTelemetry |
|---------|-------------|---------------|
| Initialization | `appInsights.setup()` | `useAzureMonitor()` |
| Custom Tracking | `trackEvent()`, `trackTrace()` | `span.addEvent()`, `console.log()` |
| Dependencies | Automatic | Automatic (via instrumentations) |
| Vendor Lock-in | Azure-specific | Vendor-neutral (CNCF standard) |
| Future Support | Limited | Full Microsoft commitment |

## See Also

- Azure Monitor for Node.js(see in azure-monitor-nodejs.md)
- Express Setup Guide(see in basic-setup-express.md)
- Next.js Setup Guide(see in basic-setup-nextjs.md)
- Bunyan Setup Guide(see in basic-setup-bunyan-nodejs.md)
- [OpenTelemetry Documentation](https://opentelemetry.io/docs/instrumentation/js/)
