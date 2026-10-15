// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.Services;

public sealed class DataBoundaryResolverTests
{
    [Theory]
    [InlineData("EU", "eu")]
    [InlineData("Global", "row")]
    public async Task ResolveAsync_MapsArmBoundaryAndCachesIt(string armValue, string expected)
    {
        var handler = new SequenceHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $"{{\"properties\":{{\"dataBoundary\":\"{armValue}\",\"provisioningState\":\"Succeeded\"}}}}"),
            });
        using var client = new HttpClient(handler);
        using var resolver = new DataBoundaryResolver(
            client,
            Cloud(AzureCloudConfiguration.AzureCloud.AzurePublicCloud),
            NullLogger<DataBoundaryResolver>.Instance);

        Assert.Equal(expected, await resolver.ResolveAsync(CancellationToken.None));
        Assert.Equal(expected, await resolver.ResolveAsync(CancellationToken.None));
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ResolveAsync_RetriesTransientFailure()
    {
        var handler = new SequenceHandler(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"properties":{"dataBoundary":"Global","provisioningState":"Succeeded"}}"""),
            });
        using var client = new HttpClient(handler);
        using var resolver = new DataBoundaryResolver(
            client,
            Cloud(AzureCloudConfiguration.AzureCloud.AzurePublicCloud),
            NullLogger<DataBoundaryResolver>.Instance);

        Assert.Equal("row", await resolver.ResolveAsync(CancellationToken.None));
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task ResolveAsync_FailsClosedForUnknownBoundary()
    {
        var handler = new SequenceHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"properties":{"dataBoundary":"Unknown","provisioningState":"Succeeded"}}"""),
            });
        using var client = new HttpClient(handler);
        using var resolver = new DataBoundaryResolver(
            client,
            Cloud(AzureCloudConfiguration.AzureCloud.AzurePublicCloud),
            NullLogger<DataBoundaryResolver>.Instance);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => resolver.ResolveAsync(CancellationToken.None));

        Assert.Contains("could not prepare", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(AzureCloudConfiguration.AzureCloud.AzureChinaCloud)]
    [InlineData(AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud)]
    public async Task ResolveAsync_RejectsSovereignCloudBeforeNetworkCall(
        AzureCloudConfiguration.AzureCloud cloud)
    {
        var handler = new SequenceHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"properties":{"dataBoundary":"Global","provisioningState":"Succeeded"}}"""),
            });
        using var client = new HttpClient(handler);
        using var resolver = new DataBoundaryResolver(
            client,
            Cloud(cloud),
            NullLogger<DataBoundaryResolver>.Instance);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => resolver.ResolveAsync(CancellationToken.None));

        Assert.Contains("supported only in Azure public cloud", exception.Message, StringComparison.Ordinal);
        Assert.Contains(cloud.ToString(), exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, handler.CallCount);
    }

    private static IAzureCloudConfiguration Cloud(AzureCloudConfiguration.AzureCloud cloud)
    {
        IAzureCloudConfiguration configuration = Substitute.For<IAzureCloudConfiguration>();
        configuration.CloudType.Returns(cloud);
        return configuration;
    }

    private sealed class SequenceHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
