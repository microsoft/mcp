// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Optimization.Models;

/// <summary>
/// Alternative resize/SKU option carried on an Azure Advisor right-size recommendation in
/// <c>properties.extendedProperties.alternatives</c>.
/// </summary>
public sealed class AlternativeRecommendation
{
    public string ResourceId { get; set; } = string.Empty;
    public int ObservationWindowDays { get; set; }
    public int Option { get; set; }
    public string? RecommendationMessage { get; set; }
    public string? ProposedSku { get; set; }
    public string? ProposedSeries { get; set; }
    public string? ProposedProcessor { get; set; }
    public double? EstimatedMonthlySavings { get; set; }
    public string? SavingsCurrency { get; set; }
    public double? EstimatedCoresSavings { get; set; }
}
