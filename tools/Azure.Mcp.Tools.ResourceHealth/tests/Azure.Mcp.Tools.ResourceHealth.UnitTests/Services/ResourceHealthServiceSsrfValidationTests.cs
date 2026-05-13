// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResourceHealth.UnitTests.Services;

/// <summary>
/// Tests to verify resource ID validation in ResourceHealthService.
/// These tests ensure that malicious resource IDs containing URLs are rejected.
/// Uses Azure.Core.ResourceIdentifier.Parse() for validation.
/// </summary>
public class ResourceHealthServiceSsrfValidationTests
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantService _tenantService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ResourceHealthService _service;
    private readonly ILogger<ResourceHealthService> _logger = Substitute.For<ILogger<ResourceHealthService>>();

    public ResourceHealthServiceSsrfValidationTests()
    {
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _tenantService = Substitute.For<ITenantService>();
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _service = new ResourceHealthService(_subscriptionService, _tenantService, _httpClientFactory, _logger);
    }

    private void SetupMocksForValidRequest(HttpResponseMessage response, string subscriptionId = "12345678-1234-1234-1234-123456789012")
    {
        // Mock CloudConfiguration to return a valid ArmEnvironment
        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        cloudConfig.AuthorityHost.Returns(new Uri("https://login.microsoftonline.com"));
        _tenantService.CloudConfiguration.Returns(cloudConfig);

        // Mock TokenCredential
        var mockCredential = Substitute.For<TokenCredential>();
        mockCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1))));

        // Mock TenantService to return the credential
        _tenantService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(mockCredential));

        var subscriptionResource = Substitute.For<SubscriptionResource>();
        subscriptionResource.Id.Returns(SubscriptionResource.CreateResourceIdentifier(subscriptionId));
        _subscriptionService.GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Microsoft.Mcp.Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(subscriptionResource);

        // Mock HttpClientFactory
        var mockHttpMessageHandler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(mockHttpMessageHandler);
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
    }

    [Theory]
    [InlineData("https://example.com/subscriptions/12345678-1234-1234-1234-123456789012/providers/Microsoft.ResourceHealth/availabilityStatuses/current")]
    [InlineData("http://example.com/subscriptions/12345678-1234-1234-1234-123456789012/providers/Microsoft.ResourceHealth/availabilityStatuses/current")]
    [InlineData("https://external.com/steal-token")]
    [InlineData("http://169.254.169.254/metadata/instance")] // Azure IMDS endpoint
    [InlineData("https://management.azure.com.example.com/subscriptions/test")]
    public async Task GetAvailabilityStatusAsync_RejectsFullUrls_WithUrlScheme(string maliciousResourceId)
    {
        // Act & Assert - ResourceIdentifier.Parse() throws FormatException for invalid IDs
        await Assert.ThrowsAsync<FormatException>(
            () => _service.GetAvailabilityStatusAsync(maliciousResourceId, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm")]
    [InlineData("resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm")]
    [InlineData("providers/Microsoft.Compute/virtualMachines/vm")]
    public async Task GetAvailabilityStatusAsync_RejectsResourceIds_NotStartingWithSlash(string invalidResourceId)
    {
        // Act & Assert - ResourceIdentifier.Parse() throws FormatException for invalid IDs
        await Assert.ThrowsAsync<FormatException>(
            () => _service.GetAvailabilityStatusAsync(invalidResourceId, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("/%68ttps%3A%2F%2Fexample.com")] // URL-encoded https://
    [InlineData("/%68ttp%3A%2F%2Fexample.com")]  // URL-encoded http://
    [InlineData("/subscriptions/test%3A%2F%2Fexample.com")]
    [InlineData("/subscriptions/test%2F%2Fexample.com")]
    public async Task GetAvailabilityStatusAsync_RejectsEncodedUrlSchemes(string maliciousResourceId)
    {
        // Act & Assert - ResourceIdentifier.Parse() throws FormatException for invalid IDs
        await Assert.ThrowsAsync<FormatException>(
            () => _service.GetAvailabilityStatusAsync(maliciousResourceId, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("/subscriptions\\..\\..\\example.com")]
    [InlineData("/subscriptions/test\\providers")]
    public async Task GetAvailabilityStatusAsync_RejectsBackslashes(string maliciousResourceId)
    {
        // Act & Assert - ResourceIdentifier.Parse() throws FormatException for invalid IDs
        await Assert.ThrowsAsync<FormatException>(
            () => _service.GetAvailabilityStatusAsync(maliciousResourceId, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("//example.com/path")]
    [InlineData("/https://example.com")]
    [InlineData("/http://example.com")]
    public async Task GetAvailabilityStatusAsync_RejectsEmbeddedUrls(string maliciousResourceId)
    {
        // Act & Assert - ResourceIdentifier.Parse() throws FormatException for invalid IDs
        await Assert.ThrowsAsync<FormatException>(
            () => _service.GetAvailabilityStatusAsync(maliciousResourceId, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("/random/path/not/azure")]
    [InlineData("/subscriptions/not-a-guid/resourceGroups/rg")]
    [InlineData("/subscriptions/12345678-1234-1234-1234-12345678901/resourceGroups/rg")] // Invalid GUID (too short)
    [InlineData("/subscriptions/12345678-1234-1234-1234-1234567890123/resourceGroups/rg")] // Invalid GUID (too long)
    public async Task GetAvailabilityStatusAsync_RejectsInvalidAzureResourceIdFormat(string invalidResourceId)
    {
        // Act & Assert - ResourceIdentifier.Parse() throws FormatException for invalid IDs
        await Assert.ThrowsAsync<FormatException>(
            () => _service.GetAvailabilityStatusAsync(invalidResourceId, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetAvailabilityStatusAsync_RejectsNullOrEmptyResourceId(string? invalidResourceId)
    {
        // Act & Assert - null/empty throws ArgumentException from ValidateRequiredParameters
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetAvailabilityStatusAsync(invalidResourceId!, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAvailabilityStatusAsync_RejectsWhitespaceResourceId()
    {
        // Act & Assert - whitespace passes ValidateRequiredParameters but fails ResourceIdentifier.Parse
        await Assert.ThrowsAsync<FormatException>(
            () => _service.GetAvailabilityStatusAsync("   ", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("/subscriptions/12345678-1234-1234-1234-123456789012")]
    [InlineData("/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/myResourceGroup")]
    [InlineData("/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/my-resource-group/providers/Microsoft.Compute/virtualMachines/myVM")]
    [InlineData("/subscriptions/ABCDEF12-1234-1234-1234-123456789ABC/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/mystorageaccount")]
    [InlineData("/subscriptions/abcdef12-1234-1234-1234-123456789abc/resourceGroups/rg/providers/Microsoft.Web/sites/mywebapp")]
    public async Task GetAvailabilityStatusAsync_AcceptsValidAzureResourceIds(string validResourceId)
    {
        // Arrange - mock all dependencies for a successful request
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                {
                    "id": "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm/providers/Microsoft.ResourceHealth/availabilityStatuses/current",
                    "name": "current",
                    "type": "Microsoft.ResourceHealth/availabilityStatuses",
                    "location": "eastus",
                    "properties": {
                        "availabilityState": "Available",
                        "summary": "Resource is healthy",
                        "detailedStatus": "Running normally",
                        "reasonType": "",
                        "occuredTime": "2025-01-29T00:00:00Z"
                    }
                }
                """, System.Text.Encoding.UTF8, "application/json")
        };
        SetupMocksForValidRequest(mockResponse);

        var result = await _service.GetAvailabilityStatusAsync(validResourceId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - valid resource IDs should pass validation and return a result
        Assert.NotNull(result);
        Assert.Equal("Available", result.AvailabilityState);
    }

    [Fact]
    public async Task GetAvailabilityStatusAsync_ThrowsUnprocessableEntityException_WhenRequestIsUnprocessable()
    {
        var resourceId = "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet";
        var responseContent = "{\"error\":{\"code\":\"UnsupportedResourceType\",\"message\":\"Resource type is not supported.\"}}";
        var mockResponse = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = new StringContent(responseContent)
        };
        SetupMocksForValidRequest(mockResponse);

        var exception = await Assert.ThrowsAsync<ResourceHealthUnprocessableEntityException>(
            () => _service.GetAvailabilityStatusAsync(resourceId, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(resourceId, exception.ResourceId);
        Assert.Equal("Microsoft.Network/virtualNetworks", exception.ResourceType);
        Assert.Equal("UnsupportedResourceType", exception.ErrorCode);
        Assert.Equal("Resource type is not supported.", exception.ErrorDetails);
        Assert.Equal(responseContent, exception.ResponseContent);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
    }

    [Fact]
    public async Task ListAvailabilityStatusesAsync_ThrowsRequestFailedException_WhenRequestConflicts()
    {
        const string subscriptionId = "12345678-1234-1234-1234-123456789012";
        var responseContent = "{\"error\":{\"code\":\"MissingSubscriptionRegistration\",\"message\":\"The subscription is not registered to use namespace 'Microsoft.ResourceHealth'.\"}}";
        var response = new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent(responseContent)
        };
        SetupMocksForValidRequest(response, subscriptionId);

        var exception = await Assert.ThrowsAsync<ResourceHealthRequestFailedException>(
            () => _service.ListAvailabilityStatusesAsync(subscriptionId, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        Assert.Equal("MissingSubscriptionRegistration", exception.ErrorCode);
        Assert.Equal("The subscription is not registered to use namespace 'Microsoft.ResourceHealth'.", exception.ErrorMessage);
        Assert.Equal(responseContent, exception.ResponseContent);
    }

    [Fact]
    public async Task ListServiceHealthEventsAsync_DeserializesSuccessResponseFromStream()
    {
        const string subscriptionId = "12345678-1234-1234-1234-123456789012";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamOnlyJsonContent("""
                {
                    "value": [
                        {
                            "id": "/subscriptions/12345678-1234-1234-1234-123456789012/providers/Microsoft.ResourceHealth/events/TRACK123",
                            "name": "TRACK123",
                            "type": "Microsoft.ResourceHealth/events",
                            "properties": {
                                "title": "Test event",
                                "summary": "Test summary",
                                "eventType": "ServiceIssue",
                                "status": "Active",
                                "trackingId": "TRACK123"
                            }
                        }
                    ]
                }
                """)
        };
        SetupMocksForValidRequest(response, subscriptionId);

        var result = await _service.ListServiceHealthEventsAsync(subscriptionId, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Single(result);
        Assert.Equal("TRACK123", result[0].TrackingId);
    }

    private sealed class MockHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response = response;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }

    private sealed class StreamOnlyJsonContent(string json) : HttpContent
    {
        private readonly byte[] _content = Encoding.UTF8.GetBytes(json);

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            throw new InvalidOperationException("The response content should be read as a stream without buffering to a string.");

        protected override Task<Stream> CreateContentReadStreamAsync() =>
            Task.FromResult<Stream>(new MemoryStream(_content, writable: false));

        protected override bool TryComputeLength(out long length)
        {
            length = _content.Length;
            return true;
        }
    }
}
