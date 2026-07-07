const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

// Enable Azure Monitor integration
useAzureMonitor({
    azureMonitorExporterOptions: {
        connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
    }
});