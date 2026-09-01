// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Optimization.Services;

namespace Azure.Mcp.Tools.Optimization.Options.Recommendation;

public class RecommendationAlternativesOptions : ISubscriptionOption
{
    [Option(Description = "Full Azure ARM resource id of the VM or VM Scale Set, e.g. " +
        "'/subscriptions/<subId>/resourceGroups/<rg>/providers/Microsoft.Compute/virtualMachines/<name>'. " +
        "You can pass either the impacted resource id or the Advisor recommendation id (the " +
        "'/providers/Microsoft.Advisor/...' suffix is stripped automatically). Use the " +
        "'resourceId' or 'id' field from the 'list' tool output.")]
    public string? ResourceId { get; set; }

    [Option(Description = "Only include proposals whose new SKU matches any of these (comma/semicolon separated, e.g. 'Standard_D4s_v5, Standard_E4s_v5').")]
    public string? NewSkus { get; set; }

    [Option(Description = "Only include proposals whose new VM series matches any of these (comma/semicolon separated, e.g. 'Dsv5, Esv5').")]
    public string? NewVmSeries { get; set; }

    [Option(Description = "Only include proposals whose new processor type matches any of these (e.g. 'Intel, AMD').")]
    public string? NewProcessorTypes { get; set; }

    [Option(Description = "Exclude proposals whose new SKU matches any of these.")]
    public string? ExcludeSkus { get; set; }

    [Option(Description = "Exclude proposals whose new VM series matches any of these.")]
    public string? ExcludeVmSeries { get; set; }

    [Option(Description = "Exclude proposals whose new processor type matches any of these.")]
    public string? ExcludeProcessorTypes { get; set; }

    [Option(Description = OptimizationStrings.SubscriptionOptionDescription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
