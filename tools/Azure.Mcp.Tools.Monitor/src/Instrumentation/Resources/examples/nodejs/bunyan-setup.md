# Basic Azure Monitor Setup for Node.js with Bunyan Logging

This guide shows how to add Azure Monitor OpenTelemetry to a Node.js application using Bunyan for logging.

## Prerequisites

- Node.js 14.x or higher
- npm or yarn
- Node.js application with Bunyan (`bunyan` package)
- Azure Application Insights resource

## Step 1: Install Package

```bash
npm install @azure/monitor-opentelemetry
```

## Step 2: Initialize at Startup

Create or update your main entry point (typically `index.js` or `server.js`):

```javascript
// IMPORTANT: This must be the first line, before any other imports
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

// Initialize Azure Monitor with Bunyan log collection enabled
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  },
  instrumentationOptions: {
    bunyan: { enabled: true }
  }
});

// Now load your application code
const express = require('express');
const bunyan = require('bunyan');

// Create Bunyan logger
const logger = bunyan.createLogger({
  name: 'my-app',
  level: process.env.LOG_LEVEL || 'info',
  serializers: bunyan.stdSerializers
});

const app = express();
const port = process.env.PORT || 3000;

app.use(express.json());

// Request logging middleware
app.use((req, res, next) => {
  logger.info({ req }, 'Incoming request');
  next();
});

app.get('/api/users', (req, res) => {
  logger.info('Fetching users');
  res.json([{ id: 1, name: 'Alice' }]);
});

app.listen(port, () => {
  logger.info({ port }, 'Server listening');
});
```

## Step 3: Configure Connection String

Create a `.env` file in your project root:

```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
LOG_LEVEL=info
PORT=3000
```

Install `dotenv` to load environment variables:

```bash
npm install dotenv
```

Load it at the very top of your entry file:

```javascript
require('dotenv').config();
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');
// ... rest of code
```

## What Gets Collected

With Bunyan instrumentation enabled, the following log data is sent to Azure Monitor:

- **Log level**: fatal, error, warn, info, debug, trace
- **Log message**: The log content
- **Log fields**: All additional fields passed to the logger
- **Timestamp**: When the log was created
- **Trace context**: Correlation with distributed traces

## Log Level Mapping

Bunyan log levels are mapped to Application Insights severity levels:

| Bunyan Level | Numeric | Application Insights Severity |
|-------------|---------|------------------------------|
| fatal | 60 | Critical |
| error | 50 | Error |
| warn | 40 | Warning |
| info | 30 | Information |
| debug | 20 | Verbose |
| trace | 10 | Verbose |

## Step 4: Structured Logging Best Practices

Bunyan excels at structured logging. Use its features effectively:

### Add Context to Logs

```javascript
// Good: Structured logging with context
logger.info({ userId: user.id, email: user.email }, 'User created');

// Good: Error logging with error serializer
logger.error({ err: error, email: req.body.email }, 'Failed to create user');

// Good: Request/response logging
logger.info({ req, res }, 'Request completed');
```

### Child Loggers for Request Context

```javascript
app.use((req, res, next) => {
  // Create a child logger with request context
  req.log = logger.child({
    requestId: req.headers['x-request-id'] || Math.random().toString(36).substr(2, 9),
    path: req.path,
    method: req.method
  });
  next();
});

app.get('/api/users/:id', (req, res) => {
  // Use the request-scoped logger
  req.log.info({ userId: req.params.id }, 'Fetching user');
  
  try {
    const user = getUserById(req.params.id);
    req.log.debug({ user }, 'User found');
    res.json(user);
  } catch (error) {
    req.log.error({ err: error, userId: req.params.id }, 'User not found');
    res.status(404).json({ error: 'User not found' });
  }
});
```

### Custom Serializers

```javascript
const logger = bunyan.createLogger({
  name: 'my-app',
  serializers: {
    ...bunyan.stdSerializers,
    user: (user) => ({
      id: user.id,
      email: user.email,
      role: user.role
    }),
    order: (order) => ({
      id: order.id,
      total: order.total,
      itemCount: order.items.length
    })
  }
});

// Use custom serializers
logger.info({ user }, 'User logged in');
logger.info({ order }, 'Order created');
```

## Step 5: Custom Telemetry (Optional)

Combine logging with custom spans:

```javascript
const { trace } = require('@opentelemetry/api');

app.post('/api/orders', async (req, res) => {
  const tracer = trace.getTracer('my-app');
  
  await tracer.startActiveSpan('create-order', async (span) => {
    req.log.info({ itemCount: req.body.items.length }, 'Creating order');
    
    try {
      const order = await createOrder(req.body);
      
      req.log.info({ order }, 'Order created successfully');
      
      span.setAttribute('order.id', order.id);
      res.status(201).json(order);
    } catch (error) {
      req.log.error({ err: error }, 'Failed to create order');
      
      span.recordException(error);
      span.setStatus({ code: 2, message: error.message });
      res.status(500).json({ error: 'Failed to create order' });
    } finally {
      span.end();
    }
  });
});
```

## Viewing Logs in Azure Portal

1. Open your Application Insights resource in Azure Portal
2. Navigate to "Logs" under Monitoring
3. Query traces table:

```kusto
traces
| where timestamp > ago(1h)
| where customDimensions.name == "my-app"
| project timestamp, message, severityLevel, customDimensions
| order by timestamp desc
```

4. Use "Transaction search" to see logs correlated with requests

## Troubleshooting

### Bunyan logs not appearing

1. Ensure `bunyan` instrumentation is enabled in options
2. Verify `useAzureMonitor()` is called **before** importing `bunyan`
3. Check that the connection string is valid

### Using Bunyan with Next.js

> **Note**: The standard setup in this guide (Steps 1-5) works for Express, Fastify, NestJS, and other standard Node.js frameworks with no special configuration. The issues below are **specific to Next.js** because it bundles server-side code with webpack.

Bunyan has optional native dependencies (`dtrace-provider`, `source-map-support`) that Next.js's webpack bundler cannot resolve. You'll see errors like:

- `Module not found: Can't resolve 'source-map-support'`
- `Module not found: Can't resolve './src/build'` (from `dtrace-provider`)

**Fix**: Add `bunyan` to both `serverComponentsExternalPackages` and `webpack.externals` in your `next.config.js`:

```javascript
const nextConfig = {
    experimental: {
        instrumentationHook: true,
        serverComponentsExternalPackages: [
            '@azure/monitor-opentelemetry',
            // ... other OpenTelemetry packages ...
            'bunyan',
        ],
    },
    webpack: (config, { isServer }) => {
        if (isServer) {
            config.externals = config.externals || [];
            config.externals.push({
                // ... other OpenTelemetry externals ...
                bunyan: 'commonjs bunyan',
            });
        }
        return config;
    },
};
```

In Next.js, also mark API routes using bunyan with `export const runtime = 'nodejs'` to ensure they run on the Node.js runtime (not Edge). This is a Next.js-specific concept and does not apply to standard Node.js apps.

See the Next.js Setup Guide(see in basic-setup-nextjs.md) for the full Next.js + bunyan configuration.

### Log fields not appearing correctly

Ensure you're using the correct Bunyan logging format:

```javascript
// Correct: fields object first, then message
logger.info({ userId: 123 }, 'User action');

// Incorrect: message first (fields won't be captured properly)
logger.info('User action', { userId: 123 });
```

### Too many logs being sent

Control log volume by adjusting the log level:

```javascript
const logger = bunyan.createLogger({
  name: 'my-app',
  level: process.env.NODE_ENV === 'production' ? 'warn' : 'debug'
});
```
