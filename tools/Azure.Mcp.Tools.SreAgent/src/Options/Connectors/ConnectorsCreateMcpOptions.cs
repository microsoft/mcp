// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Connectors;

public class ConnectorsCreateMcpOptions : BaseSreAgentOptions
{
    public string Name { get; set; } = string.Empty;

    public string? Type { get; set; }

    public string? Command { get; set; }

    public string[]? Args { get; set; }

    public string? EnvsJson { get; set; }

    public string? Endpoint { get; set; }

    public string? AuthType { get; set; }

    public string? BearerTokenEnv { get; set; }

    public string? HeadersJson { get; set; }
}
