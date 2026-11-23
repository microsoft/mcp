// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Services.Http;
using Xunit;

namespace Azure.Mcp.Core.Tests.Extensions;

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

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void AddHttpClientServices_ReadsEnvironmentVariables(bool lowercase, bool uppercase)
    {
        Assert.SkipWhen(lowercase && uppercase && OperatingSystem.IsWindows(),
            "Windows environment variable names are case-insensitive.");

        (string Name, string UppercaseValue, string LowercaseValue)[] variables =
        [
            ("ALL_PROXY", "http://upper.all.proxy:8080", "http://lower.all.proxy:8080"),
            ("HTTP_PROXY", "http://upper.http.proxy:8080", "http://lower.http.proxy:8080"),
            ("HTTPS_PROXY", "http://upper.https.proxy:8080", "http://lower.https.proxy:8080"),
            ("NO_PROXY", "localhost,upper.internal", "localhost,lower.internal")
        ];
        var savedVariables = variables
            .SelectMany(static variable => new[] { variable.Name, variable.Name.ToLowerInvariant() })
            .Distinct(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
            .Select(static name => (Name: name, Value: Environment.GetEnvironmentVariable(name)))
            .ToArray();

        try
        {
            // Inherited lowercase proxy settings take precedence over uppercase test values on Unix.
            foreach (var (name, _) in savedVariables)
            {
                Environment.SetEnvironmentVariable(name, null);
            }

            foreach (var variable in variables)
            {
                if (uppercase)
                {
                    Environment.SetEnvironmentVariable(variable.Name, variable.UppercaseValue);
                }
                if (lowercase)
                {
                    Environment.SetEnvironmentVariable(variable.Name.ToLowerInvariant(), variable.LowercaseValue);
                }
            }

            var services = new ServiceCollection();
            services.AddHttpClientServices();
            using var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
            string prefix = lowercase ? "lower" : "upper";
            Assert.Equal($"http://{prefix}.all.proxy:8080", options.AllProxy);
            Assert.Equal($"http://{prefix}.http.proxy:8080", options.HttpProxy);
            Assert.Equal($"http://{prefix}.https.proxy:8080", options.HttpsProxy);
            Assert.Equal($"localhost,{prefix}.internal", options.NoProxy);
        }
        finally
        {
            foreach (var (name, value) in savedVariables)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }
    }
}
