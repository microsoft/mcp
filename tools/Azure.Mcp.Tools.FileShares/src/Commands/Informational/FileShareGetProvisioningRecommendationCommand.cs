// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FileShares.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Commands.Informational;

public sealed class FileShareGetProvisioningRecommendationCommand(ILogger<FileShareGetProvisioningRecommendationCommand> logger)
    : BaseFileSharesCommand<FileShareGetProvisioningRecommendationCommand.FileShareGetProvisioningRecommendationOptions>
{
    private const string CommandTitle = "Get File Share Provisioning Recommendation";
    private readonly ILogger<FileShareGetProvisioningRecommendationCommand> _logger = logger;

    public override string Id => "b2c3d4e5-f6a7-48b9-c0d1-e2f3a4b5c6d7";

    public override string Name => "get-provisioning-recommendation";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override string Description => "Get provisioning recommendations for File Shares";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
    }

    protected override FileShareGetProvisioningRecommendationOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        try
        {
            var recommendation = new ProvisioningRecommendation
            {
                RecommendedStorageAccountType = "Standard_LRS",
                RecommendedTier = "Hot",
                CostOptimizationTips = new[]
                {
                    "Use Standard tier for general-purpose file shares",
                    "Use Premium tier for high-performance or latency-sensitive workloads",
                    "Enable soft delete to recover accidentally deleted shares"
                },
                PerformanceConsiderations = "Consider Premium tier if your workload requires <10ms latency"
            };

            var result = new FileShareGetProvisioningRecommendationCommandResult(recommendation);
            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareGetProvisioningRecommendationCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provisioning recommendation. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class FileShareGetProvisioningRecommendationOptions : BaseFileSharesOptions
    {
    }

    internal record FileShareGetProvisioningRecommendationCommandResult(ProvisioningRecommendation Recommendation);
}

/// <summary>
/// Model representing provisioning recommendations.
/// </summary>
public sealed record ProvisioningRecommendation
{
    [JsonPropertyName("recommendedStorageAccountType")]
    public string? RecommendedStorageAccountType { get; set; }

    [JsonPropertyName("recommendedTier")]
    public string? RecommendedTier { get; set; }

    [JsonPropertyName("costOptimizationTips")]
    public string[]? CostOptimizationTips { get; set; }

    [JsonPropertyName("performanceConsiderations")]
    public string? PerformanceConsiderations { get; set; }
}
