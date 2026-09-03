// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Advisor.Models.Chaos;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.Mcp.Tools.Advisor.Services.Models;
using Azure.Mcp.Tools.Advisor.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Recommendation;

[CommandMetadata(
    Id = "7fdc5f3e-4baf-4da9-950d-e339192db36b",
    Name = "chaos-review",
    Title = "Review Advisor Compute Zone Down Chaos Readiness",
    Description = """
        Perform a read-only Azure Advisor Compute Zone Down remediation review for one exact virtual machine scale set.
        Use this tool when the user wants to check whether an Advisor-recommended VMSS is ready for an Azure Chaos Studio zone-down experiment, determine what setup or permissions are missing, inspect compatible Chaos workspaces, scenarios, and configurations, or view related scenario run history.
        Requires and verifies an exact active Advisor recommendation type ID GUID and exact Microsoft.Compute/virtualMachineScaleSets ARM resource ID.
        Optional workspace, scenario, and configuration ARM IDs must come from a previous review result and are used only to resolve an otherwise ambiguous selection.
        Returns deterministic readiness, blocker reason codes, candidate ARM IDs, validation state, required read permission, and active or historical runs.
        This tool never creates or changes a Chaos workspace, configuration, role assignment, validation, experiment, or Advisor approval plan.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationChaosReviewCommand(
    ILogger<RecommendationChaosReviewCommand> logger,
    IAdvisorChaosReviewService chaosReviewService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RecommendationChaosReviewOptions, RecommendationChaosReviewCommand.RecommendationChaosReviewResult>(subscriptionResolver)
{
    private readonly ILogger<RecommendationChaosReviewCommand> _logger = logger;
    private readonly IAdvisorChaosReviewService _chaosReviewService = chaosReviewService;

    public override void ValidateOptions(
        RecommendationChaosReviewOptions options,
        ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        var hasRecommendationType = Guid.TryParseExact(
            options.RecommendationTypeId?.Trim(),
            "D",
            out var recommendationTypeId) &&
            recommendationTypeId != Guid.Empty;
        if (!hasRecommendationType)
        {
            validationResult.Errors.Add(
                "--recommendation-type-id must be a non-empty GUID in xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx format.");
        }

        var resourceRecommendationTypeId = hasRecommendationType
            ? recommendationTypeId
            : new Guid("00000000-0000-0000-0000-000000000001");
        if (!ChaosRemediationTarget.TryCreate(
                resourceRecommendationTypeId,
                options.Resource,
                out _,
                out var resourceError))
        {
            validationResult.Errors.Add(
                $"--resource must be an exact Microsoft.Compute/virtualMachineScaleSets ARM resource ID. {resourceError}");
        }

        ValidateOptionalResource(
            "--workspace",
            options.Workspace,
            ChaosResourceIdValidator.IsWorkspace,
            "Microsoft.Chaos/workspaces",
            validationResult);
        ValidateOptionalResource(
            "--scenario",
            options.Scenario,
            ChaosResourceIdValidator.IsScenario,
            "Microsoft.Chaos/workspaces/scenarios",
            validationResult);
        ValidateOptionalResource(
            "--configuration",
            options.Configuration,
            ChaosResourceIdValidator.IsConfiguration,
            "Microsoft.Chaos/workspaces/scenarios/configurations",
            validationResult);

        if (options.Workspace is not null &&
            options.Scenario is not null &&
            !IsChild(options.Scenario, options.Workspace, "scenarios"))
        {
            validationResult.Errors.Add(
                "--scenario must be a child of the selected --workspace.");
        }

        if (options.Scenario is not null &&
            options.Configuration is not null &&
            !IsChild(options.Configuration, options.Scenario, "configurations"))
        {
            validationResult.Errors.Add(
                "--configuration must be a child of the selected --scenario.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        RecommendationChaosReviewOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var review = await _chaosReviewService.ReviewChaosRemediationAsync(
                options.Subscription!,
                Guid.Parse(options.RecommendationTypeId),
                options.Resource,
                options.Workspace,
                options.Scenario,
                options.Configuration,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(review),
                AdvisorJsonContext.Default.RecommendationChaosReviewResult);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error reviewing Advisor Chaos readiness. Subscription: {Subscription}, RecommendationTypeId: {RecommendationTypeId}, Resource: {Resource}.",
                options.Subscription,
                options.RecommendationTypeId,
                options.Resource);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Unauthorized =>
            "Azure authentication failed while reviewing Chaos readiness. Sign in again and retry.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed while resolving the Azure subscription for the Chaos review.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    private static void ValidateOptionalResource(
        string optionName,
        string? value,
        Func<string?, bool> validator,
        string expectedResourceType,
        ValidationResult validationResult)
    {
        if (value is not null && !validator(value))
        {
            validationResult.Errors.Add(
                $"{optionName} must be an exact {expectedResourceType} ARM resource ID.");
        }
    }

    private static bool IsChild(
        string child,
        string parent,
        string collectionName) =>
        child.StartsWith(
            $"{parent.TrimEnd('/')}/{collectionName}/",
            StringComparison.OrdinalIgnoreCase);

    public sealed record RecommendationChaosReviewResult(
        ChaosRemediationStatus Review);
}
