// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Models.Chaos;
using Azure.Mcp.Tools.Advisor.Services.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Advisor.Services;

public sealed partial class AdvisorChaosReviewService(
    IAzureService azureService,
    IAdvisorService advisorService,
    ILogger<AdvisorChaosReviewService> logger)
    : BaseAzureService(azureService), IAdvisorChaosReviewService
{
    private const string ChaosApiVersion = "2026-08-01-preview";
    private const string ComputeApiVersion = "2024-11-01";
    private const string RequiredActionIdPrefix =
        "microsoft-virtualMachineScaleSet-shutdown/";
    private const string RequiredProvisioningState = "Succeeded";
    private const string VmssReadPermission =
        "Microsoft.Compute/virtualMachineScaleSets/read";
    private const string WorkspaceReadPermission = "Microsoft.Chaos/workspaces/read";
    private const string ConfigurationReadPermission =
        "Microsoft.Chaos/workspaces/scenarios/configurations/read";
    private const int MinimumRequiredZoneCount = 2;
    private const int MaxAdvisorRecommendationsForVerification = 100;
    private const int MaxListPages = 10;
    private const int MaxThrottleRetries = 3;

    private static readonly TimeSpan ThrottleRetryDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxRetryAfter = TimeSpan.FromSeconds(30);

    private readonly IAdvisorService _advisorService =
        advisorService ?? throw new ArgumentNullException(nameof(advisorService));
    private readonly ILogger<AdvisorChaosReviewService> _logger = logger;

    public async Task<ChaosRemediationStatus> ReviewChaosRemediationAsync(
        string subscription,
        Guid recommendationTypeId,
        string resource,
        string? workspace = null,
        string? scenario = null,
        string? configuration = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resource), resource));

        if (!ChaosRemediationTarget.TryCreate(
                recommendationTypeId,
                resource,
                out var target,
                out var targetError))
        {
            return StatusReadFailure(
                "InvalidVmssResourceId",
                targetError ?? "The selected resource is not a valid VMSS.",
                recommendationTypeId.ToString("D"),
                resource);
        }

        var subscriptionResource = await AzureService.GetSubscription(
            subscription,
            tenant,
            cancellationToken);
        if (!Guid.TryParse(subscriptionResource.Id.SubscriptionId, out var subscriptionId) ||
            subscriptionId != target.SubscriptionId)
        {
            return StatusReadFailure(
                "SubscriptionMismatch",
                "The VMSS resource ID does not belong to the resolved subscription.",
                recommendationTypeId.ToString("D"),
                target.ResourceId);
        }

        var recommendationFailure = await VerifyAdvisorRecommendationAsync(
            subscription,
            target,
            tenant,
            cancellationToken);
        if (recommendationFailure is not null)
        {
            return recommendationFailure;
        }

        var managementEndpoint = AzureService.CloudConfiguration.ArmEnvironment.Endpoint
            ?? throw new InvalidOperationException("The Azure Resource Manager endpoint is not configured.");
        var accessToken = await GetArmAccessTokenAsync(tenant, cancellationToken);

        using var client = AzureService.GetClient();
        var requestContext = new ArmRequestContext(
            client,
            managementEndpoint,
            accessToken.Token,
            cancellationToken);

        try
        {
            return await ReviewStatusCoreAsync(
                requestContext,
                target,
                workspace,
                scenario,
                configuration);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Chaos review timed out for resource {Resource}.",
                target.ResourceId);
            return StatusReadFailure(
                "ArmRequestFailed",
                "The Chaos review could not be completed because an Azure request timed out.",
                recommendationTypeId.ToString("D"),
                target.ResourceId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Chaos review request failed for resource {Resource}.",
                target.ResourceId);
            return StatusReadFailure(
                "ArmRequestFailed",
                "The Chaos review could not be completed because an Azure request failed.",
                recommendationTypeId.ToString("D"),
                target.ResourceId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Azure returned malformed Chaos review data for resource {Resource}.",
                target.ResourceId);
            return StatusReadFailure(
                "InvalidArmResponse",
                "Azure returned an invalid response while reviewing Chaos readiness.",
                recommendationTypeId.ToString("D"),
                target.ResourceId);
        }
    }

    private async Task<ChaosRemediationStatus?> VerifyAdvisorRecommendationAsync(
        string subscription,
        ChaosRemediationTarget target,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var recommendationTypeId = target.RecommendationTypeId.ToString("D");
        var recommendations = await _advisorService.ListRecommendationsAsync(
            subscription,
            target.ResourceGroup,
            new RecommendationFilters(
                Status: RecommendationStatus.New,
                RecommendationTypeId: recommendationTypeId,
                Resource: target.ResourceId),
            MaxAdvisorRecommendationsForVerification,
            tenant,
            cancellationToken);

        var exactMatch = recommendations.Results.Any(recommendation =>
            string.Equals(
                recommendation.Properties.RecommendationTypeId,
                recommendationTypeId,
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                recommendation.Properties.RecommendationStatus,
                RecommendationStatus.New.ToString(),
                StringComparison.OrdinalIgnoreCase) &&
            IsExactResourceId(
                recommendation.Properties.ResourceMetadata?.ResourceId,
                target.ResourceId));
        if (exactMatch)
        {
            return null;
        }

        return recommendations.AreResultsTruncated
            ? StatusReadFailure(
                "AdvisorRecommendationVerificationIncomplete",
                "Advisor returned a truncated result before the exact active recommendation could be verified.",
                recommendationTypeId,
                target.ResourceId)
            : StatusReadFailure(
                "AdvisorRecommendationNotFound",
                "No active Advisor recommendation exactly matches the supplied recommendation type and VMSS resource ID.",
                recommendationTypeId,
                target.ResourceId);
    }

    private static bool IsExactResourceId(
        string? resourceId,
        string expectedResourceId) =>
        !string.IsNullOrWhiteSpace(resourceId) &&
        string.Equals(
            resourceId.TrimEnd('/'),
            expectedResourceId.TrimEnd('/'),
            StringComparison.OrdinalIgnoreCase);

    private async Task<ChaosTargetReview> ReviewTargetAsync(
        ArmRequestContext context,
        ChaosRemediationTarget target)
    {
        var response = await GetWithThrottleRetryAsync(
            context,
            $"{target.ResourceId}?api-version={Uri.EscapeDataString(ComputeApiVersion)}");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return TargetFailure(
                "CustomerReadAccessDenied",
                "The current identity cannot read the selected VMSS.",
                target,
                VmssReadPermission);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return TargetFailure(
                "ResourceNotFound",
                "The selected VMSS no longer exists or is not visible.",
                target);
        }

        if (!response.IsSuccessStatusCode)
        {
            return TargetFailure(
                "ArmReadFailed",
                $"Azure returned HTTP {(int)response.StatusCode} while reading the VMSS.",
                target);
        }

        using var document = JsonDocument.Parse(response.Body);
        return BuildTargetReview(target, document.RootElement);
    }

    private static ChaosTargetReview BuildTargetReview(
        ChaosRemediationTarget target,
        JsonElement root)
    {
        var resourceType = GetString(root, "type");
        var location = GetString(root, "location");
        var zones = GetStringArray(root, "zones");
        var capacity = GetInt64(root, "sku", "capacity");
        var provisioningState = GetString(root, "properties", "provisioningState");

        if (!string.Equals(
                resourceType,
                "Microsoft.Compute/virtualMachineScaleSets",
                StringComparison.OrdinalIgnoreCase))
        {
            return Ineligible("ResourceTypeMismatch", "The selected Azure resource is not a VMSS.");
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            return Ineligible("LocationMissing", "The VMSS response does not contain a location.");
        }

        if (zones.Count < MinimumRequiredZoneCount)
        {
            return Ineligible(
                "InsufficientAvailabilityZones",
                $"The VMSS must use at least {MinimumRequiredZoneCount} availability zones.");
        }

        if (capacity is null or <= 0)
        {
            return Ineligible("NoActiveCapacity", "The VMSS currently has no configured capacity.");
        }

        if (!string.Equals(
                provisioningState,
                RequiredProvisioningState,
                StringComparison.OrdinalIgnoreCase))
        {
            return Ineligible(
                "ProvisioningNotSucceeded",
                $"The VMSS provisioning state must be {RequiredProvisioningState}.");
        }

        return new(
            "Eligible",
            true,
            null,
            "The selected VMSS is currently eligible for Compute Zone Down review.",
            target.RecommendationTypeId.ToString("D"),
            target.ResourceId,
            location,
            zones,
            capacity,
            provisioningState);

        ChaosTargetReview Ineligible(string reasonCode, string message) =>
            new(
                "Ineligible",
                false,
                reasonCode,
                message,
                target.RecommendationTypeId.ToString("D"),
                target.ResourceId,
                location,
                zones,
                capacity,
                provisioningState);
    }

    private static ChaosTargetReview TargetFailure(
        string reasonCode,
        string message,
        ChaosRemediationTarget target,
        string? requiredPermission = null) =>
        new(
            "Blocked",
            false,
            reasonCode,
            message,
            target.RecommendationTypeId.ToString("D"),
            target.ResourceId,
            null,
            [],
            null,
            null,
            requiredPermission);

    private async Task<ArmResponse> GetWithThrottleRetryAsync(
        ArmRequestContext context,
        string path)
    {
        var response = await SendGetAsync(context, path);
        for (var retry = 1;
             response.StatusCode == HttpStatusCode.TooManyRequests &&
             retry <= MaxThrottleRetries;
             retry++)
        {
            var delay = response.RetryAfter ?? ThrottleRetryDelay;
            if (delay > MaxRetryAfter)
            {
                delay = MaxRetryAfter;
            }

            _logger.LogWarning(
                "Chaos ARM read was throttled; retrying attempt {Retry} of {MaxRetries} after {Delay}.",
                retry,
                MaxThrottleRetries,
                delay);

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, context.CancellationToken);
            }

            response = await SendGetAsync(context, path);
        }

        return response;
    }

    private async Task<PagedResult<T>> GetAllPagesAsync<T>(
        ArmRequestContext context,
        string initialPath,
        Func<string, IReadOnlyList<T>> parseItems)
    {
        var items = new List<T>();
        string? nextPath = initialPath;
        if (!TryGetArmPath(initialPath, context.ManagementEndpoint, out var expectedPath))
        {
            throw new ArgumentException("The initial ARM list path is invalid.", nameof(initialPath));
        }

        for (var page = 1; page <= MaxListPages && nextPath is not null; page++)
        {
            if (!TryGetArmPath(nextPath, context.ManagementEndpoint, out var pagePath) ||
                !string.Equals(
                    pagePath.TrimEnd('/'),
                    expectedPath.TrimEnd('/'),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonException(
                    "Azure returned a pagination link outside the original ARM collection.");
            }

            var response = await GetWithThrottleRetryAsync(context, nextPath);
            if (!response.IsSuccessStatusCode)
            {
                return new(items, response, false);
            }

            items.AddRange(parseItems(response.Body));
            nextPath = GetNextLink(response.Body);
        }

        return new(items, null, nextPath is not null);
    }

    private static async Task<ArmResponse> SendGetAsync(
        ArmRequestContext context,
        string path)
    {
        var requestUri = BuildArmUri(path, context.ManagementEndpoint);
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            context.AccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await context.Client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            context.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(context.CancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new RequestFailedException(
                (int)response.StatusCode,
                "Azure authentication failed while reading Chaos remediation resources.");
        }

        return new(
            response.StatusCode,
            body,
            GetRetryAfter(response.Headers.RetryAfter));
    }

    private static Uri BuildArmUri(string path, Uri managementEndpoint)
    {
        if (!TryValidateArmUri(path, managementEndpoint))
        {
            throw new ArgumentException("The ARM path or URL is invalid.", nameof(path));
        }

        return path.StartsWith("/", StringComparison.Ordinal)
            ? new Uri(managementEndpoint, path)
            : new Uri(path, UriKind.Absolute);
    }

    private static TimeSpan? GetRetryAfter(RetryConditionHeaderValue? retryAfter)
    {
        if (retryAfter?.Delta is TimeSpan delta)
        {
            return delta < TimeSpan.Zero ? TimeSpan.Zero : delta;
        }

        if (retryAfter?.Date is DateTimeOffset retryDate)
        {
            var delay = retryDate - DateTimeOffset.UtcNow;
            return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
        }

        return null;
    }

    private sealed record ArmRequestContext(
        HttpClient Client,
        Uri ManagementEndpoint,
        string AccessToken,
        CancellationToken CancellationToken);

    private sealed record ArmResponse(
        HttpStatusCode StatusCode,
        string Body,
        TimeSpan? RetryAfter = null)
    {
        public bool IsSuccessStatusCode => (int)StatusCode is >= 200 and <= 299;
    }

    private sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        ArmResponse? Failure,
        bool PageLimitExceeded);
}
