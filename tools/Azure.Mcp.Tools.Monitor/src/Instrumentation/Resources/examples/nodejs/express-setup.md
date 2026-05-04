# Basic Azure Monitor Setup for Express.js

This guide shows how to add Azure Monitor OpenTelemetry to an Express.js application.

## Prerequisites

- Node.js 14.x or higher
- npm or yarn
- Express.js application
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

// Initialize Azure Monitor
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  }
});

// Now load your application code
const express = require('express');
const app = express();
const port = process.env.PORT || 3000;

// Your middleware and routes
app.use(express.json());

app.get('/', (req, res) => {
  res.json({ message: 'Hello World!' });
});

app.get('/api/users', (req, res) => {
  // This request will be automatically tracked
  res.json([{ id: 1, name: 'Alice' }, { id: 2, name: 'Bob' }]);
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

## Step 4: Add Custom Telemetry (Optional)

```javascript
const { trace } = require('@opentelemetry/api');

app.get('/api/process', async (req, res) => {
  const span = trace.getActiveSpan();
  
  // Add custom attributes
  span?.setAttribute('user.id', req.headers['user-id']);
  span?.setAttribute('operation.type', 'data-processing');
  
  try {
    // Your business logic
    const result = await processData();
    res.json(result);
  } catch (error) {
    // Exceptions are automatically tracked
    span?.recordException(error);
    res.status(500).json({ error: 'Processing failed' });
  }
});
```

## What Gets Tracked Automatically

✅ **HTTP Requests**: All incoming requests with duration, status, URL  
✅ **Dependencies**: Outgoing HTTP calls, database queries  
✅ **Exceptions**: Unhandled errors and exceptions  
✅ **Performance**: Response times and request counts  
✅ **Custom Logs**: `console.log()` statements are captured as traces

## Verify It Works

1. Start your application:
   ```bash
   npm start
   ```

2. Make some HTTP requests:
   ```bash
   curl http://localhost:3000/
   curl http://localhost:3000/api/users
   ```

3. Check Azure Portal:
   - Navigate to your Application Insights resource
   - Go to "Transaction search" or "Live Metrics"
   - You should see requests appearing within 1-2 minutes

## Complete package.json Example

```json
{
  "name": "express-azure-monitor-demo",
  "version": "1.0.0",
  "description": "Express app with Azure Monitor",
  "main": "index.js",
  "scripts": {
    "start": "node index.js",
    "dev": "nodemon index.js"
  },
  "dependencies": {
    "@azure/monitor-opentelemetry": "^1.0.0",
    "express": "^4.18.0",
    "dotenv": "^16.0.0"
  },
  "devDependencies": {
    "nodemon": "^3.0.0"
  }
}
```

## Troubleshooting

**No telemetry appearing?**
- Verify connection string is correct
- Ensure `useAzureMonitor()` is called BEFORE loading Express
- Check console for error messages
- Wait 2-3 minutes for initial data to appear

**Performance impact?**
- Azure Monitor has minimal overhead (<5% in most cases)
- Use sampling for high-traffic applications
- Disable in development if needed

## Next Steps

- Configure custom dimensions and metrics
- Set up alerts and dashboards in Azure Portal
- Enable profiler for performance analysis
- Add distributed tracing across microservices
