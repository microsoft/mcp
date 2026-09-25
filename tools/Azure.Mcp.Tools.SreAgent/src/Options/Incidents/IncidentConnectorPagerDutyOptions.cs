// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public sealed class IncidentConnectorPagerDutyOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(Description = "Environment variable containing the API key.")]
    public required string ApiKeyEnv { get; set; }

    [Option(Description = "PagerDuty subdomain.")]
    public string? Subdomain { get; set; }
}
