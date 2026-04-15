# Basic Azure Monitor Setup for NestJS

This guide shows how to add Azure Monitor OpenTelemetry to a NestJS application.

## Prerequisites

- Node.js 18.x or higher
- npm or yarn
- NestJS application
- Azure Application Insights resource

## Step 1: Install Package

```bash
npm install @azure/monitor-opentelemetry
```

## Step 2: Create Tracing File

Create a new file `src/tracing.ts` for OpenTelemetry initialization:

```typescript
import { useAzureMonitor } from '@azure/monitor-opentelemetry';

// Enable Azure Monitor integration
// This must be called before any other imports to ensure proper instrumentation
useAzureMonitor({
    azureMonitorExporterOptions: {
        connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
    }
});
```

## Step 3: Import Tracing in main.ts

Update your `src/main.ts` to import tracing **as the very first line**:

```typescript
import './tracing'; // MUST be the first import

import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);
  await app.listen(3000);
}
bootstrap();
```

> **Important**: The tracing import must be before all other imports to ensure proper instrumentation of HTTP modules and other dependencies.

## Step 4: Configure Connection String

Create a `.env` file in your project root:

```env
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...
PORT=3000
```

Install and configure `@nestjs/config` for environment variables:

```bash
npm install @nestjs/config
```

Update `app.module.ts`:

```typescript
import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';

@Module({
  imports: [
    ConfigModule.forRoot({
      isGlobal: true,
    }),
  ],
  // ... your other modules
})
export class AppModule {}
```

## Step 5: Add Custom Telemetry (Optional)

```typescript
import { Controller, Get, Param } from '@nestjs/common';
import { trace } from '@opentelemetry/api';

@Controller('users')
export class UsersController {
  @Get(':id')
  async findOne(@Param('id') id: string) {
    const span = trace.getActiveSpan();
    
    // Add custom attributes to the current span
    span?.setAttribute('user.id', id);
    span?.setAttribute('operation.type', 'user-lookup');
    
    try {
      const user = await this.userService.findById(id);
      return user;
    } catch (error) {
      // Exceptions are automatically tracked
      span?.recordException(error);
      throw error;
    }
  }
}
```

## What Gets Tracked Automatically

✅ **HTTP Requests**: All incoming requests to your NestJS controllers  
✅ **Dependencies**: Outgoing HTTP calls, database queries (TypeORM, Prisma, etc.)  
✅ **Exceptions**: Unhandled errors and NestJS exceptions  
✅ **Performance**: Response times, request counts, and latency  
✅ **Custom Logs**: Console statements are captured as traces

## Verify It Works

1. Start your application:
   ```bash
   npm run start:dev
   ```

2. Make some HTTP requests:
   ```bash
   curl http://localhost:3000/
   curl http://localhost:3000/users/1
   ```

3. Check Azure Portal:
   - Navigate to your Application Insights resource
   - Go to "Transaction search" or "Live Metrics"
   - You should see requests appearing within 1-2 minutes

## Complete package.json Example

```json
{
  "name": "nestjs-azure-monitor-demo",
  "version": "1.0.0",
  "scripts": {
    "build": "nest build",
    "start": "nest start",
    "start:dev": "nest start --watch",
    "start:prod": "node dist/main"
  },
  "dependencies": {
    "@azure/monitor-opentelemetry": "^1.0.0",
    "@nestjs/common": "^10.0.0",
    "@nestjs/config": "^3.0.0",
    "@nestjs/core": "^10.0.0",
    "@nestjs/platform-express": "^10.0.0",
    "reflect-metadata": "^0.1.13",
    "rxjs": "^7.8.0"
  }
}
```

## Troubleshooting

**No telemetry appearing?**
- Verify the tracing import is the FIRST line in `main.ts`
- Check that connection string is correct
- Ensure environment variables are loaded before `useAzureMonitor()` is called
- Wait 2-3 minutes for initial data to appear

**TypeScript compilation errors?**
- Ensure `@types/node` is installed
- Add `"esModuleInterop": true` to tsconfig.json if needed

**Performance impact?**
- Azure Monitor has minimal overhead (<5% in most cases)
- Use sampling for high-traffic applications

## Next Steps

- Configure custom dimensions and metrics
- Set up alerts and dashboards in Azure Portal
- Enable distributed tracing across microservices
- Add interceptors for custom span attributes
