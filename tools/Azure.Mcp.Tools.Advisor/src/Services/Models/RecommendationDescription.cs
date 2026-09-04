// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

internal sealed class RecommendationDescription
{
    /// <summary> The recommendation problem text. </summary>
    public string? Problem { get; set; }

    /// <summary> The recommended solution text. </summary>
    public string? Solution { get; set; }
}
