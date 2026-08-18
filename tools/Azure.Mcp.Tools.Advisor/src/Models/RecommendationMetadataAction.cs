// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// An action associated with an Advisor recommendation type.
/// </summary>
public sealed record RecommendationMetadataAction(
    string? ActionType,
    string? Caption,
    string? DocumentLink,
    string? BladeName);
