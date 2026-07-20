// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Reflection;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Recommendation;

[CommandMetadata(
    Id = "174fd0df-a11a-4139-b987-efd57611f62f",
    Name = "apply",
    Description = "This tool helps in applying advisor recommendations on IaaC files (like ARM, Terraform) for Azure resources. It returns the rules that can be applied to the IaaC file.",
    Title = "Apply Advisor Recommendations",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false
)]
public sealed class RecommendationApplyCommand(ILogger<RecommendationApplyCommand> logger)
    : BaseCommand<RecommendationApplyOptions, List<string>>
{
    private readonly ILogger<RecommendationApplyCommand> _logger = logger;
    private static readonly ConcurrentDictionary<string, string> s_advisorRecommendationRulesCache = new();
    private static readonly Lazy<HashSet<string>> s_availableResources = new(LoadAvailableResources);

    public override void ValidateOptions(RecommendationApplyOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!s_availableResources.Value.Contains(options.Resource))
        {
            validationResult.Errors.Add($"Invalid resource '{options.Resource}'. Available resources: {string.Join(", ", s_availableResources.Value.OrderBy(r => r))}");
        }
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, RecommendationApplyOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var resourceFileName = $"{options.Resource}.json";
            var recommendationApplyRules = GetAdvisorRecommendationRules(resourceFileName);

            context.Response.Results = ResponseResult.Create([recommendationApplyRules], AdvisorJsonContext.Default.ListString);

            context.Activity?.AddTag("RecommendationRules_Resource", options.Resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendation rules to apply for Resource: {Resource}",
                options.Resource);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    private static string GetAdvisorRecommendationRules(string resourceFileName)
    {
        if (!s_advisorRecommendationRulesCache.TryGetValue(resourceFileName, out string? recommendationRules))
        {
            recommendationRules = LoadRecommendationRules(resourceFileName);
            s_advisorRecommendationRulesCache[resourceFileName] = recommendationRules;
        }
        return recommendationRules ?? $"Rules weren't found for {resourceFileName}";
    }

    private static string LoadRecommendationRules(string resourceFileName)
    {
        Assembly assembly = typeof(RecommendationApplyCommand).Assembly;

        // Locate and read the embedded resource for the specified file name.
        string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, resourceFileName);
        return EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
    }

    private static HashSet<string> LoadAvailableResources()
    {
        Assembly assembly = typeof(RecommendationApplyCommand).Assembly;
        string resourcePrefix = "Azure.Mcp.Tools.Advisor.Resources.";

        var resources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(resourcePrefix, StringComparison.OrdinalIgnoreCase) &&
                           name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .Select(name => name.Substring(resourcePrefix.Length, name.Length - resourcePrefix.Length - 5)); // Remove prefix and .json

        foreach (var resourceName in resourceNames)
        {
            resources.Add(resourceName);
        }

        return resources;
    }
}
