// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Security;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AIInfraOptimization.Commands;
using Azure.Mcp.Tools.AIInfraOptimization.Configuration;
using Azure.Mcp.Tools.AIInfraOptimization.Exceptions;
using Azure.Mcp.Tools.AIInfraOptimization.Models.Common;
using Azure.Mcp.Tools.AIInfraOptimization.Models.Request;
using Azure.Mcp.Tools.AIInfraOptimization.Models.Response;
using Azure.Mcp.Tools.AIInfraOptimization.Models.VmSku;
using Azure.ResourceManager;
using Microsoft.Extensions.Options;

namespace Azure.Mcp.Tools.AIInfraOptimization.Services;

/// <summary>
/// Raw ARM client for the single Microsoft.AIInfraOptimization recommendVmSku action. The completed destination is validated
/// before any token is requested, and the caller's ARM credential is the only credential used.
/// </summary>
internal sealed class AIInfraOptimizationArmClient(
    IAzureService azureService,
    IOptions<AIInfraOptimizationOptions> options,
    AIInfraOptimizationIngressFallbackCache fallbackCache)
    : BaseAzureService(azureService), IAIInfraOptimizationArmClient
{
    public const string HttpClientName = "AIInfraOptimizationArm";
    public const string ApiVersion = "2026-06-01-preview";
    public const string ProviderNamespace = "Microsoft.AIInfraOptimization";
    public const string UnsupportedCloudMessage =
        "AI Infra Optimization is only available in the Azure public cloud. Configure the Azure MCP Server for the public cloud to use this tool.";

    // Above the 55-second Optimus service deadline.
    public static readonly TimeSpan ClientTimeout = TimeSpan.FromSeconds(90);

    private const int BadGatewayStatus = 502;
    private const int MaxMetadataLength = 128;
    private const int MaxErrorBodyBytes = 1024 * 1024;
    private const int MaxRetryAfterDigits = 10;

    private static readonly string[] EndpointUnavailableCodes =
    [
        "NoResourceTypeEndpointAvailable",
        "InvalidResourceType",
        "NoRegisteredProviderFound",
    ];

    private readonly IOptions<AIInfraOptimizationOptions> _options = options;
    private readonly AIInfraOptimizationIngressFallbackCache _fallbackCache = fallbackCache;

    public async Task<VmSkuRecommendResult> RecommendVmSkuAsync(
        string subscriptionId,
        string location,
        AIInfraOptimizationRecommendVmSkuRequestBody body,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(body);
        cancellationToken.ThrowIfCancellationRequested();

        // With no configured origin, global ARM is primary and Canary is the fallback. Both completed destinations
        // are validated before any token is requested.
        var configuredOrigin = _options.Value.ArmIngressOrigin;
        var useFallback = string.IsNullOrWhiteSpace(configuredOrigin);
        var primaryUri = CreateAndValidateRequestUri(
            useFallback ? AIInfraOptimizationArmIngress.DefaultOrigin : configuredOrigin,
            subscriptionId,
            location);
        var fallbackUri = useFallback
            ? CreateAndValidateRequestUri(AIInfraOptimizationArmIngress.CanaryOrigin, subscriptionId, location)
            : null;
        EnsurePublicCloud(AzureService.CloudConfiguration.ArmEnvironment);

        var accessToken = await GetArmAccessTokenAsync(tenant, cancellationToken);
        ValidateRequiredParameter(nameof(accessToken), accessToken.Token);

        using var httpClient = AzureService.GetClient(HttpClientName);
        var clientOptions = AddDefaultPolicies(new AIInfraOptimizationArmClientOptions());
        clientOptions.Transport = new HttpClientTransport(httpClient);
        var pipeline = HttpPipelineBuilder.Build(clientOptions);
        var content = JsonSerializer.SerializeToUtf8Bytes(body, AIInfraOptimizationJsonContext.Default.AIInfraOptimizationRecommendVmSkuRequestBody);

        if (fallbackUri is null)
        {
            return await SendAsync(pipeline, primaryUri, accessToken, content, cancellationToken);
        }

        var routeKey = primaryUri.AbsolutePath;
        if (_fallbackCache.IsPrimaryUnavailable(routeKey))
        {
            return await SendAsync(pipeline, fallbackUri, accessToken, content, cancellationToken);
        }

        try
        {
            return await SendAsync(pipeline, primaryUri, accessToken, content, cancellationToken);
        }
        catch (AIInfraOptimizationArmException ex) when (IsEndpointUnavailable(ex))
        {
            _fallbackCache.MarkPrimaryUnavailable(routeKey);
            return await SendAsync(pipeline, fallbackUri, accessToken, content, cancellationToken);
        }
    }

    /// <summary>
    /// Returns whether ARM rejected the call because no endpoint serves the action on that ARM host. Only these
    /// routing errors trigger the Canary fallback; validation, auth, throttling, and server errors never do.
    /// </summary>
    internal static bool IsEndpointUnavailable(AIInfraOptimizationArmException exception) =>
        exception.Status is 400 or 404
        && exception.Code is not null
        && EndpointUnavailableCodes.Contains(exception.Code, StringComparer.OrdinalIgnoreCase);

    private static async Task<VmSkuRecommendResult> SendAsync(
        HttpPipeline pipeline,
        Uri requestUri,
        AccessToken accessToken,
        byte[] content,
        CancellationToken cancellationToken)
    {
        var clientRequestId = Guid.NewGuid().ToString();
        using var request = pipeline.CreateRequest();
        request.Method = RequestMethod.Post;
        request.Uri.Reset(requestUri);
        request.ClientRequestId = clientRequestId;
        request.Headers.SetValue("x-ms-return-client-request-id", "true");
        request.Headers.SetValue("Authorization", $"Bearer {accessToken.Token}");
        request.Headers.SetValue("Accept", "application/json");
        request.Headers.SetValue("Content-Type", "application/json");
        request.Content = RequestContent.Create(content);

        using var response = await pipeline.SendRequestAsync(request, cancellationToken);

        var context = ReadContext(response, clientRequestId);
        if (response.Status is < 200 or >= 300)
        {
            throw CreateException(response, context);
        }

        return ParseSuccess(response, context);
    }

    internal static Uri CreateAndValidateRequestUri(string? origin, string subscriptionId, string location)
    {
        if (!AIInfraOptimizationArmIngress.TryParseOrigin(origin, out var originUri, out var originError))
        {
            throw new InvalidOperationException(originError);
        }

        if (!Guid.TryParse(subscriptionId, out var subscriptionGuid))
        {
            throw new ArgumentException("The subscription ID must be a GUID.", nameof(subscriptionId));
        }

        if (!IsSafeRouteSegment(location))
        {
            throw new ArgumentException("The location is not a valid Azure location.", nameof(location));
        }

        var normalizedSubscriptionId = subscriptionGuid.ToString("D");
        var expectedPath = BuildPath(normalizedSubscriptionId, location);
        var requestUri = new Uri(originUri, $"{expectedPath}?api-version={ApiVersion}");

        // Validate the completed destination before a token can be requested or attached.
        if (!AIInfraOptimizationArmIngress.IsApprovedAuthority(requestUri)
            || !string.Equals(requestUri.Host, originUri.Host, StringComparison.OrdinalIgnoreCase)
            || requestUri.Fragment.Length > 0
            || requestUri.Query != $"?api-version={ApiVersion}"
            || requestUri.AbsolutePath != expectedPath)
        {
            throw new SecurityException("The AI Infra Optimization request destination is not an approved endpoint.");
        }

        return requestUri;
    }

    /// <summary>
    /// Every approved ingress is an Azure public-cloud ARM endpoint, so the active cloud must be Azure public cloud before the
    /// cloud-specific ARM token scope is requested. Every other cloud fails closed with a static message.
    /// </summary>
    internal static void EnsurePublicCloud(ArmEnvironment armEnvironment)
    {
        if (!ArmEnvironment.AzurePublicCloud.Equals(armEnvironment))
        {
            throw new InvalidOperationException(UnsupportedCloudMessage);
        }
    }

    internal static string? TryReadRetryAfter(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length <= MaxRetryAfterDigits && trimmed.All(char.IsAsciiDigit))
        {
            return trimmed;
        }

        return DateTimeOffset.TryParseExact(
            trimmed,
            "r",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out _)
            ? trimmed
            : null;
    }

    private static string BuildPath(string subscriptionId, string location) =>
        $"/subscriptions/{Uri.EscapeDataString(subscriptionId)}/providers/{ProviderNamespace}/locations/{Uri.EscapeDataString(location)}/recommendVmSku";

    private static bool IsSafeRouteSegment(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > MaxMetadataLength || !char.IsAsciiLetterOrDigit(value[0]))
        {
            return false;
        }

        return value.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_');
    }

    private static AIInfraOptimizationArmResponseContext ReadContext(Response response, string clientRequestId)
    {
        response.Headers.TryGetValue("x-ms-request-id", out var requestId);
        response.Headers.TryGetValue("x-ms-client-request-id", out var returnedClientRequestId);
        response.Headers.TryGetValue("Retry-After", out var retryAfter);

        return new AIInfraOptimizationArmResponseContext(
            SanitizeMetadata(requestId),
            SanitizeMetadata(returnedClientRequestId) ?? clientRequestId,
            response.Headers.Date,
            TryReadRetryAfter(retryAfter));
    }

    private static VmSkuRecommendResult ParseSuccess(Response response, AIInfraOptimizationArmResponseContext context)
    {
        AIInfraOptimizationRecommendVmSkuResponse? parsed = null;
        if (IsJson(response) && response.Content.ToMemory().Length > 0)
        {
            try
            {
                parsed = JsonSerializer.Deserialize(
                    response.Content.ToStream(),
                    AIInfraOptimizationJsonContext.Default.AIInfraOptimizationRecommendVmSkuResponse);
            }
            catch (JsonException)
            {
            }
        }

        // A success status with an unusable payload is an upstream failure, not a result.
        return parsed is null
            ? throw new AIInfraOptimizationArmException(
                BadGatewayStatus,
                code: null,
                target: null,
                context.RequestId,
                context.ClientRequestId,
                context.RetryAfter)
            : new VmSkuRecommendResult(parsed, context);
    }

    private static AIInfraOptimizationArmException CreateException(Response response, AIInfraOptimizationArmResponseContext context)
    {
        string? code = null;
        string? target = null;

        if (IsJson(response) && response.Content.ToMemory().Length is > 0 and <= MaxErrorBodyBytes)
        {
            try
            {
                var error = JsonSerializer.Deserialize(
                    response.Content.ToStream(),
                    AIInfraOptimizationJsonContext.Default.AIInfraOptimizationArmODataErrorEnvelope)?.Error;
                code = SanitizeMetadata(error?.Code);
                target = SanitizeMetadata(error?.Target);
            }
            catch (JsonException)
            {
            }
        }

        return new AIInfraOptimizationArmException(
            response.Status,
            code,
            target,
            context.RequestId,
            context.ClientRequestId,
            context.RetryAfter);
    }

    private static bool IsJson(Response response)
    {
        var contentType = response.Headers.ContentType;
        return contentType is not null
            && (contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)
                || contentType.Contains("+json", StringComparison.OrdinalIgnoreCase));
    }

    private static string? SanitizeMetadata(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > MaxMetadataLength)
        {
            return null;
        }

        return trimmed.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.' or ':' or '[' or ']')
            ? trimmed
            : null;
    }
}
