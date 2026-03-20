# Azure Monitor for Node.js

Azure Monitor OpenTelemetry for Node.js provides automatic instrumentation and telemetry collection for Node.js applications.

## Key Features

- **Automatic Instrumentation**: Captures HTTP requests, database calls, and external dependencies
- **Custom Telemetry**: Track custom events, metrics, and traces
- **Performance Monitoring**: Monitor response times, throughput, and failures
- **Dependency Tracking**: Understand outgoing calls to databases, APIs, and services
- **Distributed Tracing**: Follow requests across microservices

## Supported Frameworks

- Express.js
- Next.js (via instrumentation hook — requires webpack externals configuration)
- Fastify
- NestJS
- Koa
- Hapi
- And many more through OpenTelemetry auto-instrumentation

> **Note**: Next.js requires special setup due to its webpack bundling. See the Next.js Setup Guide(see in basic-setup-nextjs.md) for details on externalizing server-only packages.

## Installation

```bash
npm install @azure/monitor-opentelemetry
```

## Basic Setup

```javascript
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

// Enable Azure Monitor at startup
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  }
});

// Your application code follows...
const express = require('express');
const app = express();
```

## Configuration Options

The `useAzureMonitor()` function accepts configuration for:
- Connection string
- Sampling rate
- Custom resource attributes
- Instrumentation configuration
- Logging options

## What Gets Instrumented

### Automatically Captured
- HTTP/HTTPS requests and responses
- Database queries (MongoDB, MySQL, PostgreSQL, etc.)
- Redis operations
- External HTTP calls
- Exceptions and errors

### Custom Telemetry
```javascript
const { trace } = require('@opentelemetry/api');

// Get current span
const span = trace.getActiveSpan();
span?.setAttribute('custom.attribute', 'value');

// Log custom events
console.log('Custom event logged'); // Captured as trace
```

## Best Practices

1. **Initialize Early**: Call `useAzureMonitor()` before loading other modules
2. **Use Environment Variables**: Store connection string in `.env` file
3. **Enable Sampling**: For high-traffic apps, configure sampling to manage costs
4. **Add Context**: Use custom attributes to enrich telemetry
5. **Monitor Performance**: Set up alerts in Azure Monitor for key metrics

## Connection String

Get your connection string from Azure Portal:
1. Navigate to your Application Insights resource
2. Go to "Overview" section
3. Copy the "Connection String" value

Set it as an environment variable:
```bash
export APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=..."
```

Or in `.env` file:
```
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=...
```

## Links

- [Azure Monitor OpenTelemetry for Node.js Documentation](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable?tabs=nodejs)
- [OpenTelemetry for Node.js](https://opentelemetry.io/docs/instrumentation/js/)
