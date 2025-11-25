// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Services.Http;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Http;

public class HttpClientServiceTests
{
    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HttpClientService(null!, null!));
    }

    [Fact]
    public void DefaultClient_ReturnsConfiguredHttpClient()
    {
        // Arrange
        var options = new HttpClientOptions
        {
            DefaultTimeout = TimeSpan.FromSeconds(30),
        };
        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(options);
        using var service = new HttpClientService(optionsWrapper, null!);

        // Act
        var client = service.DefaultClient;

        // Assert
        Assert.NotNull(client);
        Assert.Equal(TimeSpan.FromSeconds(30), client.Timeout);
    }

    [Fact]
    public void CreateClient_WithBaseAddress_ReturnsClientWithBaseAddress()
    {
        // Arrange
        var options = new HttpClientOptions();
        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(options);
        using var service = new HttpClientService(optionsWrapper, null!);
        var baseAddress = new Uri("https://example.com");

        // Act
        using var client = service.CreateClient(baseAddress);

        // Assert
        Assert.NotNull(client);
        Assert.Equal(baseAddress, client.BaseAddress);
    }

    [Fact]
    public void CreateClient_WithConfigureAction_AppliesConfiguration()
    {
        // Arrange
        var options = new HttpClientOptions();
        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(options);
        using var service = new HttpClientService(optionsWrapper, null!);
        var baseAddress = new Uri("https://example.com");

        // Act
        using var client = service.CreateClient(baseAddress, c =>
        {
            c.DefaultRequestHeaders.Add("Custom-Header", "CustomValue");
        });

        // Assert
        Assert.NotNull(client);
        Assert.Equal(baseAddress, client.BaseAddress);
        Assert.True(client.DefaultRequestHeaders.Contains("Custom-Header"));
    }

    [Fact]
    public void CreateClient_WithProxyConfiguration_CreatesProxyEnabledClient()
    {
        // Arrange
        var options = new HttpClientOptions
        {
            AllProxy = "http://proxy.example.com:8080",
            NoProxy = "localhost,127.0.0.1"
        };
        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(options);
        using var service = new HttpClientService(optionsWrapper, null!);

        // Act
        using var client = service.CreateClient();

        // Assert
        Assert.NotNull(client);
        // Note: We can't easily test the proxy configuration without reflection
        // or making the handler accessible, but this verifies the client is created
    }

    [Fact]
    public void Dispose_DisposesDefaultClient()
    {
        // Arrange
        var options = new HttpClientOptions();
        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(options);
        var service = new HttpClientService(optionsWrapper, null!);
        var client = service.DefaultClient; // Force creation

        // Act
        service.Dispose();

        // Assert
        Assert.Throws<ObjectDisposedException>(() => service.CreateClient());
    }

    [Fact]
    public void UserAgent_IsSetCorrectly()
    {
        // Arrange
        var options = new HttpClientOptions();
        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(options);
        var serviceStartOptions = new ServiceStartOptions
        {
            Transport = "http"
        };
        var serviceStartOptionsWrapper = Microsoft.Extensions.Options.Options.Create(serviceStartOptions);
        var service = new HttpClientService(optionsWrapper, serviceStartOptionsWrapper);
        var client = service.DefaultClient;

        // Act
        var userAgent = client.DefaultRequestHeaders.UserAgent;

        // Assert
        Assert.Contains("azmcp-http/", userAgent.ToString());
    }

    [Fact]
    public void UserAgent_UserAgentFromHttpClientOptionsIsIgnored()
    {
        // Arrange
        var options = new HttpClientOptions();
        options.DefaultUserAgent = "CustomAgent/1.0";
        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(options);
        var serviceStartOptions = new ServiceStartOptions
        {
            Transport = "http"
        };
        var serviceStartOptionsWrapper = Microsoft.Extensions.Options.Options.Create(serviceStartOptions);
        var service = new HttpClientService(optionsWrapper, serviceStartOptionsWrapper);
        var client = service.DefaultClient;

        // Act
        var userAgent = client.DefaultRequestHeaders.UserAgent;

        // Assert
        Assert.Contains("azmcp-http/", userAgent.ToString());
    }

}
