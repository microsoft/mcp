// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Support.Options.ProblemClassification;

public class ProblemClassificationListOptions
{
    [Option(Description = SupportOptionDescriptions.Service)]
    public required string Service { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
