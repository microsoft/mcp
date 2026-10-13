// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

/// <summary>
/// Selects which published prioritization signals (<c>properties.signalValues</c> on the recommendation-type metadata)
/// are returned with a prioritized summary group.
/// </summary>
/// <remarks>
/// <para>
/// This class only filters. An emitted value is always the published string, unchanged, with its published casing, so
/// the response can be audited against the metadata directly. Keys and values are matched case-insensitively because
/// published casing is inconsistent, for example <c>high</c>, <c>Networking</c>, and <c>nonQcs</c>.
/// </para>
/// <para>
/// A value is omitted when it carries no customer-facing meaning, so that a caller never describes something as
/// unknown, unavailable, or "not critical":
/// </para>
/// <list type="table">
/// <listheader><term>Signal</term><description>Emitted when</description></listheader>
/// <item><term>any key</term><description>Never for empty, Unknown, costUnavailable, notAvailable, or none.
/// costUnavailable means cost impact was not determined; it never means free.</description></item>
/// <item><term>recommendationImpact</term><description>Never. It is a high/other bucket of the metadata impact,
/// which is returned instead.</description></item>
/// <item><term>recommendationSubcategory</term><description>Only for the published retirement bands retirementPastDue,
/// retirementInDays&lt;N&gt; (N digits), retirementWithin30Days, and retirementWithin180Days, returned as
/// retirementWindow. Other values equal the metadata subcategory or are the scoring bucket "other".</description></item>
/// <item><term>resourceTypeCriticality</term><description>Any value except Other.</description></item>
/// <item><term>criticalService</term><description>Only qcs (a critical service); nonQcs is never surfaced.</description></item>
/// <item><term>effort</term><description>Any remaining value (Low, Medium, or High today).</description></item>
/// <item><term>actionability</term><description>Only OneClickOrFullyExecutable; Guidance is not surfaced.</description></item>
/// <item><term>costImplication</term><description>Any remaining value (noCostImpact, lowCost, or otherCost today).</description></item>
/// <item><term>unlisted key</term><description>Ignored. Exposing a newly published signal is a deliberate change: add a
/// property to RecommendationPrioritizationSignals and a case here.</description></item>
/// </list>
/// </remarks>
internal static partial class PrioritizationSignalFilter
{
    // Signal keys as published on recommendation-type metadata.
    internal const string RecommendationImpactKey = "recommendationImpact";
    internal const string RecommendationSubcategoryKey = "recommendationSubcategory";
    internal const string ResourceTypeCriticalityKey = "resourceTypeCriticality";
    internal const string CriticalServiceKey = "criticalService";
    internal const string EffortKey = "effort";
    internal const string ActionabilityKey = "actionability";
    internal const string CostImplicationKey = "costImplication";

    private const string OtherResourceTypeCriticality = "Other";
    private const string CriticalServiceValue = "qcs";
    private const string FullyExecutableActionability = "OneClickOrFullyExecutable";

    // Values that mean "not determined" for every signal.
    private static readonly HashSet<string> s_absentValues = new(StringComparer.OrdinalIgnoreCase)
    {
        string.Empty,
        "Unknown",
        "costUnavailable",
        "notAvailable",
        "none",
    };

    /// <summary>Returns the emitted signals; every property is unset when <paramref name="signalValues"/> is not an object.</summary>
    internal static RecommendationPrioritizationSignals Filter(JsonElement signalValues)
    {
        if (signalValues.ValueKind != JsonValueKind.Object)
        {
            return new();
        }

        string? retirementWindow = null;
        string? resourceTypeCriticality = null;
        string? criticalService = null;
        string? effort = null;
        string? actionability = null;
        string? costImplication = null;

        // When a key is published more than once with different casing, the first meaningful value is kept.
        // Keys not handled below are ignored on purpose; see the remarks on this class.
        foreach (var signal in signalValues.EnumerateObject())
        {
            if (IsAbsent(signal.Value))
            {
                continue;
            }

            var text = signal.Value.ValueKind == JsonValueKind.String ? signal.Value.GetString() : null;
            switch (signal.Name)
            {
                case var key when Is(key, RecommendationImpactKey):
                    break;
                case var key when Is(key, RecommendationSubcategoryKey):
                    if (text is not null && RetirementBand().IsMatch(text))
                    {
                        retirementWindow ??= text;
                    }

                    break;
                case var key when Is(key, ResourceTypeCriticalityKey):
                    if (text is not null && !Is(text, OtherResourceTypeCriticality))
                    {
                        resourceTypeCriticality ??= text;
                    }

                    break;
                case var key when Is(key, CriticalServiceKey):
                    if (text is not null && Is(text, CriticalServiceValue))
                    {
                        criticalService ??= text;
                    }

                    break;
                case var key when Is(key, EffortKey):
                    effort ??= text;
                    break;
                case var key when Is(key, ActionabilityKey):
                    if (text is not null && Is(text, FullyExecutableActionability))
                    {
                        actionability ??= text;
                    }

                    break;
                case var key when Is(key, CostImplicationKey):
                    costImplication ??= text;
                    break;
            }
        }

        return new()
        {
            RetirementWindow = retirementWindow,
            ResourceTypeCriticality = resourceTypeCriticality,
            CriticalService = criticalService,
            Effort = effort,
            Actionability = actionability,
            CostImplication = costImplication,
        };
    }

    /// <summary>True for null, undefined, and the shared not-determined strings.</summary>
    internal static bool IsAbsent(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => true,
        JsonValueKind.String => IsAbsent(value.GetString()),
        _ => false,
    };

    internal static bool IsAbsent(string? value) =>
        value is null || s_absentValues.Contains(value.Trim());

    // The exact retirement bands the publisher emits; any other value starting with "retirement" is not surfaced.
    [GeneratedRegex(@"^retirement(PastDue|InDays[0-9]+|Within30Days|Within180Days)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RetirementBand();

    private static bool Is(string value, string expected) =>
        string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
}
