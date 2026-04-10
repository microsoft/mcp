# Basic Azure Monitor Setup for Node.js with Console Logging

This guide shows how to add Azure Monitor to a Node.js application that uses built-in console logging.

> **Important**: To capture `console.log`, `console.warn`, `console.error`, etc. as telemetry in Application Insights, you must use the `applicationinsights` npm package (the Application Insights SDK). The `@azure/monitor-opentelemetry` package does **not** support automatic console log collection — it only supports structured logging libraries like Bunyan and Winston.

## Prerequisites

- Node.js 14.x or higher
- npm or yarn
- Node.js application using console logging
- Azure Application Insights resource

## Step 1: Install Package

```bash
npm install applicationinsights
```

## Step 2: Initialize at Startup

Create or update your main entry point (typically `index.js` or `server.js`):

```javascript
// IMPORTANT: This must be the first line, before any other imports
const appInsights = require('applicationinsights');

// Initialize Application Insights with console log collection
appInsights.setup(process.env.APPLICATIONINSIGHTS_CONNECTION_STRING)
  .setAutoCollectConsole(true, true) // Enable console.log and console.error collection
  .start();

// Now load your application code
const express = require('express');

const app = express();
const port = process.env.PORT || 3000;

app.use(express.json());

// Request logging middleware
app.use((req, res, next) => {
  console.log(`[${new Date().toISOString()}] ${req.method} ${req.path}`);
  next();
});

app.get('/api/users', (req, res) => {
  console.log('Fetching users');
  res.json([{ id: 1, name: 'Alice' }]);
});

app.listen(port, () => {
  console.log(`Server listening on port ${port}`);
});
```

## Step 3: Configure Connection String

Create a `.env` file in your project root:

```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
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

## Why `applicationinsights` Instead of `@azure/monitor-opentelemetry`?

The `@azure/monitor-opentelemetry` package is the recommended OpenTelemetry-based SDK for Node.js, but it only supports automatic log collection from **Bunyan** and **Winston** logging libraries. It does **not** have a `console` instrumentation option.

The `applicationinsights` package (Application Insights Node.js SDK) provides built-in support for capturing `console.log` output via `setAutoCollectConsole(true)`. If you want to use the OpenTelemetry-based SDK instead, consider migrating to Winston or Bunyan for structured logging.

## What Gets Collected

With `setAutoCollectConsole(true, true)` enabled, the following is captured:

| Console Method | Application Insights Severity |
|---------------|------------------------------|
| console.error() | Error |
| console.warn() | Warning |
| console.info() | Information |
| console.log() | Information |
| console.debug() | Verbose |
| console.trace() | Verbose |

## Step 4: Best Practices for Console Logging

### Use Appropriate Log Levels

```javascript
// Error conditions
console.error('Failed to connect to database:', error.message);

// Warning conditions
console.warn('API rate limit approaching:', currentRate);

// Informational messages
console.info('User logged in:', userId);
console.log('Processing request for:', endpoint);

// Debug information
console.debug('Request body:', JSON.stringify(body));
```

### Structured-ish Logging with Console

While console doesn't support true structured logging, you can format messages consistently:

```javascript
function log(level, message, data = {}) {
  const timestamp = new Date().toISOString();
  const dataStr = Object.keys(data).length ? ` ${JSON.stringify(data)}` : '';
  console[level](`[${timestamp}] ${message}${dataStr}`);
}

// Usage
log('info', 'User created', { userId: 123, email: 'user@example.com' });
log('error', 'Database connection failed', { host: 'localhost', error: err.message });
```

### Request Context Helper

```javascript
function createRequestLogger(req) {
  const requestId = req.headers['x-request-id'] || Math.random().toString(36).substr(2, 9);
  
  return {
    info: (message, data = {}) => {
      console.log(`[${requestId}] ${message}`, data);
    },
    error: (message, data = {}) => {
      console.error(`[${requestId}] ${message}`, data);
    },
    warn: (message, data = {}) => {
      console.warn(`[${requestId}] ${message}`, data);
    }
  };
}

app.use((req, res, next) => {
  req.log = createRequestLogger(req);
  next();
});

app.get('/api/users/:id', (req, res) => {
  req.log.info(`Fetching user ${req.params.id}`);
  // ...
});
```

## Step 5: Custom Telemetry (Optional)

Combine logging with custom spans:

```javascript
const { trace } = require('@opentelemetry/api');

app.post('/api/orders', async (req, res) => {
  const tracer = trace.getTracer('my-app');
  
  await tracer.startActiveSpan('create-order', async (span) => {
    console.log('Creating order with', req.body.items.length, 'items');
    
    try {
      const order = await createOrder(req.body);
      
      console.log('Order created successfully:', order.id);
      
      span.setAttribute('order.id', order.id);
      res.status(201).json(order);
    } catch (error) {
      console.error('Failed to create order:', error.message);
      
      span.recordException(error);
      span.setStatus({ code: 2, message: error.message });
      res.status(500).json({ error: 'Failed to create order' });
    } finally {
      span.end();
    }
  });
});
```

## When to Consider Upgrading to a Logging Library

Console logging is fine for simple applications, but consider Winston or Bunyan when you need:

- **Structured logging**: Proper JSON logging with metadata
- **Log levels**: Configurable filtering by environment
- **Multiple transports**: File, HTTP, or other destinations
- **Better performance**: Async logging for high-throughput apps
- **Request correlation**: Automatic trace context propagation

## Viewing Logs in Azure Portal

1. Open your Application Insights resource in Azure Portal
2. Navigate to "Logs" under Monitoring
3. Query traces table:

```kusto
traces
| where timestamp > ago(1h)
| project timestamp, message, severityLevel
| order by timestamp desc
```

4. Filter by severity:

```kusto
traces
| where timestamp > ago(1h)
| where severityLevel >= 3  // Warning and above
| project timestamp, message, severityLevel
| order by timestamp desc
```

## Troubleshooting

### Console logs not appearing

1. Ensure `setAutoCollectConsole(true, true)` is called during setup
2. Verify `appInsights.setup().start()` is called **before** any console.log calls
3. Check that the connection string is valid
4. For short-lived apps (scripts that exit immediately), call `appInsights.defaultClient.flush()` before the process exits to ensure telemetry is sent

### Too many logs being sent

Console instrumentation captures all console output. To reduce volume:

1. Use sampling:

```javascript
const appInsights = require('applicationinsights');
appInsights.setup(process.env.APPLICATIONINSIGHTS_CONNECTION_STRING)
  .setAutoCollectConsole(true, true)
  .start();

// Set sampling percentage (e.g., 50%)
appInsights.defaultClient.config.samplingPercentage = 50;
```

2. Or only collect console.error (not console.log):

```javascript
// First parameter: collect console.log (false = disabled)
// Second parameter: collect console.error (true = enabled)
appInsights.setup(process.env.APPLICATIONINSIGHTS_CONNECTION_STRING)
  .setAutoCollectConsole(false, true)
  .start();
```

## Migration Path to Structured Logging

When you're ready to upgrade, here's a simple migration to Winston:

```javascript
// Before: console
console.log('User created:', userId);
console.error('Database error:', error.message);

// After: Winston
const winston = require('winston');
const logger = winston.createLogger({
  level: 'info',
  format: winston.format.json(),
  transports: [new winston.transports.Console()]
});

logger.info('User created', { userId });
logger.error('Database error', { error: error.message });
```
