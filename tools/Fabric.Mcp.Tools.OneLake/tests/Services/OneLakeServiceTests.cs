// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable xUnit1051 // Cancellation token warnings - this is example code

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using NSubstitute;

namespace Fabric.Mcp.Tools.OneLake.Tests.Services;

/// <summary>
/// Tests for OneLakeService using testable architecture patterns following the Fabric.Mcp.Tools.PublicApi pattern.
/// This demonstrates how the service could be tested with dependency injection and mocking.
/// 
/// Key Learning: Fabric.Mcp.Tools.PublicApi succeeds in testing because they:
/// 1. Inject abstractions (IResourceProviderService) instead of concrete implementations
/// 2. Mock the dependencies that handle external calls
/// 3. Test the business logic without hitting real Azure APIs
/// 4. Validate parameters BEFORE making external calls
/// </summary>
public class OneLakeServiceTests
{
    // Example interface that could abstract away Azure authentication
    public interface IOneLakeApiClient
    {
        Task<Stream> SendFabricApiRequestAsync(HttpMethod method, string url, string? jsonContent = null, CancellationToken cancellationToken = default);
        Task<Stream> SendOneLakeApiRequestAsync(HttpMethod method, string url, string? jsonContent = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> SendDataPlaneRequestAsync(HttpMethod method, string url, CancellationToken cancellationToken = default);
    }

    // Example testable service following the PublicApi pattern
    public class TestableOneLakeService
    {
        private readonly IOneLakeApiClient _apiClient;

        public TestableOneLakeService(IOneLakeApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task<IEnumerable<Workspace>> ListOneLakeWorkspacesAsync(string? continuationToken = null, CancellationToken cancellationToken = default)
        {
            // Build URL with optional continuation token
            var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/?comp=list";
            if (!string.IsNullOrEmpty(continuationToken))
            {
                // Parameter validation - ensure continuation token is properly escaped
                if (continuationToken.Length > 1000) // Example validation
                    throw new ArgumentException("Continuation token is too long.", nameof(continuationToken));
                
                url += $"&continuationToken={Uri.EscapeDataString(continuationToken)}";
            }

            var response = await _apiClient.SendOneLakeApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
            
            // In real implementation, would parse XML response and return workspaces
            // For this example, return mock data based on whether continuation token was provided
            var mockWorkspaces = new List<Workspace>
            {
                new() { Id = "workspace1", DisplayName = "Test Workspace 1" },
                new() { Id = "workspace2", DisplayName = "Test Workspace 2" }
            };

            return mockWorkspaces;
        }
    }

    #region ListOneLakeWorkspacesAsync Tests

