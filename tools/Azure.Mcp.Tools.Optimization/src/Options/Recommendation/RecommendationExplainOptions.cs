// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Optimization.Services;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Optimization.Options.Recommendation;

public class RecommendationExplainOptions : ISubscriptionOption
{
    [Option(Description = "Full Azure ARM resource id of the VM or VM Scale Set, e.g. " +
        "'/subscriptions/<subId>/resourceGroups/<rg>/providers/Microsoft.Compute/virtualMachines/<name>'. " +
        "You can pass either the impacted resource id or the Advisor recommendation id (the " +
        "'/providers/Microsoft.Advisor/...' suffix is stripped automatically). Use the " +
        "'resourceId' or 'id' field from the 'list' tool output.")]
    public string? ResourceId { get; set; }

    [Option(Description = "Azure Advisor recommendation type ID to retrieve and explain.")]
    public string? RecommendationTypeId { get; set; }

    [Option(Description = "Optional target Azure VM SKU to compare against the current SKU, e.g. 'Standard_E2as_v5'. " +
        "If omitted, the tool automatically derives it from the top alternative resize recommendation for the resource, " +
        "so you do not need to call the 'alternatives' tool first. Only set this when the user explicitly names a SKU.")]
    public string? TargetSku { get; set; }

    [Option(Description = "Which utilization view(s) to return: 'Detail' (default, 7-day / 30-minute maximum), " +
        "'Trend' (7-day / 6-hour), or 'Both'. Only set 'Trend' or 'Both' when the user explicitly requests longer-term data.")]
    public string? View { get; set; }

    [Option(Description = OptimizationStrings.SubscriptionOptionDescription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
