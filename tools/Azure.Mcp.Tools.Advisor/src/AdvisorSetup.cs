// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Commands.Metadata;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Commands.Remediation;
using Azure.Mcp.Tools.Advisor.Commands.ServiceGroupIntelligence;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Advisor;

public class AdvisorSetup : IAreaSetup
{
    public string Name => "advisor";
    public string Title => "Azure Advisor Recommendations";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAdvisorService, AdvisorService>();
        services.AddSingleton<IRemediationService, RemediationService>();
        services.AddSingleton<IRecommendationSummaryService, RecommendationSummaryService>();
        services.AddSingleton<IAdvisorServiceGroupIntelligenceService, AdvisorServiceGroupIntelligenceService>();
        services.AddSingleton<ServiceGroupIntelligenceGetCommand>();
        services.AddSingleton<RecommendationListCommand>();
        services.AddSingleton<RecommendationUpdateCommand>();
        services.AddSingleton<RecommendationSummaryCommand>();
        services.AddSingleton<RecommendationMetadataListCommand>();
        services.AddSingleton<MetadataGetCommand>();
        services.AddSingleton<RemediationGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Advisor command group
        var advisor = new CommandGroup(Name,
            "Azure Advisor operations - Query Azure Advisor recommendations in subscriptions or service groups and update " +
            "recommendation state in either scope. Use when you need visibility into Advisor recommendations or want to " +
            "manage their customer-provided state. Requires Azure " +
            "subscription context for querying recommendations and subscription-scoped updates; service-group updates " +
            "use service-group context.",
            Title);

        // Create Advisor subgroups
        var recommendation = new CommandGroup(
            "recommendation",
            "Advisor recommendations - List individual recommendations; summarize counts, rankings, lifecycle states, metadata subcategories, and service-retirement dates; or update subscription- or service-group-scoped customer-provided state.");
        advisor.AddSubGroup(recommendation);

        var metadata = new CommandGroup(
            "metadata",
            "Discover and retrieve the global Azure Advisor recommendation metadata catalog, also known as recommendation types, from Azure Resource Graph. List localized guidance, impact, categories, subcategories, supported resource types, actions, and service-retirement details, or get a specific catalog entry by recommendation type ID. Use the list command in greenfield environments with no generated recommendations, filter by resource type during brownfield onboarding, or find service retirements by tracking ID and retirement date. Service-retirement filters apply to the ServiceUpgradeAndRetirement subcategory; conflicting subcategory filters are rejected. List results are ordered High, Medium, then Low impact.");
        advisor.AddSubGroup(metadata);

        var remediation = new CommandGroup(
            "remediation",
            "Get the Azure Advisor remediation package that explains how to fix or resolve a recommendation type id. Returns step-by-step remediation guidance (manual, human-readable steps to fix the issue) and/or ready-to-run executable artifacts and scripts to remediate it: Azure CLI, PowerShell, Bicep, ARM template and terraform. Also includes remediation metadata, safety flags, methods with parameters, ordered steps, and verification. Use when an agent needs to know how to fix a recommendation, or wants the ARM, Bicep, CLI, PowerShell or terraform artifacts to remediate it.");
        advisor.AddSubGroup(remediation);

        var serviceGroupIntelligence = new CommandGroup(
            "servicegroupintelligence",
            "Advisor Service Group intelligence - Get the current Advisor status (On Track, Attention Needed, At Risk, Critical, Unavailable) and criticality of Service Groups. Use to see which Service Groups, including mission-critical ones, need attention. Defaults to all Service Groups when none is specified.");
        advisor.AddSubGroup(serviceGroupIntelligence);

        // Register Advisor commands
        recommendation.AddCommand<RecommendationSummaryCommand>(serviceProvider);
        recommendation.AddCommand<RecommendationListCommand>(serviceProvider);
        recommendation.AddCommand<RecommendationUpdateCommand>(serviceProvider);
        metadata.AddCommand<RecommendationMetadataListCommand>(serviceProvider);
        metadata.AddCommand<MetadataGetCommand>(serviceProvider);
        remediation.AddCommand<RemediationGetCommand>(serviceProvider);
        serviceGroupIntelligence.AddCommand<ServiceGroupIntelligenceGetCommand>(serviceProvider);

        return advisor;
    }
}
