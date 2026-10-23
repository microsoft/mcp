// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public sealed class IncidentConnectorServiceNowOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(Description = "ServiceNow instance URL.")]
    public required string InstanceUrl { get; set; }

    [Option(Description = SreAgentOptionDefinitions.AuthTypeDescription)]
    public required string AuthType { get; set; }

    [Option(Description = "Environment variable containing bearer token.")]
    public string? TokenEnv { get; set; }

    [Option(Description = "Environment variable containing username.")]
    public string? UsernameEnv { get; set; }

    [Option(Description = "Environment variable containing password.")]
    public string? PasswordEnv { get; set; }
}
