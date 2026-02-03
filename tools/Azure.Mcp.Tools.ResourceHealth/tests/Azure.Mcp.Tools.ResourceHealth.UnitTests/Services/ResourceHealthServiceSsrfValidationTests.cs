// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ResourceHealth.Services;
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

    public ResourceHealthServiceSsrfValidationTests()
    {
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _tenantService = Substitute.For<ITenantService>();
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _service = new ResourceHealthService(_subscriptionService, _tenantService, _httpClientFactory);
    }

    private void SetupMocksForValidRequest(HttpResponseMessage response)
    {
        // Mock TokenCredential
        var mockCredential = Substitute.For<TokenCredential>();
        mockCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1))));

        // Mock TenantService to return the credential
        _tenantService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(mockCredential));

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

    private sealed class MockHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response = response;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
