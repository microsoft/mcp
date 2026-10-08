// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.IoTHub.Services;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.Tests.Services;

public class IoTHubRoutingServiceTests()
{
    [Fact]
    public async Task GetRoutingEndpointsHealthAsync_FollowsNextLink()
    {
        const string nextLink = "https://management.azure.com/next-health-page?api-version=2023-06-30";
        using var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $"{{\"value\":[{{\"endpointId\":\"id1\",\"healthStatus\":\"healthy\"}}],\"nextLink\":\"{nextLink}\"}}")
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"value":[{"endpointId":"id2","healthStatus":"unhealthy"}]}""")
            });
        var service = CreateService(handler);

        var result = await service.GetRoutingEndpointsHealthAsync(
            new ResourceIdentifier(
                "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/IotHubs/hub1"),
            tenant: null,
            TestContext.Current.CancellationToken);

        Assert.Collection(
            result,
            item =>
            {
                Assert.Equal("id1", item.EndpointId);
                Assert.Equal("healthy", item.HealthStatus);
            },
            item =>
            {
                Assert.Equal("id2", item.EndpointId);
                Assert.Equal("unhealthy", item.HealthStatus);
            });
        Assert.Equal(2, handler.RequestUris.Count);
        Assert.Equal(nextLink, handler.RequestUris[1]?.ToString());
    }

    [Fact]
    public void BuildMonitorMetricsOptions_AppliesRequestedIntervalAndWindow()
    {
        var startTime = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
        var window = new ObservationWindow(startTime, startTime.AddHours(2));
        var interval = TimeSpan.FromMinutes(15);

        var options = IoTHubRoutingService.BuildMonitorMetricsOptions(
            "Transactions",
            "Microsoft.Storage/storageAccounts",
            window,
            interval,
            "Total",
            "ResponseType eq '*'");

        Assert.Equal("Transactions", options.Metricnames);
        Assert.Equal("Microsoft.Storage/storageAccounts", options.Metricnamespace);
        Assert.Equal(window.Timespan, options.Timespan);
        Assert.Equal(interval, options.Interval);
        Assert.Equal("Total", options.Aggregation);
        Assert.Equal("ResponseType eq '*'", options.Filter);
        Assert.Equal(IoTHubRoutingService.MetricSeriesLimit, options.Top);
    }

    [Fact]
    public void BuildMonitorMetricsOptions_OmitsFilterWhenNotProvided()
    {
        var startTime = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
        var window = new ObservationWindow(startTime, startTime.AddHours(6));
        var interval = TimeSpan.FromHours(6);

        var options = IoTHubRoutingService.BuildMonitorMetricsOptions(
            "SuccessfulRequests,ServerErrors",
            "Microsoft.EventHub/namespaces",
            window,
            interval,
            "Total");

        Assert.Equal(interval, options.Interval);
        Assert.Null(options.Filter);
        Assert.Null(options.Top);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "ResourceNotFound")]
    [InlineData(HttpStatusCode.BadRequest, "ParentResourceNotFound")]
    [InlineData(HttpStatusCode.BadRequest, "EntityNotFound")]
    [InlineData(HttpStatusCode.BadRequest, "MessagingEntityNotFound")]
    [InlineData(HttpStatusCode.BadRequest, "ContainerNotFound")]
    [InlineData(HttpStatusCode.BadRequest, "NotFound")]
    [InlineData(HttpStatusCode.NotFound, "Unrecognized")]
    public void IsResourceNotFoundResponse_RecognizesAzureNotFoundResponses(
        HttpStatusCode statusCode,
        string errorCode)
    {
        var responseContent =
            $"{{\"error\":{{\"code\":\"{errorCode}\",\"message\":\"The resource does not exist.\"}}}}";

        Assert.True(IoTHubRoutingService.IsResourceNotFoundResponse(statusCode, responseContent));
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, """{"error":{"code":"AuthorizationFailed"}}""")]
    [InlineData(HttpStatusCode.BadRequest, """{"error":{"code":"InvalidRequest"}}""")]
    [InlineData(HttpStatusCode.InternalServerError, "not-json")]
    public void IsResourceNotFoundResponse_RejectsOtherResponses(
        HttpStatusCode statusCode,
        string responseContent)
    {
        Assert.False(IoTHubRoutingService.IsResourceNotFoundResponse(statusCode, responseContent));
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task GetRoutingEndpointsHealthAsync_RetriesTransientFailures(HttpStatusCode statusCode)
    {
        var retryResponse = new HttpResponseMessage(statusCode) { Content = new StringContent("{}") };
        retryResponse.Headers.Add("x-ms-retry-after-ms", "1");
        using var handler = new SequenceHttpMessageHandler(
            retryResponse,
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"value":[]}""") });

        var result = await CreateService(handler).GetRoutingEndpointsHealthAsync(
            new ResourceIdentifier("/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/IotHubs/hub1"),
            null,
            TestContext.Current.CancellationToken);

        Assert.Empty(result);
        Assert.Equal(2, handler.RequestUris.Count);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task GetRoutingEndpointsHealthAsync_DoesNotRetryPermanentFailures(HttpStatusCode statusCode)
    {
        using var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(statusCode) { Content = new StringContent("{}") });

        var error = await Assert.ThrowsAsync<RequestFailedException>(() =>
            CreateService(handler).GetRoutingEndpointsHealthAsync(
                new ResourceIdentifier("/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/IotHubs/hub1"),
                null,
                TestContext.Current.CancellationToken));

        Assert.Equal((int)statusCode, error.Status);
        Assert.Single(handler.RequestUris);
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_ReportsExpiredBudget()
    {
        var error = await Assert.ThrowsAsync<TimeoutException>(() =>
            IoTHubRoutingService.ExecuteWithTimeoutAsync(
                async token =>
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, token);
                    return 0;
                },
                "routing test",
                TestContext.Current.CancellationToken,
                TimeSpan.FromMilliseconds(20)));

        Assert.Contains("routing test", error.Message);
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_PreservesCallerCancellation()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            IoTHubRoutingService.ExecuteWithTimeoutAsync(
                async token =>
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, token);
                    return 0;
                },
                "routing test",
                cancellation.Token));
    }

    [Fact]
    public async Task GetRoutingEndpointsHealthAsync_UsesConfiguredCloudAndTenant()
    {
        var azureService = Substitute.For<IAzureService>();
        var credential = Substitute.For<TokenCredential>();
        using var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"value":[]}""") });
        var service = CreateService(handler, azureService, ArmEnvironment.AzureGovernment, credential);
        azureService.ResolveTenantIdAsync("tenant-alias", Arg.Any<CancellationToken>()).Returns("tenant-id");

        await service.GetRoutingEndpointsHealthAsync(
            new ResourceIdentifier("/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/IotHubs/hub1"),
            "tenant-alias",
            TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.RequestUris);
        Assert.Equal(ArmEnvironment.AzureGovernment.Endpoint.Host, request!.Host);
        await azureService.Received(1).GetTokenCredentialAsync("tenant-id", Arg.Any<CancellationToken>());
        await credential.Received().GetTokenAsync(
            Arg.Is<TokenRequestContext>(context =>
                context.Scopes.Length == 1 && context.Scopes[0] == ArmEnvironment.AzureGovernment.DefaultScope),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRoutingEndpointsHealthAsync_RejectsForeignNextLinkBeforeResolvingCredential()
    {
        var azureService = Substitute.For<IAzureService>();
        using var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[],"nextLink":"https://example.com/next-health-page"}""")
            });
        var service = CreateService(handler, azureService);

        await Assert.ThrowsAsync<SecurityException>(() =>
            service.GetRoutingEndpointsHealthAsync(
                new ResourceIdentifier("/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/IotHubs/hub1"),
                null,
                TestContext.Current.CancellationToken));

        Assert.Single(handler.RequestUris);
        await azureService.Received(1).GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void BuildArmRequestUri_ResolvesResourcePathAgainstArmEndpoint()
    {
        var uri = IoTHubRoutingService.BuildArmRequestUri(
            ArmEnvironment.AzureGovernment,
            "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/IotHubs/hub1/routingEndpointsHealth",
            "2023-06-30");

        Assert.Equal(Uri.UriSchemeHttps, uri.Scheme);
        Assert.Equal(ArmEnvironment.AzureGovernment.Endpoint.Host, uri.Host);
        Assert.Equal(
            "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/IotHubs/hub1/routingEndpointsHealth",
            uri.AbsolutePath);
        Assert.Equal("?api-version=2023-06-30", uri.Query);
    }

    [Fact]
    public void BuildArmRequestUri_UsesArmNextLinkAsReturned()
    {
        const string nextLink =
            "https://management.azure.com/subscriptions/sub1/next?api-version=2023-06-30&$skiptoken=abc";

        var uri = IoTHubRoutingService.BuildArmRequestUri(
            ArmEnvironment.AzurePublicCloud,
            nextLink,
            apiVersion: null);

        Assert.Equal(nextLink, uri.AbsoluteUri);
    }

    [Theory]
    [InlineData("file:///subscriptions/sub1/resourceGroups/rg1")]
    [InlineData("http://management.azure.com/subscriptions/sub1/next")]
    [InlineData("https://example.com/subscriptions/sub1/next")]
    [InlineData("https://management.usgovcloudapi.net/subscriptions/sub1/next")]
    public void BuildArmRequestUri_RejectsLinksOutsideArmEndpoint(string link)
    {
        Assert.Throws<SecurityException>(() =>
            IoTHubRoutingService.BuildArmRequestUri(
                ArmEnvironment.AzurePublicCloud,
                link,
                apiVersion: null));
    }

    private static IoTHubRoutingService CreateService(
        HttpMessageHandler handler,
        IAzureService? azureService = null,
        ArmEnvironment? environment = null,
        TokenCredential? credential = null)
    {
        azureService ??= Substitute.For<IAzureService>();
        var logger = Substitute.For<ILogger<IoTHubRoutingService>>();

        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(environment ?? ArmEnvironment.AzurePublicCloud);
        azureService.CloudConfiguration.Returns(cloudConfiguration);

        credential ??= Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("fake-token", DateTimeOffset.UtcNow.AddHours(1)));
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(credential));
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));

        return new IoTHubRoutingService(azureService, factory, logger);
    }
}
