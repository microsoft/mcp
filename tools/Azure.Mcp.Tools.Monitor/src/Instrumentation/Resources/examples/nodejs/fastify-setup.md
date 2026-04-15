# Basic Azure Monitor Setup for Fastify

This guide shows how to add Azure Monitor OpenTelemetry to a Fastify application.

## Prerequisites

- Node.js 18.x or higher
- npm or yarn
- Fastify application
- Azure Application Insights resource

## Step 1: Install Package

```bash
npm install @azure/monitor-opentelemetry
```

## Step 2: Initialize at Startup

Update your main entry point (typically `index.js`, `server.js`, or `app.js`):

```javascript
// IMPORTANT: This must be the first line, before any other imports
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

// Enable Azure Monitor integration - must be called before other requires
useAzureMonitor({
    azureMonitorExporterOptions: {
        connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
    }
});

// Now load your application code
const fastify = require('fastify')({ logger: true });

// Register routes
fastify.get('/', async (request, reply) => {
    return { message: 'Hello World!' };
});

fastify.get('/api/users', async (request, reply) => {
    // This request will be automatically tracked
    return [
        { id: 1, name: 'Alice' },
        { id: 2, name: 'Bob' }
    ];
});

// Start server
const start = async () => {
    try {
        await fastify.listen({ port: process.env.PORT || 3000 });
    } catch (err) {
        fastify.log.error(err);
        process.exit(1);
    }
};

start();
```

> **Important**: `useAzureMonitor()` must be called before requiring Fastify or any other modules to ensure proper instrumentation.

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

fastify.get('/api/process/:id', async (request, reply) => {
    const span = trace.getActiveSpan();
    
    // Add custom attributes to the current span
    span?.setAttribute('process.id', request.params.id);
    span?.setAttribute('operation.type', 'data-processing');
    
    try {
        // Your business logic
        const result = await processData(request.params.id);
        return result;
    } catch (error) {
        // Exceptions are automatically tracked
        span?.recordException(error);
        reply.status(500).send({ error: 'Processing failed' });
    }
});
```

## Using with TypeScript

For TypeScript projects, create your entry file:

```typescript
// IMPORTANT: This must be the first import
import { useAzureMonitor } from '@azure/monitor-opentelemetry';

useAzureMonitor({
    azureMonitorExporterOptions: {
        connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
    }
});

import Fastify from 'fastify';

const fastify = Fastify({ logger: true });

fastify.get('/', async () => {
    return { message: 'Hello World!' };
});

fastify.listen({ port: 3000 });
```

## What Gets Tracked Automatically

✅ **HTTP Requests**: All incoming requests with duration, status, URL  
✅ **Dependencies**: Outgoing HTTP calls, database queries  
✅ **Exceptions**: Unhandled errors and exceptions  
✅ **Performance**: Response times and request counts  
✅ **Custom Logs**: Fastify logger output is captured as traces

## Using with Fastify Plugins

Azure Monitor works seamlessly with Fastify plugins:

```javascript
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');
useAzureMonitor({
    azureMonitorExporterOptions: {
        connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
    }
});

const fastify = require('fastify')({ logger: true });

// Register plugins - they will be automatically instrumented
fastify.register(require('@fastify/postgres'), {
    connectionString: process.env.DATABASE_URL
});

fastify.register(require('@fastify/redis'), {
    host: process.env.REDIS_HOST
});

// Routes using the plugins
fastify.get('/users', async (request, reply) => {
    const { rows } = await fastify.pg.query('SELECT * FROM users');
    return rows;
});
```

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
  "name": "fastify-azure-monitor-demo",
  "version": "1.0.0",
  "description": "Fastify app with Azure Monitor",
  "main": "index.js",
  "scripts": {
    "start": "node index.js",
    "dev": "nodemon index.js"
  },
  "dependencies": {
    "@azure/monitor-opentelemetry": "^1.0.0",
    "fastify": "^4.24.0",
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
- Ensure `useAzureMonitor()` is called BEFORE requiring Fastify
- Check console for error messages
- Wait 2-3 minutes for initial data to appear

**Fastify logger not working with telemetry?**
- Both work independently; Fastify logs go to stdout, telemetry goes to Azure
- Use `@opentelemetry/api` for custom spans within telemetry

**Performance impact?**
- Azure Monitor has minimal overhead (<5% in most cases)
- Use sampling for high-traffic applications
- Disable in development if needed

## Next Steps

- Configure custom dimensions and metrics
- Set up alerts and dashboards in Azure Portal
- Enable profiler for performance analysis
- Add distributed tracing across microservices
