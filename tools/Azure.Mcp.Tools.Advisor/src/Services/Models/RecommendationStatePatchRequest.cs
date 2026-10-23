// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

internal sealed record RecommendationStatePatchRequest(
    [property: JsonPropertyName("properties")] RecommendationStatePatchProperties Properties);
