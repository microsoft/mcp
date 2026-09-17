// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text;
using Azure.Mcp.Tools.Optimization.Models;

namespace Azure.Mcp.Tools.Optimization.Services;

/// <summary>Renders the alternative-recommendations markdown.</summary>
public static class AlternativeMarkdownBuilder
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string BuildNoData(string resourceId, AlternativeFilters filters)
    {
        var summary = BuildFilterSummary(filters);
        var suffix = string.IsNullOrEmpty(summary)
            ? OptimizationStrings.AltNoRecommendationsPeriod
            : string.Format(Inv, OptimizationStrings.AltNoRecommendationsForFiltersSuffix, summary);

        var sb = new StringBuilder();
        sb.AppendLine(OptimizationStrings.AltHeader);
        sb.AppendLine(string.Format(Inv, OptimizationStrings.AltResourceIdLabel, resourceId));
        sb.AppendLine();
        sb.AppendLine(OptimizationStrings.AltNoRecommendationsFoundMessage + suffix);
        sb.AppendLine();
        sb.AppendLine(OptimizationStrings.AltPossibleReasonsHeader);
        sb.AppendLine(OptimizationStrings.AltPossibleReasonOne);
        sb.AppendLine(OptimizationStrings.AltPossibleReasonTwo);
        sb.AppendLine(OptimizationStrings.AltPossibleReasonThree);
        sb.AppendLine();
        sb.AppendLine(OptimizationStrings.AltTryAdjustFiltersMessage);
        return sb.ToString();
    }

    public static string Build(string resourceId, IReadOnlyList<AlternativeRecommendation> recs, AlternativeFilters filters)
    {
        var sb = new StringBuilder();
        var summary = BuildFilterSummary(filters);

        sb.AppendLine(OptimizationStrings.AltHeader);
        sb.AppendLine(string.Format(Inv, OptimizationStrings.AltResourceIdLabel, resourceId));
        if (!string.IsNullOrEmpty(summary))
        {
            sb.AppendLine(string.Format(Inv, OptimizationStrings.AltAppliedProposedFiltersLabel, summary));
        }

        sb.AppendLine(string.Format(Inv, OptimizationStrings.AltReturnedLabel, recs.Count));
        sb.AppendLine(string.Format(Inv, OptimizationStrings.AltObservationWindowLabel, recs[0].ObservationWindowDays));
        sb.AppendLine();

        sb.AppendLine(OptimizationStrings.AltTableHeader);
        foreach (var rec in recs)
        {
            var currency = string.IsNullOrWhiteSpace(rec.SavingsCurrency)
                ? OptimizationStrings.NotAvailableValue
                : rec.SavingsCurrency;
            sb.AppendLine(string.Format(
                Inv,
                OptimizationStrings.AltRow,
                rec.Option,
                rec.RecommendationMessage ?? OptimizationStrings.NotAvailableValue,
                rec.ProposedSku ?? OptimizationStrings.NotAvailableValue,
                rec.ProposedSeries ?? OptimizationStrings.NotAvailableValue,
                rec.ProposedProcessor ?? OptimizationStrings.NotAvailableValue,
                rec.EstimatedMonthlySavings ?? 0,
                currency,
                rec.EstimatedCoresSavings?.ToString("F0", Inv) ?? OptimizationStrings.NotAvailableValue));
        }

        sb.AppendLine();
        sb.AppendLine(string.Format(Inv, OptimizationStrings.AltSummary, recs[0].ObservationWindowDays));
        return sb.ToString();
    }

    private static string BuildFilterSummary(AlternativeFilters f)
    {
        var parts = new List<string>();
        AddIf(parts, f.NewSkus, OptimizationStrings.FilterSkusLabel);
        AddIf(parts, f.NewVmSeries, OptimizationStrings.FilterSeriesLabel);
        AddIf(parts, f.NewProcessorTypes, OptimizationStrings.FilterProcessorsLabel);
        AddIf(parts, f.ExcludeSkus, OptimizationStrings.FilterExcludeSkusLabel);
        AddIf(parts, f.ExcludeVmSeries, OptimizationStrings.FilterExcludeSeriesLabel);
        AddIf(parts, f.ExcludeProcessorTypes, OptimizationStrings.FilterExcludeProcessorsLabel);
        return string.Join(OptimizationStrings.FilterJoinSeparator, parts);
    }

    private static void AddIf(List<string> parts, List<string> values, string label)
    {
        if (values.Count > 0)
        {
            parts.Add(string.Format(Inv, label, string.Join("/", values)));
        }
    }
}
