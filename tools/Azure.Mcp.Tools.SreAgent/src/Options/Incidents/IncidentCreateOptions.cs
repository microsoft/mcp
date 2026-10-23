// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public sealed class IncidentCreateOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.SeverityDescription)]
    public required string Severity { get; set; }

    [Option(Description = "Incident title.")]
    public required string Title { get; set; }

    [Option(Description = SreAgentOptionDefinitions.DescriptionDescription)]
    public required string Description { get; set; }

    [Option(Description = SreAgentOptionDefinitions.ServicesDescription)]
    public required string[] Services { get; set; }
}
