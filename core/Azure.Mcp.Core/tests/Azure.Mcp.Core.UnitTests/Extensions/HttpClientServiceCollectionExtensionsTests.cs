// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Services.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Extensions;

public class HttpClientServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHttpClientServices_RegistersHttpClientOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHttpClientServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetService<IOptions<HttpClientOptions>>();
        Assert.NotNull(options);
    }

    [Fact]
    public void AddHttpClientServices_RegistersHttpClientFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHttpClientServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var factory = serviceProvider.GetService<IHttpClientFactory>();
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddHttpClientServices_WithConfiguration_AppliesCustomConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHttpClientServices(options =>
        {
            options.DefaultTimeout = TimeSpan.FromSeconds(45);
            options.DefaultUserAgent = "CustomAgent";
        });
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>();
        Assert.Equal(TimeSpan.FromSeconds(45), options.Value.DefaultTimeout);
        Assert.Equal("CustomAgent", options.Value.DefaultUserAgent);
    }

    [Fact]
    public void AddHttpClientServices_ReadsEnvironmentVariables()
    {
        // Arrange
        var services = new ServiceCollection();

        // Set environment variables
        Environment.SetEnvironmentVariable("HTTP_PROXY", "http://test.proxy:8080");
        Environment.SetEnvironmentVariable("NO_PROXY", "localhost");

        try
        {
            // Act
            services.AddHttpClientServices();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var options = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>();
            Assert.Equal("http://test.proxy:8080", options.Value.HttpProxy);
            Assert.Contains("localhost", options.Value.NoProxy);
        }
        finally
        {
            // Clean up
            Environment.SetEnvironmentVariable("HTTP_PROXY", null);
            Environment.SetEnvironmentVariable("NO_PROXY", null);
        }
    }
}
