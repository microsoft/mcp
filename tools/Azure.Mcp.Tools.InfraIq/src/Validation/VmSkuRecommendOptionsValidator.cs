// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.InfraIq.Models.Request;
using Azure.Mcp.Tools.InfraIq.Options.VmSku;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.InfraIq.Validation;

/// <summary>
/// Validates recommendVmSku inputs and normalizes them into the pinned 2026-06 request body. Rules mirror the
/// Optimus 2026-06 request validation; the Hugging Face model ID is checked only for being nonblank.
/// </summary>
public static class VmSkuRecommendOptionsValidator
{
    internal const int MaximumTokenBudget = 131_072;
    internal const int MaximumTargetVmSizeCount = 50;
    internal const int MaximumTargetVmSizeLength = 128;

    private static readonly string[] s_weightPrecisions = ["FP32", "FP16", "BF16", "FP8", "INT8", "INT4", "FP4"];
    private static readonly string[] s_procurementOptions = ["OnDemand", "Spot"];
    private static readonly string[] s_rankingPreferences = ["Cost", "Latency"];

    public static void Validate(VmSkuRecommendOptions options, ValidationResult validationResult)
    {
        var errors = validationResult.Errors;

        if (string.IsNullOrWhiteSpace(options.Location))
        {
            errors.Add("--location is required.");
        }
        else if (!IsSafeLocation(NormalizeLocation(options.Location)))
        {
            errors.Add("--location must be a valid Azure location such as 'eastus2'.");
        }

        ValidateModel(options, errors);
        ValidateWorkload(options, errors);
        ValidateDeployment(options, errors);
        ValidateOptimization(options, errors);
        ValidateTargetVmSizes(options.TargetVmSize, errors);
    }

    /// <summary>
    /// Mirrors Optimus location normalization: whitespace removed and lowercased.
    /// </summary>
    public static string NormalizeLocation(string location) =>
        string.Concat(location.Where(character => !char.IsWhiteSpace(character))).ToLowerInvariant();

    /// <summary>
    /// Builds the request body from options that have already passed <see cref="Validate"/>. Groups with no values
    /// are omitted; subscription options are always sent with explicit flags.
    /// </summary>
    public static InfraIqRecommendVmSkuRequestBody BuildRequestBody(VmSkuRecommendOptions options) => new()
    {
        Model = BuildModel(options),
        Workload = BuildWorkload(options),
        Deployment = BuildDeployment(options),
        Optimization = BuildOptimization(options),
        TargetVmSizes = options.TargetVmSize is { Length: > 0 }
            ? [.. options.TargetVmSize.Select(vmSize => vmSize.Trim())]
            : null,
        SubscriptionOptions = new()
        {
            IncludeQuota = options.IncludeQuota,
            IncludePlacement = options.IncludePlacement,
            IncludePricing = options.IncludePricing
        }
    };

    private static void ValidateModel(VmSkuRecommendOptions options, List<string> errors)
    {
        var hasModelId = options.HuggingFaceModelId is not null;
        var hasUsableModelId = !string.IsNullOrWhiteSpace(options.HuggingFaceModelId);

        if (hasModelId && !hasUsableModelId)
        {
            errors.Add("--hugging-face-model-id cannot be empty or whitespace.");
        }

        if (options.ParameterCount is <= 0)
        {
            errors.Add("--parameter-count must be greater than zero.");
        }

        if (options.WeightPrecision is not null && !TryCanonicalize(options.WeightPrecision, s_weightPrecisions, out _))
        {
            errors.Add("--weight-precision must be one of: FP32, FP16, BF16, FP8, INT8, INT4, FP4.");
        }

        var hasCompleteArchitecture = options.NumLayers.HasValue && options.NumKeyValueHeads.HasValue && options.HeadDim.HasValue;
        var hasAnyArchitecture = options.NumLayers.HasValue || options.NumKeyValueHeads.HasValue || options.HeadDim.HasValue;

        if (!hasUsableModelId)
        {
            // Without a model ID the backend cannot resolve the architecture, so every field is required.
            if (options.ParameterCount is null || !hasCompleteArchitecture)
            {
                errors.Add("Provide --hugging-face-model-id, or provide all of --parameter-count, --num-layers, --num-key-value-heads, and --head-dim.");
            }
        }
        else if (hasAnyArchitecture && !hasCompleteArchitecture)
        {
            errors.Add("--num-layers, --num-key-value-heads, and --head-dim must be supplied together.");
        }

        AddIfNotPositive(options.NumLayers, "--num-layers", errors);
        AddIfNotPositive(options.NumKeyValueHeads, "--num-key-value-heads", errors);
        AddIfNotPositive(options.HeadDim, "--head-dim", errors);
    }

    private static void ValidateWorkload(VmSkuRecommendOptions options, List<string> errors)
    {
        AddIfNotPositive(options.MaxConcurrentRequestsPerReplica, "--max-concurrent-requests-per-replica", errors);

        if (!options.PromptTokens.HasValue && !options.MaxOutputTokens.HasValue)
        {
            return;
        }

        if (options.PromptTokens.HasValue != options.MaxOutputTokens.HasValue)
        {
            errors.Add("--prompt-tokens and --max-output-tokens must be supplied together.");
            return;
        }

        if (options.PromptTokens < 0 || options.MaxOutputTokens < 0)
        {
            errors.Add("--prompt-tokens and --max-output-tokens must be zero or greater.");
            return;
        }

        var total = (long)options.PromptTokens!.Value + options.MaxOutputTokens!.Value;
        if (total is <= 0 or > MaximumTokenBudget)
        {
            errors.Add($"--prompt-tokens and --max-output-tokens must total from 1 through {MaximumTokenBudget}.");
        }
    }

