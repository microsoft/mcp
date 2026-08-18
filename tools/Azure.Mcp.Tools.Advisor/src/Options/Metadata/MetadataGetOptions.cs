// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Metadata;

public sealed class MetadataGetOptions
{
    [Option(Description = "The stable Advisor recommendation type id (properties.recommendationTypeId) to look up in the " +
        "global recommendation metadata. This is the join key shared with individual recommendations. Required.")]
    public required string RecommendationTypeId { get; set; }

    [Option(Description = "Metadata locale to filter on (properties.language). Defaults to 'en'. Case-insensitive. " +
        "Supported values: en, cs, de, es, fr, hu, id, it, ja, ko, nl, pl, pt-br, pt-pt, ru, sv, tr, zh-hans, zh-hant. " +
        "Region-qualified tags such as 'en-US' are accepted and mapped to the base language ('en').",
        DefaultValue = "en")]
    public string Language { get; set; } = "en";
}
