# Azure MCP Server Telemetry Guide

> **⚠️ Note:** This guide is a work in progress and will be expanded in future revisions with more detailed information about telemetry collection, configuration options, and data handling practices.

## Overview

The Azure MCP Server includes built-in telemetry capabilities to help monitor server performance, diagnose issues, and improve the overall quality of the service. Telemetry is collected using OpenTelemetry and can be exported to Azure Application Insights.

## Telemetry Architecture

### Version 2.0.0-beta.5 and Earlier

In version 2.0.0-beta.5, telemetry configuration is common for both:
- **Microsoft-owned Application Insights instance**: Collects aggregated usage data to improve the product
- **User-provided Application Insights instance**: Allows users to monitor their own deployments

Both telemetry streams share the same configuration pipeline and collect the same set of metrics, traces, and logs.

### Version 2.0.0-beta.6 and Later

Starting with version 2.0.0-beta.6, additional telemetry capabilities have been added specifically for user-provided Application Insights instances:

**New in 2.0.0-beta.6:**
- **HTTP Trace Instrumentation**: Captures detailed traces of incoming and outgoing HTTP requests when running in HTTP transport mode
- **ASP.NET Core Instrumentation**: Monitors incoming HTTP requests to the MCP server
- **HttpClient Instrumentation**: Tracks outgoing HTTP calls to Azure services and external APIs
- **Smart Filtering**: Prevents telemetry loops and duplicate spans from Azure SDK operations

This enhanced telemetry is only enabled for self-hosted scenarios running in HTTP mode and requires explicit configuration via the `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable.

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `AZURE_MCP_COLLECT_TELEMETRY` | Enable or disable all telemetry collection | `true` |
| `AZURE_MCP_COLLECT_TELEMETRY_MICROSOFT` | Enable or disable Microsoft-owned telemetry | `true` |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Connection string for user-provided Application Insights (required for HTTP trace telemetry in 2.0.0-beta.6+) | Not set |
| `AZURE_MCP_ENABLE_OTLP_EXPORTER` | Enable OpenTelemetry Protocol (OTLP) exporter | `false` |

### Disabling Telemetry

To completely disable telemetry collection:

```bash
export AZURE_MCP_COLLECT_TELEMETRY=false
```

To disable only Microsoft telemetry while keeping your own:

```bash
export AZURE_MCP_COLLECT_TELEMETRY_MICROSOFT=false
```

### Enabling HTTP Trace Telemetry (2.0.0-beta.6+)

For self-hosted scenarios running in HTTP mode, configure your Application Insights connection string:

```bash
export APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=...;IngestionEndpoint=...;LiveEndpoint=..."
```

**Requirements:**
- Server must be running in HTTP transport mode (`--transport http`)
- `AZURE_MCP_COLLECT_TELEMETRY` must not be set to `false`
- Valid `APPLICATIONINSIGHTS_CONNECTION_STRING` must be provided
- Only available in Release builds

## What Data is Collected

### Common Telemetry (All Versions)

- Command execution metrics
- Tool invocation traces
- Performance metrics (duration, latency)
- Server startup and configuration metadata

### HTTP Trace Telemetry (2.0.0-beta.6+)

**For user-provided Application Insights only:**
- Incoming HTTP request details (method, path, status code, duration)
- Outgoing HTTP request traces to Azure services
- Request dependencies and correlation
- Distributed tracing across service boundaries

**Filtered out to prevent issues:**
- Application Insights ingestion requests (prevents telemetry loops)
- Duplicate spans from Azure SDK operations (SDK already creates spans)

## Transport Modes

### STDIO Mode
- Standard telemetry only (metrics, traces, logs)
- No HTTP instrumentation

### HTTP Mode
- All standard telemetry
- **Additional HTTP trace instrumentation** (2.0.0-beta.6+) when configured with `APPLICATIONINSIGHTS_CONNECTION_STRING`
- Suitable for remote/self-hosted deployments

## Privacy and Data Handling

> **Note:** Detailed information about data handling practices, retention policies, and privacy considerations will be added in a future revision of this guide.

### General Principles

- Telemetry does not capture sensitive data such as passwords, keys, or connection strings
- Request/response payloads are not logged in telemetry
- Personal identifiable information (PII) is not intentionally collected
- Users can disable telemetry at any time using environment variables

## Troubleshooting

### Telemetry Not Working

1. **Check environment variables**: Ensure `AZURE_MCP_COLLECT_TELEMETRY` is not set to `false`
2. **Verify connection string**: For HTTP traces, ensure `APPLICATIONINSIGHTS_CONNECTION_STRING` is valid
3. **Confirm transport mode**: HTTP trace telemetry only works in HTTP mode
4. **Check build configuration**: HTTP trace telemetry is only enabled in Release builds

### Viewing Telemetry Data

To view telemetry in Application Insights:

1. Navigate to your Application Insights resource in Azure Portal
2. Use **Application Map** to see distributed traces
3. Use **Performance** blade to analyze request durations

## Future Documentation

This guide will be expanded to include:

- Detailed telemetry schema and data models
- Custom telemetry integration examples
- Advanced filtering and sampling configuration
- Performance impact and optimization guidelines
- Complete list of collected telemetry attributes
- Data retention and compliance information
- Telemetry dashboards and query examples

## Related Documentation

- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)
- [Azure Monitor OpenTelemetry Exporter](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable)
- [Application Insights Overview](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview)

## Support

For questions or issues related to telemetry:
- [Report an issue](https://github.com/microsoft/mcp/issues)
- [Contributing guidelines](../../CONTRIBUTING.md)
