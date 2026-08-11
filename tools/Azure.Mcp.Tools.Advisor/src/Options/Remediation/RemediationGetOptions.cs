// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Remediation;

public sealed class RemediationGetOptions
{
    [Option(Description = "The Advisor recommendation type id (GUID). Used as {recommendationTypeId} in the " +
        "Microsoft.Advisor/remediationTypes ARM path to fetch the remediation package. " +
        "Must be a 36-character GUID in the form xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx. Required.")]
    public required string RecommendationId { get; set; }

    [Option(Description = "Optional comma-separated subset of remediation artifact types to return. " +
        "Allowed values: cli, powershell, bicep, arm. When omitted, all approved artifacts are returned. " +
        "Unsupported values are rejected. Example: 'cli,bicep'.")]
    public string? ArtifactTypes { get; set; }
}
