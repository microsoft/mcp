# Basic Azure Monitor Setup for Next.js

This guide shows how to add Azure Monitor OpenTelemetry to a Next.js application using the instrumentation hook.

## Prerequisites

- Node.js 18.x or higher
- npm or yarn
- Next.js 13.4+ application (App Router recommended)
- Azure Application Insights resource

## Step 1: Install Package

```bash
npm install @azure/monitor-opentelemetry
```

## Step 2: Create Instrumentation File

Create a new file `instrumentation.js` (or `instrumentation.ts`) in your project root:

```javascript
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

export function register() {
    // Only initialize on server-side
    if (process.env.NEXT_RUNTIME === 'nodejs') {
        useAzureMonitor({
            azureMonitorExporterOptions: {
                connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
            }
        });
    }
}
```

> **Note**: The `register()` function is called once when the Next.js server starts. The `NEXT_RUNTIME` check ensures telemetry only initializes on the server, not in Edge runtime.

## Step 3: Enable Instrumentation Hook and Externalize Packages

Update your `next.config.js` to enable the instrumentation hook **and** externalize server-only packages. Without this, Next.js's webpack bundler will try to resolve Node.js built-in modules (`fs`, `stream`, etc.) used by `@grpc/grpc-js` and other OpenTelemetry dependencies, causing `Module not found` errors.

```javascript
/** @type {import('next').NextConfig} */
const nextConfig = {
    experimental: {
        instrumentationHook: true,
        serverComponentsExternalPackages: [
            '@azure/monitor-opentelemetry',
            '@opentelemetry/sdk-node',
            '@opentelemetry/api',
            '@opentelemetry/exporter-logs-otlp-grpc',
            '@opentelemetry/otlp-grpc-exporter-base',
            '@grpc/grpc-js',
            '@grpc/proto-loader',
            '@opentelemetry/instrumentation',
        ],
    },
    webpack: (config, { isServer }) => {
        if (isServer) {
            config.externals = config.externals || [];
            config.externals.push({
                '@azure/monitor-opentelemetry': 'commonjs @azure/monitor-opentelemetry',
                '@opentelemetry/sdk-node': 'commonjs @opentelemetry/sdk-node',
                '@opentelemetry/instrumentation': 'commonjs @opentelemetry/instrumentation',
                '@opentelemetry/api': 'commonjs @opentelemetry/api',
                '@grpc/grpc-js': 'commonjs @grpc/grpc-js',
            });
        }
        return config;
    },
};

module.exports = nextConfig;
```

> **Important**: Both `serverComponentsExternalPackages` and `webpack.externals` are required. The `serverComponentsExternalPackages` tells Next.js to skip bundling these packages for Server Components, while the `webpack.externals` configuration ensures the instrumentation hook itself (which runs outside the Server Components context) also resolves these packages from `node_modules` at runtime instead of bundling them.

