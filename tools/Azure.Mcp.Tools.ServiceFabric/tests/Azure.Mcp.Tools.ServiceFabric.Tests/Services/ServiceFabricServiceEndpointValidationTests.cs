// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.ServiceFabric.Commands;
using Azure.Mcp.Tools.ServiceFabric.Models;
using Azure.Mcp.Tools.ServiceFabric.Services;
using Azure.Mcp.Tools.ServiceFabric.Tests.TestSupport;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ServiceFabric.Tests.Services;

public sealed class ServiceFabricServiceEndpointValidationTests
{
    private const string SubscriptionId = "11111111-1111-1111-1111-111111111111";

    [Theory]
    [InlineData("Public")]
    [InlineData("China")]
    [InlineData("Government")]
    public async Task Requests_UseConfiguredCloudAndEscapePathSegments(string cloud)
    {
        ArmEnvironment armEnvironment = GetArmEnvironment(cloud);
        var handler = new RecordingHttpMessageHandler(request =>
        {
            Assert.Equal("Bearer test-token", request.Headers.Authorization?.ToString());
            if (request.Method == HttpMethod.Post)
            {
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }

            return request.RequestUri!.AbsolutePath.EndsWith("/nodes", StringComparison.Ordinal)
                ? CreateListResponse("node-1")
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"id":"node-1"}""")
                };
        });
        using var client = new HttpClient(handler);
        var service = new ServiceFabricService(CreateAzureService(armEnvironment, client));
        const string pathSegment = "name/../?redirect=https://evil.example/#fragment";
        const string escapedSegment = "name%2F..%2F%3Fredirect%3Dhttps%3A%2F%2Fevil.example%2F%23fragment"; // cspell:disable-line

        List<ManagedClusterNode> nodes = await service.ListManagedClusterNodes(
            SubscriptionId, pathSegment, pathSegment, cancellationToken: TestContext.Current.CancellationToken);
        ManagedClusterNode node = await service.GetManagedClusterNode(
            SubscriptionId, pathSegment, pathSegment, pathSegment, cancellationToken: TestContext.Current.CancellationToken);
        RestartNodeResponse restart = await service.RestartManagedClusterNodes(
            SubscriptionId, pathSegment, pathSegment, pathSegment, ["node-1"],
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("node-1", Assert.Single(nodes).Id);
        Assert.Equal("node-1", node.Id);
        Assert.Equal(202, restart.StatusCode);
        Assert.Equal(3, handler.RequestUris.Count);
        Assert.All(handler.RequestUris, requestUri =>
        {
            Assert.Equal(Uri.UriSchemeHttps, requestUri.Scheme);
            Assert.Equal(armEnvironment.Endpoint.Host, requestUri.Host);
            Assert.Equal("?api-version=2024-04-01", requestUri.Query);
            Assert.Empty(requestUri.Fragment);
        });
        string clusterPath = $"/subscriptions/{SubscriptionId}/resourceGroups/{escapedSegment}/providers/Microsoft.ServiceFabric/managedClusters/{escapedSegment}";
        Assert.Equal($"{clusterPath}/nodes", handler.RequestUris[0].AbsolutePath);
        Assert.Equal($"{clusterPath}/nodes/{escapedSegment}", handler.RequestUris[1].AbsolutePath);
        Assert.Equal($"{clusterPath}/nodeTypes/{escapedSegment}/restart", handler.RequestUris[2].AbsolutePath);
    }

    [Theory]
    [InlineData("Public")]
    [InlineData("China")]
    [InlineData("Government")]
    public async Task ListManagedClusterNodes_ValidContinuation_ReturnsAllPages(string cloud)
    {
        ArmEnvironment armEnvironment = GetArmEnvironment(cloud);
        string nextLink = $"{armEnvironment.Endpoint.AbsoluteUri}subscriptions/{SubscriptionId}/resourceGroups/rg/providers/Microsoft.ServiceFabric/managedClusters/cluster/nodes?api-version=2024-04-01&$skiptoken=next"; // cspell:disable-line
        int page = 0;
        var handler = new RecordingHttpMessageHandler(request =>
        {
            Assert.Equal("Bearer test-token", request.Headers.Authorization?.ToString());
            return ++page == 1 ? CreateListResponse("node-1", nextLink) : CreateListResponse("node-2");
        });
        using var client = new HttpClient(handler);
        var service = new ServiceFabricService(CreateAzureService(armEnvironment, client));

        List<ManagedClusterNode> nodes = await service.ListManagedClusterNodes(
            SubscriptionId, "rg", "cluster", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["node-1", "node-2"], nodes.Select(node => node.Id));
        Assert.Equal(2, handler.RequestUris.Count);
        Assert.Equal(nextLink, handler.RequestUris[1].AbsoluteUri);
    }

    [Theory]
    [InlineData("https://evil.example/steal-token")]
    [InlineData("https://management.azure.com.evil.example/nodes")]
    [InlineData("https://management.azure.com@evil.example/nodes")]
    [InlineData("http://management.azure.com/nodes")]
    [InlineData("http://169.254.169.254/metadata/instance")]
    [InlineData("https://management.chinacloudapi.cn/nodes")]
    [InlineData("https://management.usgovcloudapi.net/nodes")]
    [InlineData("//evil.example/nodes")]
    [InlineData("\\\\evil.example\\nodes")]
    [InlineData("/subscriptions/next")]
    [InlineData("not-a-uri")]
    public async Task ListManagedClusterNodes_DisallowedContinuation_RejectsBeforeNextRequest(string nextLink)
    {
        var handler = new RecordingHttpMessageHandler(_ => CreateListResponse("node-1", nextLink));
        using var client = new HttpClient(handler);
        var service = new ServiceFabricService(CreateAzureService(ArmEnvironment.AzurePublicCloud, client));

        await Assert.ThrowsAsync<SecurityException>(() =>
            service.ListManagedClusterNodes(
                SubscriptionId, "rg", "cluster", cancellationToken: TestContext.Current.CancellationToken));

        Uri requestUri = Assert.Single(handler.RequestUris);
        Assert.Equal("management.azure.com", requestUri.Host);
    }

    [Theory]
    [InlineData("https://evil.example/")]
    [InlineData("https://management.azure.com.evil.example/")]
    [InlineData("http://management.azure.com/")]
    public async Task Requests_DisallowedManagementEndpoint_RejectBeforeCredentialAcquisition(string endpoint)
    {
        var armEnvironment = new ArmEnvironment(new Uri(endpoint), ArmEnvironment.AzurePublicCloud.DefaultScope);
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = new HttpClient(handler);
        IAzureService azureService = CreateAzureService(armEnvironment, client);
        var service = new ServiceFabricService(azureService);

        await Assert.ThrowsAsync<SecurityException>(() =>
            service.ListManagedClusterNodes(
                SubscriptionId, "rg", "cluster", cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<SecurityException>(() =>
            service.GetManagedClusterNode(
                SubscriptionId, "rg", "cluster", "node", cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<SecurityException>(() =>
            service.RestartManagedClusterNodes(
                SubscriptionId, "rg", "cluster", "type", ["node"],
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Empty(handler.RequestUris);
        await azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    private static IAzureService CreateAzureService(ArmEnvironment armEnvironment, HttpClient client)
    {
        IAzureCloudConfiguration cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(armEnvironment);
        TokenCredential credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
        SubscriptionResource subscription = Substitute.For<SubscriptionResource>();
        subscription.Id.Returns(SubscriptionResource.CreateResourceIdentifier(SubscriptionId));

        IAzureService azureService = Substitute.For<IAzureService>();
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        azureService.GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(subscription);
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(credential);
        azureService.GetClient().Returns(client);
        return azureService;
    }

    private static HttpResponseMessage CreateListResponse(string nodeId, string? nextLink = null) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(
                new ListNodesResponse
                {
                    Value = [new ManagedClusterNode { Id = nodeId }],
                    NextLink = nextLink
                },
                ServiceFabricJsonContext.Default.ListNodesResponse))
        };

    private static ArmEnvironment GetArmEnvironment(string cloud) => cloud switch
    {
        "Public" => ArmEnvironment.AzurePublicCloud,
        "China" => ArmEnvironment.AzureChina,
        "Government" => ArmEnvironment.AzureGovernment,
        _ => throw new ArgumentOutOfRangeException(nameof(cloud))
    };
}
