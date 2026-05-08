// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Architecture;

public class PlanOptions : GlobalOptions
{
    public string? Requirements { get; set; }
    public string? TriggerType { get; set; }
    public string? KustoConnector { get; set; }
}
