// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// A remediation action attached to an Advisor recommendation type
/// (from <c>properties.actions[]</c>).
/// </summary>
public sealed record RecommendationMetadataAction(
    string? ActionType,
    string? Caption,
    string? DocumentLink,
    string? BladeName);