> **Note (Next.js-specific)**: If you use logging libraries like **Bunyan** or **Winston** in a Next.js app, you must also add them to both `serverComponentsExternalPackages` and `webpack.externals`. Bunyan in particular has optional native dependencies (`dtrace-provider`, `source-map-support`) that webpack cannot resolve. This is not an issue in standard Node.js apps (Express, Fastify, etc.) where these libraries work without special configuration. See the [Using Logging Libraries](#using-logging-libraries-bunyan-winston) section below.

## Step 4: Configure Connection String

Create a `.env.local` file in your project root:

```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
```

> **Important**: Use `.env.local` for local development. For production, set this in your hosting platform's environment variables.

## Step 5: Add Custom Telemetry (Optional)

In your API routes or Server Components:

```typescript
// app/api/users/route.ts
import { trace } from '@opentelemetry/api';
import { NextResponse } from 'next/server';

export async function GET(request: Request) {
    const span = trace.getActiveSpan();
    
    // Add custom attributes
    span?.setAttribute('api.endpoint', '/api/users');
    span?.setAttribute('operation.type', 'list-users');
    
    try {
        const users = await fetchUsers();
        return NextResponse.json(users);
    } catch (error) {
        span?.recordException(error as Error);
        return NextResponse.json({ error: 'Failed to fetch users' }, { status: 500 });
    }
}
```

In Server Components:

```typescript
// app/users/page.tsx
import { trace } from '@opentelemetry/api';

export default async function UsersPage() {
    const span = trace.getActiveSpan();
    span?.setAttribute('page.name', 'users');
    
    const users = await fetchUsers();
    
    return (
        <div>
            {users.map(user => <UserCard key={user.id} user={user} />)}
        </div>
    );
}
```

## Using Logging Libraries (Bunyan, Winston)

> **Next.js-specific**: The configuration in this section is only required because Next.js bundles server code with webpack. In standard Node.js applications (Express, Fastify, NestJS, etc.), Bunyan and Winston work out of the box — just install them, enable the instrumentation option, and use them. No externalization or runtime exports are needed. See the Bunyan Setup Guide(see in basic-setup-bunyan-nodejs.md) for the standard approach.

In Next.js, you need extra configuration because webpack tries to bundle these libraries and their native/optional dependencies, which causes `Module not found` errors.

### Step 1: Install the logging library

```bash
npm install bunyan
```

### Step 2: Add to externals in next.config.js

Add the logging library to **both** `serverComponentsExternalPackages` and `webpack.externals`:

```javascript
experimental: {
    serverComponentsExternalPackages: [
        // ... existing packages ...
        'bunyan',  // Add this
    ],
},
webpack: (config, { isServer }) => {
    if (isServer) {
        config.externals = config.externals || [];
        config.externals.push({
            // ... existing externals ...
            bunyan: 'commonjs bunyan',  // Add this
        });
    }
    return config;
},
```

> **Why is this needed in Next.js?** Bunyan has optional native dependencies (`dtrace-provider`, `source-map-support`) that webpack cannot resolve. Next.js bundles server code with webpack, so these modules must be externalized. In standard Node.js apps (Express, Fastify, etc.), this is not an issue because there is no webpack bundling step.

### Step 3: Enable bunyan instrumentation

Update your `instrumentation.js` to enable bunyan log collection:

```javascript
const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

export function register() {
    if (process.env.NEXT_RUNTIME === 'nodejs') {
        useAzureMonitor({
            azureMonitorExporterOptions: {
                connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
            },
            instrumentationOptions: {
                bunyan: { enabled: true }
            }
        });
    }
}
```

### Step 4: Use in API routes (Next.js-specific)

In Next.js, mark API routes that use bunyan with `export const runtime = 'nodejs'` to ensure they run on the Node.js runtime (as opposed to Next.js's Edge runtime). This is a Next.js concept and does not apply to standard Node.js apps:

```javascript
import { NextResponse } from 'next/server';
import bunyan from 'bunyan';

export const runtime = 'nodejs';

const logger = bunyan.createLogger({ name: 'my-nextjs-app' });

export async function GET(request) {
    logger.info({ action: 'fetch-data' }, 'Handling request');
    logger.warn({ reason: 'slow-query' }, 'Query took longer than expected');
    logger.error({ err: new Error('Something failed') }, 'Operation failed');
    
    return NextResponse.json({ success: true });
}
```

Bunyan logs will be collected by the OpenTelemetry bunyan instrumentation and exported to Application Insights as traces with proper severity mapping.

> **Winston**: The same Next.js-specific pattern applies — add `winston` to webpack externals and enable `winston: { enabled: true }` in `instrumentationOptions`. In standard Node.js apps, just enable the instrumentation option — no externalization needed.

## What Gets Tracked Automatically

✅ **Server-Side Requests**: All API routes and Server Component renders  
✅ **Dependencies**: Outgoing HTTP calls via `fetch()`  
✅ **Exceptions**: Unhandled errors in API routes and Server Components  
✅ **Performance**: Response times and request counts  
✅ **Database Calls**: Queries through supported ORMs (Prisma, etc.)

> **Note**: Client-side rendering and navigation are NOT tracked by server-side telemetry. Use Application Insights JavaScript SDK for client-side monitoring.

## Verify It Works

1. Start your development server:
   ```bash
   npm run dev
   ```

2. Make some requests:
   ```bash
   curl http://localhost:3000/api/users
   ```
   Or navigate to pages in your browser.

3. Check Azure Portal:
   - Navigate to your Application Insights resource
   - Go to "Transaction search" or "Live Metrics"
   - You should see requests appearing within 1-2 minutes

## Complete package.json Example

```json
{
  "name": "nextjs-azure-monitor-demo",
  "version": "1.0.0",
  "scripts": {
    "dev": "next dev",
    "build": "next build",
    "start": "next start"
  },
  "dependencies": {
    "@azure/monitor-opentelemetry": "^1.0.0",
    "next": "^14.0.0",
    "react": "^18.2.0",
    "react-dom": "^18.2.0"
  }
}
```

## Project Structure

```
my-nextjs-app/
├── app/
│   ├── api/
│   │   └── users/
│   │       └── route.ts
│   ├── layout.tsx
│   └── page.tsx
├── instrumentation.js    ← Azure Monitor setup
├── next.config.js        ← Enable instrumentationHook
├── .env.local            ← Connection string
└── package.json
```

## Troubleshooting

**Module not found: Can't resolve 'fs' / 'stream' / 'net' / 'tls'?**
- This is the most common issue. Next.js tries to bundle server-only Node.js modules used by `@grpc/grpc-js` and OpenTelemetry packages.
- **Fix**: Add `serverComponentsExternalPackages` AND `webpack.externals` to `next.config.js` as shown in Step 3.
- Both configurations are required — `serverComponentsExternalPackages` alone is not sufficient for the instrumentation hook.
- Ensure `@opentelemetry/instrumentation`, `@opentelemetry/sdk-node`, and `@grpc/grpc-js` are all included in `webpack.externals`.

**Module not found: Can't resolve 'source-map-support' or './src/build' (dtrace-provider)?**
- This occurs when using Bunyan in Next.js. Bunyan has optional native dependencies that webpack cannot resolve.
- **Fix**: Add `bunyan` to both `serverComponentsExternalPackages` and `webpack.externals` as shown in the [Using Logging Libraries](#using-logging-libraries-bunyan-winston) section.

**Module not found: Can't resolve '@azure/functions-core'?**
- This is a harmless warning. `@azure/functions-core` is an optional dependency used only in Azure Functions environments.
- It can be safely ignored for standalone Next.js deployments.

**No telemetry appearing?**
- Verify `instrumentationHook: true` is in `next.config.js`
- Check that `instrumentation.js` is in the project root (not in `/app` or `/src`)
- Ensure connection string is correct
- Wait 2-3 minutes for initial data to appear

**Edge Runtime not supported?**
- Azure Monitor OpenTelemetry only works with Node.js runtime
- Ensure your API routes are not using Edge runtime
- The `NEXT_RUNTIME` check prevents errors in Edge environments

**Development vs Production?**
- Telemetry works in both `npm run dev` and `npm run start`
- In development, you may see more verbose logging

## Next Steps

- Add client-side monitoring with Application Insights JavaScript SDK
- Configure custom dimensions and metrics
- Set up alerts and dashboards in Azure Portal
- Enable distributed tracing across microservices
