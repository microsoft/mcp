// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
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
public sealed class RecommendationApplyCommand(ILogger<RecommendationApplyCommand> logger) : BaseCommand<RecommendationApplyOptions>
{
    private readonly ILogger<RecommendationApplyCommand> _logger = logger;
    private static readonly ConcurrentDictionary<string, string> s_advisorRecommendationRulesCache = new();
    private static readonly Lazy<HashSet<string>> s_availableResources = new(LoadAvailableResources);

    protected override void RegisterOptions(Command command)
    {
        command.Options.Add(RecommendationApplyOptionDefinitions.Resource);
        command.Validators.Add(commandResult =>
        {
            commandResult.TryGetValue(RecommendationApplyOptionDefinitions.Resource, out string? resource);

            if (string.IsNullOrWhiteSpace(resource))
            {
                commandResult.AddError("Resource parameter is required.");
            }
            else
            {
                bool validResource = s_availableResources.Value.Contains(resource);

                if (!validResource)
                {
                    commandResult.AddError($"Invalid resource '{resource}'. Available resources: {string.Join(", ", s_availableResources.Value.OrderBy(r => r))}");
                }
            }
        });
    }

    protected override RecommendationApplyOptions BindOptions(ParseResult parseResult)
    {
        return new RecommendationApplyOptions
        {
            Resource = parseResult.GetValueOrDefault<string>(RecommendationApplyOptionDefinitions.Resource.Name)
        };
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        var options = BindOptions(parseResult);

        try
        {
            var resourceFileName = $"{options.Resource}.json";
            var recommendationApplyRules = GetAdvisorRecommendationRules(resourceFileName);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create([recommendationApplyRules], AdvisorJsonContext.Default.ListString);
            context.Response.Message = string.Empty;

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
