// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Metadata;

public sealed class RecommendationMetadataListOptions
{
    [Option(
        Description = "Language for localized recommendation metadata. Defaults to English ('en'). " +
            "Regional tags such as en-US fall back to their supported base language; catalog locales such as pt-BR, pt-PT, zh-Hans, and zh-Hant are preserved.",
        DefaultValue = "en")]
    public string Language { get; set; } = "en";

    [Option(Description = "Optional exact Azure resource type filter, such as 'microsoft.compute/virtualmachines'. Use it during brownfield onboarding to discover recommendation types applicable to that resource type. Matched case-insensitively.")]
    public string? ResourceType { get; set; }

    [Option(Description = "Optional recommendation impact filter. Allowed values are High, Medium, or Low. Matched case-insensitively; unfiltered results are ordered High, Medium, then Low.")]
    public string? Impact { get; set; }

    [Option(Description = "Optional exact Advisor category filter. Current categories include Cost, HighAvailability, Security, Performance, and OperationalExcellence; future Advisor categories are also accepted. Matched case-insensitively.")]
    public string? Category { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
