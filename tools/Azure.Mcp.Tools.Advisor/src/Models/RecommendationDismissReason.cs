// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

[JsonConverter(typeof(JsonStringEnumConverter<RecommendationDismissReason>))]
public enum RecommendationDismissReason
{
    ExcessiveCostInvestmentRequired,
    ImplementationStepsAreUnclear,
    IncompatibleWithTheCurrentConfiguration,
    RiskIsAcceptable,
    TooComplexOrImpracticalToImplement,
    AnAlternativeSolutionIsAlreadyInPlace,
    Other,
}
