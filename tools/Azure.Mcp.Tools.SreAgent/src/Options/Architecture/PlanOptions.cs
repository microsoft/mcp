// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Architecture;

public sealed class PlanOptions
{
    [Option(Description = "Architecture requirements.")]
    public required string Requirements { get; set; }

    [Option(Description = "Trigger type, such as manual or scheduled.")]
    public string? TriggerType { get; set; }

    [Option(Description = "Kusto connector name.")]
    public string? KustoConnector { get; set; }
}
