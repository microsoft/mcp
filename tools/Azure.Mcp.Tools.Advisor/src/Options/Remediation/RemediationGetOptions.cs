// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Remediation;

public sealed class RemediationGetOptions
{
    [Option(Description = "The Advisor recommendation type id (GUID) whose remediation package should be fetched. " +
        "Must be a 36-character GUID in the form xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx. Required.")]
    public required string RecommendationTypeId { get; set; }
}