    [Fact]
    public async Task ListOneLakeWorkspacesAsync_WithoutContinuationToken_CallsCorrectUrlAndReturnsWorkspaces()
    {
        // Arrange
    using var mockResponse = new MemoryStream();
        var mockApiClient = Substitute.For<IOneLakeApiClient>();
        mockApiClient.SendOneLakeApiRequestAsync(
            HttpMethod.Get,
            $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/?comp=list",
            null,
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        var service = new TestableOneLakeService(mockApiClient);

        // Act
        var result = await service.ListOneLakeWorkspacesAsync();

        // Assert
        Assert.NotNull(result);
        var workspaces = result.ToList();
        Assert.Equal(2, workspaces.Count);
        Assert.Equal("workspace1", workspaces[0].Id);
        Assert.Equal("Test Workspace 1", workspaces[0].DisplayName);

        // Verify correct API call was made
        await mockApiClient.Received(1).SendOneLakeApiRequestAsync(
            HttpMethod.Get,
            $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/?comp=list",
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListOneLakeWorkspacesAsync_WithContinuationToken_IncludesTokenInUrl()
    {
        // Arrange
        var continuationToken = "token123";
        var expectedUrl = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/?comp=list&continuationToken={Uri.EscapeDataString(continuationToken)}";
    using var mockResponse = new MemoryStream();
        
        var mockApiClient = Substitute.For<IOneLakeApiClient>();
        mockApiClient.SendOneLakeApiRequestAsync(
            HttpMethod.Get,
            expectedUrl,
            null,
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        var service = new TestableOneLakeService(mockApiClient);

        // Act
        var result = await service.ListOneLakeWorkspacesAsync(continuationToken);

        // Assert
        Assert.NotNull(result);
        var workspaces = result.ToList();
        Assert.Equal(2, workspaces.Count);

        // Verify correct URL with continuation token was called
        await mockApiClient.Received(1).SendOneLakeApiRequestAsync(
            HttpMethod.Get,
            expectedUrl,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListOneLakeWorkspacesAsync_WithEmptyString_TreatsAsNull()
    {
        // Arrange
    using var mockResponse = new MemoryStream();
        var mockApiClient = Substitute.For<IOneLakeApiClient>();
        mockApiClient.SendOneLakeApiRequestAsync(
            HttpMethod.Get,
            $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/?comp=list",
            null,
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        var service = new TestableOneLakeService(mockApiClient);

        // Act
        var result = await service.ListOneLakeWorkspacesAsync("");

        // Assert
        Assert.NotNull(result);

        // Verify empty string is treated as null (no continuation token in URL)
        await mockApiClient.Received(1).SendOneLakeApiRequestAsync(
            HttpMethod.Get,
            $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/?comp=list",
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListOneLakeWorkspacesAsync_WithTooLongContinuationToken_ThrowsArgumentException()
    {
        // Arrange
        var longToken = new string('x', 1001); // Exceeds 1000 character limit
        var mockApiClient = Substitute.For<IOneLakeApiClient>();
        var service = new TestableOneLakeService(mockApiClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            service.ListOneLakeWorkspacesAsync(longToken));
        Assert.Equal("Continuation token is too long. (Parameter 'continuationToken')", exception.Message);

        // Verify no API call was made due to validation failure
        await mockApiClient.DidNotReceive().SendOneLakeApiRequestAsync(
            Arg.Any<HttpMethod>(), 
            Arg.Any<string>(), 
            Arg.Any<string>(), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListOneLakeWorkspacesAsync_WithSpecialCharactersInToken_EscapesCorrectly()
    {
        // Arrange
        var specialToken = "token with spaces & symbols!";
        var expectedUrl = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/?comp=list&continuationToken={Uri.EscapeDataString(specialToken)}";
    using var mockResponse = new MemoryStream();
        
        var mockApiClient = Substitute.For<IOneLakeApiClient>();
        mockApiClient.SendOneLakeApiRequestAsync(
            HttpMethod.Get,
            expectedUrl,
            null,
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        var service = new TestableOneLakeService(mockApiClient);

        // Act
        var result = await service.ListOneLakeWorkspacesAsync(specialToken);

        // Assert
        Assert.NotNull(result);

        // Verify URL encoding was applied correctly
        await mockApiClient.Received(1).SendOneLakeApiRequestAsync(
            HttpMethod.Get,
            expectedUrl,
            null,
            Arg.Any<CancellationToken>());
    }

    #endregion

    /// <summary>
    /// This demonstrates the key architectural difference:
    /// 
    /// Current OneLakeService:
    /// - Creates DefaultAzureCredential internally
    /// - Authentication happens in service methods
    /// - Cannot mock authentication layer
    /// - Requires Azure credentials for testing
    /// 
    /// Fabric.PublicApi Service Pattern:
    /// - Injects IResourceProviderService abstraction
    /// - Validation happens before external calls
    /// - Dependencies can be mocked
    /// - Pure unit testing without external dependencies
    /// 
    /// The choice was made to keep the current OneLake architecture for production
    /// reliability and focus testing efforts on the command layer where the 
    /// business logic resides.
    /// </summary>
    [Fact]
    public void ArchitecturalPattern_DocumentationTest()
    {
        // This test exists purely to document the architectural pattern difference
        // and could be used as a starting point for future refactoring
        Assert.True(true);
    }
}