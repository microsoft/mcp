# Basic Azure Monitor Setup for Node.js with Winston Logging

This guide shows how to add Azure Monitor OpenTelemetry to a Node.js application using Winston for logging.

## Prerequisites

- Node.js 14.x or higher
- npm or yarn
- Node.js application with Winston (`winston` package)
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

// Initialize Azure Monitor with Winston log collection enabled
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  },
  instrumentationOptions: {
    winston: { enabled: true }
  }
});

// Now load your application code
const express = require('express');
const winston = require('winston');

// Create Winston logger
const logger = winston.createLogger({
  level: process.env.LOG_LEVEL || 'info',
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.errors({ stack: true }),
    winston.format.json()
  ),
  defaultMeta: { service: 'my-app' },
  transports: [
    new winston.transports.Console({
      format: winston.format.combine(
        winston.format.colorize(),
        winston.format.simple()
      )
    })
  ]
});

const app = express();
const port = process.env.PORT || 3000;

app.use(express.json());

// Request logging middleware
app.use((req, res, next) => {
  logger.info('Incoming request', {
    method: req.method,
    path: req.path
  });
  next();
});

app.get('/api/users', (req, res) => {
  logger.info('Fetching users');
  res.json([{ id: 1, name: 'Alice' }]);
});

app.listen(port, () => {
  logger.info(`Server listening on port ${port}`);
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

With Winston instrumentation enabled, the following log data is sent to Azure Monitor:

- **Log level**: error, warn, info, debug, etc.
- **Log message**: The log content
- **Metadata**: Any additional properties passed to the logger
- **Timestamp**: When the log was created
- **Trace context**: Correlation with distributed traces

## Log Level Mapping

Winston log levels are mapped to Application Insights severity levels:

| Winston Level | Application Insights Severity |
|--------------|------------------------------|
| error | Error |
| warn | Warning |
| info | Information |
| http | Information |
| verbose | Verbose |
| debug | Verbose |
| silly | Verbose |

## Step 4: Structured Logging Best Practices

### Add Context to Logs

```javascript
// Good: Structured logging with context
logger.info('User created', {
  userId: user.id,
  email: user.email,
  action: 'create'
});

// Good: Error logging with stack trace
logger.error('Failed to create user', {
  error: err.message,
  stack: err.stack,
  email: req.body.email
});
```

### Correlation with Requests

Logs are automatically correlated with HTTP requests when using Express:

```javascript
app.get('/api/users/:id', (req, res) => {
  // This log will be correlated with the request trace
  logger.info('Fetching user', { userId: req.params.id });
  
  try {
    const user = getUserById(req.params.id);
    logger.debug('User found', { user });
    res.json(user);
  } catch (error) {
    logger.error('User not found', { 
      userId: req.params.id,
      error: error.message 
    });
    res.status(404).json({ error: 'User not found' });
  }
});
```

## Step 5: Custom Telemetry (Optional)

Combine logging with custom spans:

```javascript
const { trace } = require('@opentelemetry/api');

app.post('/api/orders', async (req, res) => {
  const tracer = trace.getTracer('my-app');
  
  await tracer.startActiveSpan('create-order', async (span) => {
    logger.info('Creating order', { items: req.body.items.length });
    
    try {
      const order = await createOrder(req.body);
      
      logger.info('Order created successfully', {
        orderId: order.id,
        total: order.total
      });
      
      span.setAttribute('order.id', order.id);
      res.status(201).json(order);
    } catch (error) {
      logger.error('Failed to create order', {
        error: error.message,
        stack: error.stack
      });
      
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
| where customDimensions.service == "my-app"
| project timestamp, message, severityLevel, customDimensions
| order by timestamp desc
```

4. Use "Transaction search" to see logs correlated with requests

## Troubleshooting

### Winston logs not appearing

1. Ensure `winston` instrumentation is enabled in options
2. Verify `useAzureMonitor()` is called **before** importing `winston`
3. Check that the connection string is valid

### Too many logs being sent

Control log volume by adjusting the log level:

```javascript
const logger = winston.createLogger({
  level: process.env.NODE_ENV === 'production' ? 'warn' : 'debug',
  // ...
});
```

Or use sampling:

```javascript
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  },
  instrumentationOptions: {
    winston: { enabled: true }
  },
  samplingRatio: 0.5 // Sample 50% of telemetry
});
```
