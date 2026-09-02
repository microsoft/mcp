// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using Azure.Mcp.Tools.Adme.Commands.HealthCheck;
using Azure.Mcp.Tools.Adme.Commands.Schema;
using Azure.Mcp.Tools.Adme.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests;

public sealed class AdmeSetupTests
{
    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Setup_RetriesTransientResponses(HttpStatusCode transientStatus)
    {
        var requestCount = 0;
        using var serviceProvider = CreateServiceProvider(new StubHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(++requestCount == 1 ? transientStatus : HttpStatusCode.OK);
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero);
            return response;
        }));
        using var client = serviceProvider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(AdmeServiceHelper.HttpClientName);

        using var response = await client.GetAsync(
            $"{TestConstants.Endpoint}/api/schema-service/v1/schema",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, requestCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task Setup_DoesNotRetryExcludedResponse(HttpStatusCode statusCode)
    {
        var requestCount = 0;
        using var serviceProvider = CreateServiceProvider(new StubHttpMessageHandler(_ =>
        {
            requestCount++;
            return new HttpResponseMessage(statusCode);
        }));
        using var client = serviceProvider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(AdmeServiceHelper.HttpClientName);

        using var response = await client.GetAsync(
            $"{TestConstants.Endpoint}/api/schema-service/v1/schema",
            TestContext.Current.CancellationToken);

        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(1, requestCount);
    }

    [Fact]
    public async Task Setup_StopsAfterThreeRetries()
    {
        var requestCount = 0;
        using var serviceProvider = CreateServiceProvider(new StubHttpMessageHandler(_ =>
        {
            requestCount++;
            var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero);
            return response;
        }));
        using var client = serviceProvider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(AdmeServiceHelper.HttpClientName);

        using var response = await client.GetAsync(
            $"{TestConstants.Endpoint}/api/schema-service/v1/schema",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(4, requestCount);
    }

    [Fact]
    public void Setup_RegistersAndExposesCommands()
    {
        var setup = new AdmeSetup();
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IAzureTokenCredentialProvider>());
        setup.ConfigureServices(services);
        using var serviceProvider = services.BuildServiceProvider();

        var adme = setup.RegisterCommands(serviceProvider);

        Assert.Equal("adme", setup.Name);
        Assert.Equal("Azure Data Manager for Energy", setup.Title);
        Assert.Equal("adme", adme.Name);
        var health = Assert.Single(adme.SubGroup, group => group.Name == "health");
        Assert.True(health.Commands.ContainsKey("check"));
        var schema = Assert.Single(adme.SubGroup, group => group.Name == "schema");
        Assert.True(schema.Commands.ContainsKey("get"));
        Assert.True(schema.Commands.ContainsKey("list"));
        Assert.NotNull(serviceProvider.GetRequiredService<HealthCheckCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<SchemaGetCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<SchemaListCommand>());
    }

    private static ServiceProvider CreateServiceProvider(HttpMessageHandler handler)
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IAzureTokenCredentialProvider>());
        new AdmeSetup().ConfigureServices(services);
        services.AddHttpClient(AdmeServiceHelper.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        return services.BuildServiceProvider();
    }
}
