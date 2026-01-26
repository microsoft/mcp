// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Services.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Http;

/// <summary>
/// Integration tests demonstrating the HttpClient service functionality
/// </summary>
public class HttpClientServiceIntegrationTests
{
    [Fact]
    public void FullIntegration_WithProxyEnvironmentVariables_ConfiguresCorrectly()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ALL_PROXY", "http://proxy.example.com:8080");
        Environment.SetEnvironmentVariable("NO_PROXY", "localhost,127.0.0.1,*.local");

        try
        {
            var services = new ServiceCollection();

            // Act - Register services with environment variable configuration
            services.AddHttpClientServices(options =>
            {
                options.DefaultUserAgent = "Azure MCP Test Agent";
                options.DefaultTimeout = TimeSpan.FromSeconds(30);
            });

            var serviceProvider = services.BuildServiceProvider();
            var httpClientService = serviceProvider.GetRequiredService<IHttpClientService>();
            var options = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>();

            // Assert - Verify configuration
            Assert.Equal("http://proxy.example.com:8080", options.Value.AllProxy);
            Assert.Equal("localhost,127.0.0.1,*.local", options.Value.NoProxy);
            Assert.Equal("Azure MCP Test Agent", options.Value.DefaultUserAgent);
            Assert.Equal(TimeSpan.FromSeconds(30), options.Value.DefaultTimeout);

            // Verify client creation
            using var defaultClient = httpClientService.DefaultClient;
            using var customClient = httpClientService.CreateClient(new Uri("https://management.azure.com"));

            Assert.NotNull(defaultClient);
            Assert.NotNull(customClient);
            Assert.Equal(new Uri("https://management.azure.com"), customClient.BaseAddress);
            Assert.Equal(TimeSpan.FromSeconds(30), defaultClient.Timeout);
        }
        finally
        {
            // Clean up environment variables
            Environment.SetEnvironmentVariable("ALL_PROXY", null);
            Environment.SetEnvironmentVariable("NO_PROXY", null);
        }
    }

    [Fact]
    public void ServiceRegistration_InAzureMcpContext_WorksCorrectly()
    {
        // This demonstrates how the service would be used in the actual Azure MCP context
        // Arrange
        var services = new ServiceCollection();

        // Act - This simulates what happens in the real application
        services.AddHttpClientServices();

        // Simulate registering a service that depends on IHttpClientService
        services.AddSingleton<TestServiceUsingHttpClient>();

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var testService = serviceProvider.GetRequiredService<TestServiceUsingHttpClient>();
        Assert.NotNull(testService);

        var httpClient = testService.GetTestHttpClient();
        Assert.NotNull(httpClient);
    }

    private class TestServiceUsingHttpClient(IHttpClientService httpClientService)
    {
        private readonly IHttpClientService _httpClientService = httpClientService;

        public HttpClient GetTestHttpClient() => _httpClientService.DefaultClient;
    }
}
