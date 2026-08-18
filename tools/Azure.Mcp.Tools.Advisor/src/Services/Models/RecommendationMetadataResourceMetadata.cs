// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

/// <summary> Resource naming metadata associated with an Advisor recommendation type. </summary>
internal sealed record RecommendationMetadataResourceMetadata(
    string? Singular,
    string? Plural);
