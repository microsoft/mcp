// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class AdvisorRecommendationUpdateServiceTests
{
    private const string SubscriptionId = "12345678-1234-1234-1234-123456789012";
    private readonly IAzureService _azureService = Substitute.For<IAzureService>();

    [Fact]
    public async Task UpdateRecommendationAsync_SendsCloudAwareAuthenticatedPatch()
    {
        var handler = ConfigureService(
            ArmEnvironment.AzureChina,
            """
            {
              "id": "/subscriptions/12345678-1234-1234-1234-123456789012/providers/Microsoft.Advisor/recommendations/rec-1",
              "name": "rec-1",
              "type": "Microsoft.Advisor/recommendations",
              "properties": {
                "category": "HighAvailability",
                "impact": "High",
                "recommendationStatus": "Completed",
                "shortDescription": {
                  "problem": "Enable availability zones",
                  "solution": "Deploy across zones"
                },
                "resourceMetadata": {
                  "resourceId": "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm"
                }
              }
            }
            """);
        var service = new AdvisorService(_azureService);

        var result = await service.UpdateRecommendationAsync(
            "subscription-name",
            "rec/1",
            RecommendationStatus.Completed,
            DateTimeOffset.UtcNow.AddDays(10),
            RecommendationDismissReason.Other,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, handler.Method);
        Assert.Equal(
            $"{ArmEnvironment.AzureChina.Endpoint}subscriptions/{SubscriptionId}/providers/Microsoft.Advisor/recommendations/rec%2F1?api-version=2026-03-01-preview",
            handler.RequestUri?.AbsoluteUri);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("test-token", handler.AuthorizationParameter);
        Assert.Equal("application/json", handler.Accept);

        using var request = JsonDocument.Parse(handler.RequestBody!);
        var properties = request.RootElement.GetProperty("properties");
        Assert.Equal("Completed", properties.GetProperty("recommendationStatus").GetString());
        Assert.False(properties.TryGetProperty("postponedUntilDateTime", out _));
        Assert.False(properties.TryGetProperty("recommendationDismissReason", out _));

        Assert.Equal("rec-1", result.RecommendationId);
        Assert.Equal("Completed", result.RecommendationStatus);
        Assert.Equal("Deploy across zones", result.Solution);
        Assert.Equal("Microsoft.Compute/virtualMachines", result.ImpactedResourceType);
    }

    [Fact]
    public async Task UpdateRecommendationAsync_Postponed_IncludesPostponementDate()
    {
        var postponedUntil = new DateTimeOffset(2099, 1, 1, 12, 30, 0, TimeSpan.FromHours(5.5));
        var handler = ConfigureService(
            ArmEnvironment.AzurePublicCloud,
            """
            {
              "name": "rec-1",
              "properties": {
                "category": "Cost",
                "recommendationStatus": "Postponed",
                "postponedUntilDateTime": "2099-01-01T00:00:00Z",
                "shortDescription": { "problem": "Right-size a resource" }
              }
            }
            """);
        var service = new AdvisorService(_azureService);

        await service.UpdateRecommendationAsync(
            SubscriptionId,
            "rec-1",
            RecommendationStatus.Postponed,
            postponedUntil,
            cancellationToken: TestContext.Current.CancellationToken);

        using var request = JsonDocument.Parse(handler.RequestBody!);
        var properties = request.RootElement.GetProperty("properties");
        Assert.Equal("Postponed", properties.GetProperty("recommendationStatus").GetString());
        Assert.Equal(
            postponedUntil,
            properties.GetProperty("postponedUntilDateTime").GetDateTimeOffset());
        Assert.False(properties.TryGetProperty("recommendationDismissReason", out _));
    }

    [Fact]
    public async Task UpdateRecommendationAsync_Dismissed_IncludesDismissReason()
    {
        var handler = ConfigureService(
            ArmEnvironment.AzurePublicCloud,
            """
            {
              "name": "rec-1",
              "properties": {
                "category": "Cost",
                "recommendationStatus": "Dismissed",
                "recommendationDismissReason": "RiskIsAcceptable",
                "shortDescription": { "problem": "Right-size a resource" }
              }
            }
            """);
        var service = new AdvisorService(_azureService);

        await service.UpdateRecommendationAsync(
            SubscriptionId,
            "rec-1",
            RecommendationStatus.Dismissed,
            recommendationDismissReason: RecommendationDismissReason.RiskIsAcceptable,
            cancellationToken: TestContext.Current.CancellationToken);

        using var request = JsonDocument.Parse(handler.RequestBody!);
        var properties = request.RootElement.GetProperty("properties");
        Assert.Equal(
            "RiskIsAcceptable",
            properties.GetProperty("recommendationDismissReason").GetString());
        Assert.False(properties.TryGetProperty("postponedUntilDateTime", out _));
    }

    [Theory]
    [InlineData(RecommendationStatus.Postponed, null, null, "--postponed-until-date-time is required")]
    [InlineData(RecommendationStatus.Dismissed, null, null, "--recommendation-dismiss-reason is required")]
    public async Task UpdateRecommendationAsync_MissingStateRequirement_ThrowsArgumentException(
        RecommendationStatus status,
        DateTimeOffset? postponedUntil,
        RecommendationDismissReason? dismissReason,
        string expectedMessage)
    {
        var service = new AdvisorService(_azureService);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UpdateRecommendationAsync(
                SubscriptionId,
                "rec-1",
                status,
                postponedUntil,
                dismissReason,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains(expectedMessage, exception.Message);
        _azureService.DidNotReceive().GetClient(Arg.Any<string?>());
    }

    [Fact]
    public async Task UpdateRecommendationAsync_BackendJsonError_UsesErrorCodeAndMessage()
    {
        const string rawBody = """
            {
              "error": {
                "code": "RecommendationStateNotAllowed",
                "message": "internal backend details"
              }
            }
            """;
        ConfigureService(
            ArmEnvironment.AzurePublicCloud,
            rawBody,
            HttpStatusCode.BadRequest);
        var service = new AdvisorService(_azureService);

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() =>
            service.UpdateRecommendationAsync(
                SubscriptionId,
                "rec-1",
                RecommendationStatus.Completed,
                retryPolicy: new RetryPolicyOptions { MaxRetries = 0 },
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal((int)HttpStatusCode.BadRequest, exception.Status);
        Assert.Contains("RecommendationStateNotAllowed", exception.Message);
        Assert.Contains("internal backend details", exception.Message);
    }

    [Theory]
    [InlineData(
        HttpStatusCode.BadRequest,
        "SecurityRecommendationStateChangeBlocked",
        "State changes are not allowed for Security category recommendations")]
    [InlineData(
        HttpStatusCode.BadRequest,
        "UndefinedRecommendationStateChangeBlocked",
        "State changes are not allowed for recommendations with an Undefined state.")]
    [InlineData(
        HttpStatusCode.BadRequest,
        "ResolvedRecommendationStateChangeBlocked",
        "State changes are not allowed for recommendations that have already been resolved from platform side.")]
    [InlineData(
        HttpStatusCode.NotFound,
        "RecommendationNotFound",
        "Recommendation was not found. It may have been deleted.")]
    [InlineData(
        HttpStatusCode.Conflict,
        "ConcurrentModification",
        "The recommendation was modified by another operation. Please retrieve the latest version and retry.")]
    public async Task UpdateRecommendationAsync_KnownLifecycleError_PreservesCodeAndPublicMessage(
        HttpStatusCode statusCode,
        string errorCode,
        string errorMessage)
    {
        var responseBody = $$"""
            {
              "error": {
                "code": "{{errorCode}}",
                "message": "{{errorMessage}}"
              }
            }
            """;
        ConfigureService(
            ArmEnvironment.AzurePublicCloud,
            responseBody,
            statusCode);
        var service = new AdvisorService(_azureService);

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() =>
            service.UpdateRecommendationAsync(
                SubscriptionId,
                "rec-1",
                RecommendationStatus.Completed,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Contains(errorMessage, exception.Message);
    }

    [Fact]
    public async Task UpdateRecommendationAsync_BackendNonJsonError_UsesStatusCode()
    {
        ConfigureService(
            ArmEnvironment.AzurePublicCloud,
            "gateway failure",
            HttpStatusCode.ServiceUnavailable,
            "text/plain");
        var service = new AdvisorService(_azureService);

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() =>
            service.UpdateRecommendationAsync(
                SubscriptionId,
                "rec-1",
                RecommendationStatus.Completed,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, exception.Status);
        Assert.Contains("status code 503", exception.Message);
        Assert.DoesNotContain("gateway failure", exception.Message);
    }

    [Fact]
    public async Task UpdateRecommendationAsync_RetryableFailure_RetriesRequest()
    {
        var attempts = 0;
        var handler = ConfigureService(
            ArmEnvironment.AzurePublicCloud,
            string.Empty,
            responseFactory: () =>
            {
                attempts++;
                if (attempts == 1)
                {
                    var retryResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                    retryResponse.Headers.RetryAfter = new(TimeSpan.Zero);
                    return retryResponse;
                }

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                          "name": "rec-1",
                          "properties": {
                            "category": "Cost",
                            "recommendationStatus": "Completed",
                            "shortDescription": { "problem": "Right-size a resource" }
                          }
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            });
        var service = new AdvisorService(_azureService);

        var result = await service.UpdateRecommendationAsync(
            SubscriptionId,
            "rec-1",
            RecommendationStatus.Completed,
            retryPolicy: new RetryPolicyOptions { MaxRetries = 1 },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, handler.CallCount);
        Assert.Equal("Completed", result.RecommendationStatus);
    }

    [Fact]
    public async Task UpdateRecommendationAsync_MaxRetriesAboveThree_IsCappedAtThree()
    {
        var handler = ConfigureService(
            ArmEnvironment.AzurePublicCloud,
            string.Empty,
            responseFactory: () =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                response.Headers.RetryAfter = new(TimeSpan.Zero);
                return response;
            });
        var service = new AdvisorService(_azureService);

        await Assert.ThrowsAsync<RequestFailedException>(() =>
            service.UpdateRecommendationAsync(
                SubscriptionId,
                "rec-1",
                RecommendationStatus.Completed,
                retryPolicy: new RetryPolicyOptions { MaxRetries = 10 },
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(4, handler.CallCount);
    }

    [Fact]
    public async Task UpdateRecommendationAsync_NoRetryOptions_DefaultsToThreeRetries()
    {
        var handler = ConfigureService(
            ArmEnvironment.AzurePublicCloud,
            string.Empty,
            responseFactory: () =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                response.Headers.RetryAfter = new(TimeSpan.Zero);
                return response;
            });
        var service = new AdvisorService(_azureService);

        await Assert.ThrowsAsync<RequestFailedException>(() =>
            service.UpdateRecommendationAsync(
                SubscriptionId,
                "rec-1",
                RecommendationStatus.Completed,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(4, handler.CallCount);
    }

    private CapturingHttpMessageHandler ConfigureService(
        ArmEnvironment environment,
        string responseBody,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string mediaType = "application/json",
        Func<HttpResponseMessage>? responseFactory = null)
    {
        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(environment);
        _azureService.CloudConfiguration.Returns(cloudConfiguration);

        var subscriptionResource = Substitute.For<SubscriptionResource>();
        subscriptionResource.Id.Returns(SubscriptionResource.CreateResourceIdentifier(SubscriptionId));
        _azureService.GetSubscription(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(subscriptionResource);

        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(
            Arg.Any<TokenRequestContext>(),
            Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(
                new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1))));
        _azureService.GetTokenCredentialAsync(
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(credential);

        var handler = new CapturingHttpMessageHandler(
            responseFactory ?? (() => new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, mediaType)
            }));
        _azureService.GetClient(Arg.Any<string?>()).Returns(new HttpClient(handler));

        return handler;
    }

    private sealed class CapturingHttpMessageHandler(Func<HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string? AuthorizationScheme { get; private set; }
        public string? AuthorizationParameter { get; private set; }
        public string? Accept { get; private set; }
        public string? RequestBody { get; private set; }
        public int CallCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Method = request.Method;
            RequestUri = request.RequestUri;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            Accept = request.Headers.Accept.SingleOrDefault()?.MediaType;
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return responseFactory();
        }
    }
}
