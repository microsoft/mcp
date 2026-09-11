// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

internal sealed record RecommendationMetadataActionData(
    string? ActionType,
    string? Caption,
    string? DocumentLink,
    string? BladeName);
