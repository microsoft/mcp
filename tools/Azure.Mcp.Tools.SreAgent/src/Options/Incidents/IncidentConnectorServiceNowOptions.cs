// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public class IncidentConnectorServiceNowOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }
    public string? InstanceUrl { get; set; }
    public string? AuthType { get; set; }
    public string? TokenEnv { get; set; }
    public string? UsernameEnv { get; set; }
    public string? PasswordEnv { get; set; }
}
