// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Reflection;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Recommendation;

public sealed class RecommendationApplyCommand(ILogger<RecommendationApplyCommand> logger) : BaseCommand<RecommendationApplyOptions>
{
    private const string CommandTitle = "Apply Advisor Recommendations";
    private readonly ILogger<RecommendationApplyCommand> _logger = logger;
    private static readonly Dictionary<string, string> s_advisorRecommendationRulesCache = new();
    private static readonly HashSet<string> s_availableResources = new(StringComparer.OrdinalIgnoreCase);

    public override string Id => "174fd0df-a11a-4139-b987-efd57611f62f";

    public override string Name => "apply";

    public override string Description =>
        @"This tool applies advisor recommendations to create or modify IaaC files (like ARM, Bicep) for Azure resources. It returns the rules that can be applied to the IaaC file. This tool can also be used to fetch advisor recommendations to apply to a technical design or architecture diagram.";

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
                // Lazy load available resources on first validation
                if (s_availableResources.Count == 0)
                {
                    LoadAvailableResources();
                }

                bool validResource = s_availableResources.Contains(resource);

                if (!validResource)
                {
                    commandResult.AddError($"Invalid resource '{resource}'. Available resources: {string.Join(", ", s_availableResources.OrderBy(r => r))}");
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
            if (string.IsNullOrEmpty(options.Resource))
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = "Resource parameter is required.";
                return Task.FromResult(context.Response);
            }

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

    private string GetAdvisorRecommendationRules(string resourceFileName)
    {
        if (string.IsNullOrEmpty(resourceFileName))
        {
            throw new ArgumentException("Resource file name cannot be null or empty.", nameof(resourceFileName));
        }

        if (!s_advisorRecommendationRulesCache.TryGetValue(resourceFileName, out string? recommendationRules))
        {
            recommendationRules = LoadRecommendationRules(resourceFileName);
            s_advisorRecommendationRulesCache[resourceFileName] = recommendationRules;
        }
        return recommendationRules;
    }

    private static string LoadRecommendationRules(string resourceFileName)
    {
        Assembly assembly = typeof(RecommendationApplyCommand).Assembly;

        // Handle multiple files separated by comma
        string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, resourceFileName);
        return EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
    }

    private static void LoadAvailableResources()
    {
        Assembly assembly = typeof(RecommendationApplyCommand).Assembly;
        string resourcePrefix = "Azure.Mcp.Tools.Advisor.Resources.";

        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(resourcePrefix, StringComparison.OrdinalIgnoreCase) && 
                           name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .Select(name => name.Substring(resourcePrefix.Length, name.Length - resourcePrefix.Length - 5)) // Remove prefix and .json
            .ToList();

        foreach (var resourceName in resourceNames)
        {
            s_availableResources.Add(resourceName);
        }
    }
}
