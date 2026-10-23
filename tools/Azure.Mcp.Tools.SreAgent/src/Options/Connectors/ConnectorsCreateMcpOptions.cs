// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Connectors;

public sealed class ConnectorsCreateMcpOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(Description = "The MCP connector type: stdio or http.")]
    public required string Type { get; set; }

    [Option(Description = "The command for stdio MCP connectors.")]
    public string? Command { get; set; }

    [Option(Description = "Arguments for stdio MCP connectors.")]
    public string[]? Args { get; set; }

    [Option(Description = "JSON object of environment variables for stdio MCP connectors.")]
    public string? EnvsJson { get; set; }

    [Option(Description = "The HTTP MCP connector endpoint.")]
    public string? Endpoint { get; set; }

    [Option(Description = SreAgentOptionDefinitions.AuthTypeDescription)]
    public string? AuthType { get; set; }

    [Option(Description = "Environment variable containing the bearer token.")]
    public string? BearerTokenEnv { get; set; }

    [Option(Description = "JSON object of HTTP headers.")]
    public string? HeadersJson { get; set; }
}
