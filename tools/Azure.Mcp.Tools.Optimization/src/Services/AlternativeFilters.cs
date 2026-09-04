// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Optimization.Models;

namespace Azure.Mcp.Tools.Optimization.Services;

/// <summary>
/// Parsed inclusion/exclusion filters for the alternative-recommendations query. Comma/semicolon
/// separated, with "and"/"or" connectors treated as separators.
/// </summary>
public sealed class AlternativeFilters
{
    public List<string> NewSkus { get; init; } = new();
    public List<string> NewVmSeries { get; init; } = new();
    public List<string> NewProcessorTypes { get; init; } = new();
    public List<string> ExcludeSkus { get; init; } = new();
    public List<string> ExcludeVmSeries { get; init; } = new();
    public List<string> ExcludeProcessorTypes { get; init; } = new();

    public bool HasAny =>
        NewSkus.Count > 0 || NewVmSeries.Count > 0 || NewProcessorTypes.Count > 0 ||
        ExcludeSkus.Count > 0 || ExcludeVmSeries.Count > 0 || ExcludeProcessorTypes.Count > 0;

    /// <summary>Splits a raw filter string into tokens, ignoring "and"/"or" connectors and empties.</summary>
    public static List<string> Parse(string? raw)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return result;
        }

        raw = raw.Replace(" and ", ",", StringComparison.OrdinalIgnoreCase)
                 .Replace(" or ", ",", StringComparison.OrdinalIgnoreCase)
                 .Replace(";", ",");

        foreach (var token in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var cleaned = token.Trim();
            if (cleaned.Length > 0)
            {
                result.Add(cleaned);
            }
        }

        return result;
    }

    /// <summary>
    /// Applies inclusion/exclusion filters in-memory and orders survivors by option. Inclusion
    /// filters keep only matching proposals; exclusion filters drop matching proposals.
    /// </summary>
    public IReadOnlyList<AlternativeRecommendation> Apply(IEnumerable<AlternativeRecommendation> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        IEnumerable<AlternativeRecommendation> query = source;

        if (NewSkus.Count > 0)
        {
            query = query.Where(r => Matches(NewSkus, r.ProposedSku));
        }

        if (NewVmSeries.Count > 0)
        {
            query = query.Where(r => Matches(NewVmSeries, r.ProposedSeries));
        }

        if (NewProcessorTypes.Count > 0)
        {
            query = query.Where(r => Matches(NewProcessorTypes, r.ProposedProcessor));
        }

        if (ExcludeSkus.Count > 0)
        {
            query = query.Where(r => !Matches(ExcludeSkus, r.ProposedSku));
        }

        if (ExcludeVmSeries.Count > 0)
        {
            query = query.Where(r => !Matches(ExcludeVmSeries, r.ProposedSeries));
        }

        if (ExcludeProcessorTypes.Count > 0)
        {
            query = query.Where(r => !Matches(ExcludeProcessorTypes, r.ProposedProcessor));
        }

        return query.OrderBy(r => r.Option).ToList();
    }

    private static bool Matches(List<string> values, string? candidate)
        => !string.IsNullOrWhiteSpace(candidate)
            && values.Any(v => string.Equals(v, candidate, StringComparison.OrdinalIgnoreCase));
}
