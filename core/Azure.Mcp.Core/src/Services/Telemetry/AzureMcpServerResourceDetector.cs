// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Configuration;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;

namespace Azure.Mcp.Core.Services.Telemetry;

/// <summary>
/// Adds the MCP server as an application to gather telemetry from.
/// </summary>
/// <param name="serverConfiguration">MCP server configuration</param>
public class AzureMcpServerResourceDetector(IOptions<AzureMcpServerConfiguration> serverConfiguration)
    : IResourceDetector
{
    private readonly AzureMcpServerConfiguration _serverConfiguration = serverConfiguration.Value;

    public Resource Detect()
    {
        return ResourceBuilder.CreateDefault()
            .AddService(_serverConfiguration.Prefix, serviceVersion: _serverConfiguration.Version)
            .AddTelemetrySdk()
            .Build();
    }
}
