// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public class IncidentCreateOptions : BaseSreAgentOptions
{
    public string? Severity { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string[]? Services { get; set; }
}
