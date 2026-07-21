// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.Services;

public sealed class IoTHubDeviceServiceTests
{
    [Fact]
    public async Task RunQuery_WhenResponseBodyDoesNotComplete_UsesRetryNetworkTimeout()
    {
        var hubName = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "test-sub";
        var hostName = "test-hub.azure-devices.net";
        var primaryKey = Convert.ToBase64String(Encoding.UTF8.GetBytes("test-key"));

        var ioTHubService = Substitute.For<IIoTHubService>();
        ioTHubService.GetIoTHub(hubName, resourceGroup, subscription, Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([
                new IoTHubDescription(
                    "id",
                    hubName,
                    "eastus",
                    resourceGroup,
                    subscription,
                    "S1",
                    1,
                    "Active",
                    hostName)
            ]);
        ioTHubService.GetIoTHubKeys(hubName, resourceGroup, subscription, Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([
                new IoTHubKey("service", primaryKey, primaryKey, "ServiceConnect")
            ]);

        var handler = new CallbackHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new BlockingHttpContent()
        }));
        var httpClientService = Substitute.For<IHttpClientService>();
        httpClientService.CreateClient().Returns(new HttpClient(handler)
        {
            Timeout = Timeout.InfiniteTimeSpan
        });

        var service = new IoTHubDeviceService(
            ioTHubService,
            Substitute.For<IAzureTokenCredentialProvider>(),
            httpClientService,
            Substitute.For<ITenantService>(),
            Substitute.For<ICacheService>(),
            Substitute.For<ILogger<IoTHubDeviceService>>());
        var retryPolicy = new RetryPolicyOptions
        {
            HasNetworkTimeoutSeconds = true,
            NetworkTimeoutSeconds = 0.01
        };

        var exception = await Assert.ThrowsAsync<TimeoutException>(() => service.RunQuery(
            "SELECT * FROM devices",
            hubName,
            resourceGroup,
            subscription,
            100,
            null,
            retryPolicy,
            CancellationToken.None));

        Assert.Contains("query run", exception.Message, StringComparison.Ordinal);
        Assert.Contains("timed out", exception.Message, StringComparison.Ordinal);
        Assert.NotNull(handler.Request);
        Assert.True(handler.Request.Headers.TryGetValues("x-ms-max-item-count", out var maxCountValues));
        Assert.Equal("100", Assert.Single(maxCountValues));
    }
}