// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Drills;

public sealed class DrillEndOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the running resilience drill to end.")]
    public required string Drill { get; set; }

    [Option(Description = "The outcome attested when ending the drill. Allowed values: Success, Failed.")]
    public required string Attestation { get; set; }

    [Option(Description = "Notes explaining the attested drill outcome.")]
    public required string AttestationNotes { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
