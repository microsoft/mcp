// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Connectors;

public sealed class ConnectorsCreateMcpOptions : BaseSreAgentOptions
{
    [Option(SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option("The MCP connector type: stdio or http.")]
    public required string Type { get; set; }

    [Option("The command for stdio MCP connectors.")]
    public string? Command { get; set; }

    [Option("The command for stdio MCP connectors.")]
    public string[]? Args { get; set; }

    [Option("JSON object of environment variables for stdio MCP connectors.")]
    public string? EnvsJson { get; set; }

    [Option("The HTTP MCP connector endpoint.")]
    public string? Endpoint { get; set; }

    [Option(SreAgentOptionDefinitions.AuthTypeDescription)]
    public string? AuthType { get; set; }

    [Option("Environment variable containing the bearer token.")]
    public string? BearerTokenEnv { get; set; }

    [Option("JSON object of HTTP headers.")]
    public string? HeadersJson { get; set; }
}
