// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public class IncidentConnectorPagerDutyOptions : BaseSreAgentOptions
{
    public string? Name { get; set; }
    public string? ApiKeyEnv { get; set; }
    public string? Subdomain { get; set; }
}
