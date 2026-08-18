// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

/// <summary> Metadata identifying the resource affected by an Advisor recommendation. </summary>
internal sealed class RecommendationResourceMetadata
{
    /// <summary> Resource ID pertaining to the affected resource. </summary>
    public string? ResourceId { get; set; }
}