    private static void ValidateDeployment(VmSkuRecommendOptions options, List<string> errors)
    {
        AddIfNotPositive(options.ReplicaCount, "--replica-count", errors);

        if (options.ProcurementOption is not null && !TryCanonicalize(options.ProcurementOption, s_procurementOptions, out _))
        {
            errors.Add("--procurement-option must be one of: OnDemand, Spot.");
        }
    }

    private static void ValidateOptimization(VmSkuRecommendOptions options, List<string> errors)
    {
        if (options.RankingPreference is not null && !TryCanonicalize(options.RankingPreference, s_rankingPreferences, out _))
        {
            errors.Add("--ranking-preference must be one of: Cost, Latency.");
        }

        if (options.MaxDeploymentCostPerHourAmount is null && options.MaxDeploymentCostPerHourCurrencyCode is null)
        {
            return;
        }

        if (options.MaxDeploymentCostPerHourAmount is not { } amount || !double.IsFinite(amount) || amount <= 0)
        {
            errors.Add("--max-deployment-cost-per-hour-amount must be a finite value greater than zero.");
        }

        if (!string.Equals(options.MaxDeploymentCostPerHourCurrencyCode?.Trim(), "USD", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("--max-deployment-cost-per-hour-currency-code must be USD when a maximum hourly cost is supplied.");
        }
    }

    private static void ValidateTargetVmSizes(string[]? targetVmSizes, List<string> errors)
    {
        if (targetVmSizes is null)
        {
            return;
        }

        if (targetVmSizes.Length is 0 or > MaximumTargetVmSizeCount)
        {
            errors.Add($"--target-vm-size must be supplied 1 through {MaximumTargetVmSizeCount} times.");
            return;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var vmSize in targetVmSizes)
        {
            if (string.IsNullOrWhiteSpace(vmSize))
            {
                errors.Add("--target-vm-size values must be nonblank VM sizes.");
                return;
            }

            if (vmSize.Length > MaximumTargetVmSizeLength)
            {
                errors.Add($"--target-vm-size values must contain at most {MaximumTargetVmSizeLength} characters.");
                return;
            }

            if (!seen.Add(vmSize.Trim()))
            {
                errors.Add("--target-vm-size values must not contain duplicates.");
                return;
            }
        }
    }

    private static InfraIqRecommendVmSkuModel? BuildModel(VmSkuRecommendOptions options)
    {
        var model = new InfraIqRecommendVmSkuModel
        {
            HuggingFaceModelId = options.HuggingFaceModelId?.Trim(),
            ParameterCount = options.ParameterCount,
            WeightPrecision = Canonicalize(options.WeightPrecision, s_weightPrecisions),
            NumLayers = options.NumLayers,
            NumKeyValueHeads = options.NumKeyValueHeads,
            HeadDim = options.HeadDim
        };

        return model.HuggingFaceModelId is null
            && model.ParameterCount is null
            && model.WeightPrecision is null
            && model.NumLayers is null
            && model.NumKeyValueHeads is null
            && model.HeadDim is null
            ? null
            : model;
    }

    private static InfraIqRecommendVmSkuWorkload? BuildWorkload(VmSkuRecommendOptions options)
    {
        InfraIqRecommendVmSkuTokenBudget? tokenBudget = options.PromptTokens.HasValue || options.MaxOutputTokens.HasValue
            ? new() { PromptTokens = options.PromptTokens, MaxOutputTokens = options.MaxOutputTokens }
            : null;

        return options.MaxConcurrentRequestsPerReplica is null && tokenBudget is null
            ? null
            : new()
            {
                MaxConcurrentRequestsPerReplica = options.MaxConcurrentRequestsPerReplica,
                TokenBudget = tokenBudget
            };
    }

    private static InfraIqRecommendVmSkuDeployment? BuildDeployment(VmSkuRecommendOptions options)
    {
        var procurementOption = Canonicalize(options.ProcurementOption, s_procurementOptions);

        return options.ReplicaCount is null && procurementOption is null
            ? null
            : new() { ReplicaCount = options.ReplicaCount, ProcurementOption = procurementOption };
    }

    private static InfraIqRecommendVmSkuOptimization? BuildOptimization(VmSkuRecommendOptions options)
    {
        var rankingPreference = Canonicalize(options.RankingPreference, s_rankingPreferences);
        InfraIqRecommendVmSkuHourlyCost? cost = options.MaxDeploymentCostPerHourAmount.HasValue
            ? new() { Amount = options.MaxDeploymentCostPerHourAmount, CurrencyCode = "USD" }
            : null;

        return rankingPreference is null && cost is null
            ? null
            : new() { RankingPreference = rankingPreference, MaxDeploymentCostPerHour = cost };
    }

    private static string? Canonicalize(string? value, string[] allowedValues) =>
        value is not null && TryCanonicalize(value, allowedValues, out var canonical) ? canonical : null;

    private static bool TryCanonicalize(string value, string[] allowedValues, out string? canonical)
    {
        var trimmed = value.Trim();
        canonical = allowedValues.FirstOrDefault(allowed => string.Equals(allowed, trimmed, StringComparison.OrdinalIgnoreCase));
        return canonical is not null;
    }

    private static void AddIfNotPositive(int? value, string optionName, List<string> errors)
    {
        if (value is <= 0)
        {
            errors.Add($"{optionName} must be greater than zero.");
        }
    }

    private static bool IsSafeLocation(string normalizedLocation) =>
        normalizedLocation.Length is > 0 and <= 128
        && char.IsAsciiLetterOrDigit(normalizedLocation[0])
        && normalizedLocation.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_');
}
