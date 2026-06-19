// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Connectors;

public sealed class ConnectorsCreateMcpOptions : ISubscriptionOption
{
    [Option(SreAgentOptionDefinitions.AgentDescription)]
    public required string Agent { get; set; }

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

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
