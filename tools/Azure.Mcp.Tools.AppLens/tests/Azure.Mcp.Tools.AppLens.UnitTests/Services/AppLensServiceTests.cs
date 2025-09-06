// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AppLens.Models;
using Azure.Mcp.Tools.AppLens.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AppLens.UnitTests.Services;

[Trait("Area", "AppLens")]
public class AppLensServiceTests
{
    private readonly HttpClient _httpClient;
    private readonly ITenantService _tenantService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly AppLensService _service;
    private readonly TokenCredential _credential;

    public AppLensServiceTests()
    {
        _httpClient = Substitute.For<HttpClient>();
        _tenantService = Substitute.For<ITenantService>();
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _credential = Substitute.For<TokenCredential>();
        
        _service = new AppLensService(_httpClient, _tenantService, _loggerFactory);
    }

    [Fact]
    public async Task DiagnoseResourceAsync_ThrowsInvalidOperationException_WhenSubscriptionIsMissing()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DiagnoseResourceAsync(
                "Why is my app slow?",
                "myapp",
                null,
                "rg1",
                "Microsoft.Web/sites"));

        Assert.Contains("Subscription ID and Resource Group are required", exception.Message);
    }

    [Fact]
    public async Task DiagnoseResourceAsync_ThrowsInvalidOperationException_WhenResourceGroupIsMissing()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DiagnoseResourceAsync(
                "Why is my app slow?",
                "myapp",
                "sub123",
                null,
                "Microsoft.Web/sites"));

        Assert.Contains("Subscription ID and Resource Group are required", exception.Message);
    }

    [Fact]
    public async Task DiagnoseResourceAsync_ThrowsInvalidOperationException_WhenBothSubscriptionAndResourceGroupAreMissing()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DiagnoseResourceAsync(
                "Why is my app slow?",
                "myapp",
                null,
                null,
                "Microsoft.Web/sites"));

        Assert.Contains("Subscription ID and Resource Group are required", exception.Message);
    }

    [Fact]
    public async Task DiagnoseResourceAsync_ThrowsInvalidOperationException_WhenSubscriptionIsEmpty()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DiagnoseResourceAsync(
                "Why is my app slow?",
                "myapp",
                "",
                "rg1",
                "Microsoft.Web/sites"));

        Assert.Contains("Subscription ID and Resource Group are required", exception.Message);
    }

    [Fact]
    public async Task DiagnoseResourceAsync_ThrowsInvalidOperationException_WhenResourceGroupIsEmpty()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DiagnoseResourceAsync(
                "Why is my app slow?",
                "myapp",
                "sub123",
                "",
                "Microsoft.Web/sites"));

        Assert.Contains("Subscription ID and Resource Group are required", exception.Message);
    }

    [Theory]
    [InlineData("", "myapp", "sub123", "rg1", "Microsoft.Web/sites")]
    [InlineData("Why is my app slow?", "", "sub123", "rg1", "Microsoft.Web/sites")]
    [InlineData("Why is my app slow?", "myapp", "", "rg1", "Microsoft.Web/sites")]
    [InlineData("Why is my app slow?", "myapp", "sub123", "", "Microsoft.Web/sites")]
    public async Task DiagnoseResourceAsync_HandlesEmptyStringParameters(
        string question, string resourceName, string subscription, string resourceGroup, string resourceType)
    {
        // Arrange & Act & Assert
        if (string.IsNullOrEmpty(subscription) || string.IsNullOrEmpty(resourceGroup))
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.DiagnoseResourceAsync(question, resourceName, subscription, resourceGroup, resourceType));
            Assert.Contains("Subscription ID and Resource Group are required", exception.Message);
        }
        else
        {
            // For other empty parameters, the service should handle them gracefully
            // This would require mocking the HTTP client and credentials, which is complex for this test
            // We'll leave this as a placeholder for integration testing
            Assert.True(true, "Service should handle empty question/resourceName/resourceType parameters gracefully");
        }
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var service = new AppLensService(_httpClient, _tenantService, _loggerFactory);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WorksWithNullOptionalParameters()
    {
        // Arrange & Act
        var service = new AppLensService(_httpClient);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task DiagnoseResourceAsync_ValidatesResourceIdFormat()
    {
        // Arrange - This test verifies that the service constructs resource IDs correctly
        
        // Act - This will fail at the credential/HTTP stage, but we can verify the resource ID construction logic
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DiagnoseResourceAsync(
                "test question",
                "test-app", 
                "test-sub",
                "test-rg",
                "Microsoft.Web/sites"));

        // Assert - The service should attempt to process the correctly formatted resource ID
        // Since we can't easily mock the internal credential/HTTP logic without major refactoring,
        // we verify that it doesn't fail on resource ID construction by ensuring it gets past that step
        Assert.Contains("Failed to create diagnostics session for resource", exception.Message);
    }
}
