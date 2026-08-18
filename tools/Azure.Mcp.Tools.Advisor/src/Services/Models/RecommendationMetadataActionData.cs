// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

/// <summary> Wire representation of an Advisor recommendation metadata action. </summary>
internal sealed record RecommendationMetadataActionData(
    string? ActionType,
    string? Caption,
    string? DocumentLink,
    string? BladeName);
