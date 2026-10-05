// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.InfraIq.Options.VmSku;

public sealed class VmSkuRecommendOptions : ISubscriptionOption
{
    [Option(Description = "Azure location used for VM SKU availability, pricing, quota, and placement, for example 'eastus2'.")]
    public required string Location { get; set; }

    [Option(Description = "Hugging Face model ID, for example 'meta-llama/Llama-3.1-70B-Instruct'. When omitted, --parameter-count, --num-layers, --num-key-value-heads, and --head-dim are all required.")]
    public string? HuggingFaceModelId { get; set; }

    [Option(Description = "Logical total model parameter count, including all mixture-of-experts experts. Must be greater than zero. Required with --num-layers, --num-key-value-heads, and --head-dim when no model ID is provided.")]
    public long? ParameterCount { get; set; }

    [Option(Description = "Weight precision: FP32, FP16, BF16, FP8, INT8, INT4, or FP4. The service defaults to FP16 when omitted.")]
    public string? WeightPrecision { get; set; }

    [Option(Description = "Model layer count for KV-cache sizing. Supply together with --num-key-value-heads and --head-dim.")]
    public int? NumLayers { get; set; }

    [Option(Description = "Model KV-head count for KV-cache sizing. Supply together with --num-layers and --head-dim.")]
    public int? NumKeyValueHeads { get; set; }

    [Option(Description = "Attention-head dimension for KV-cache sizing. Supply together with --num-layers and --num-key-value-heads.")]
    public int? HeadDim { get; set; }

    [Option(Description = "Maximum simultaneous inference requests handled by each deployment replica. Must be greater than zero. The service defaults to 1.")]
    public int? MaxConcurrentRequestsPerReplica { get; set; }

    [Option(Description = "Prompt (input) tokens per inference request. Supply together with --max-output-tokens; the combined budget must total 1 through 131072.")]
    public int? PromptTokens { get; set; }

    [Option(Description = "Maximum generated output tokens per inference request. Supply together with --prompt-tokens; the combined budget must total 1 through 131072.")]
    public int? MaxOutputTokens { get; set; }

    [Option(Description = "Number of deployment replicas used for cost, quota, and placement. Must be greater than zero. The service defaults to 1.")]
    public int? ReplicaCount { get; set; }

    [Option(Description = "Azure VM procurement option: OnDemand or Spot. The service defaults to OnDemand.")]
    public string? ProcurementOption { get; set; }

    [Option(Description = "Recommendation ranking preference: Cost or Latency. The service defaults to Cost.")]
    public string? RankingPreference { get; set; }

    [Option(Description = "Optional deployment-wide maximum hourly cost. Must be a finite value greater than zero and requires --max-deployment-cost-per-hour-currency-code.")]
    public double? MaxDeploymentCostPerHourAmount { get; set; }

    [Option(Description = "Currency code for --max-deployment-cost-per-hour-amount. Only USD is supported.")]
    public string? MaxDeploymentCostPerHourCurrencyCode { get; set; }

    [Option(Description = "Optional Azure VM size to rank, for example 'Standard_ND96isr_H100_v5'. Repeat or list multiple values to rank only these VM sizes (1 to 50 distinct sizes of at most 128 characters).")]
    public string[]? TargetVmSize { get; set; }

    [Option(Description = "Evaluate subscription quota for candidate VM sizes.")]
    public bool IncludeQuota { get; set; }

    [Option(Description = "Query placement scores for quota-available candidate VM sizes.")]
    public bool IncludePlacement { get; set; }

    [Option(Description = "Fetch subscription-scoped VM pricing.")]
    public bool IncludePricing { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
