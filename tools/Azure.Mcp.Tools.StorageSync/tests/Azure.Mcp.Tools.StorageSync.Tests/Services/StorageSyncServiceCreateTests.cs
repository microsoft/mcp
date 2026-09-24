// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.StorageSync.Services;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.Tests.Services;

public class StorageSyncServiceCreateTests
{
    [Theory]
    [InlineData(false, null, "AllowVirtualNetworksOnly")]
    [InlineData(true, null, "AllowAllTraffic")]
    [InlineData(false, "AllowAllTraffic", "AllowAllTraffic")]
    [InlineData(true, "AllowVirtualNetworksOnly", "AllowVirtualNetworksOnly")]
    public async Task CreateStorageSyncService_PreservesExistingPolicy(
        bool enablePublicNetworkAccess, string? existingPolicy, string expectedPolicy)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1))));

        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);

        using var handler = new StorageSyncCreateRequestHandler(existingPolicy);
        using var client = new HttpClient(handler, disposeHandler: false);
        var azureService = Substitute.For<IAzureService>();
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        azureService.GetClient().Returns(client);
        var service = new StorageSyncService(azureService, Substitute.For<ILogger<StorageSyncService>>());

        await Assert.ThrowsAsync<RequestFailedException>(() => service.CreateStorageSyncServiceAsync(
            "11111111-1111-1111-1111-111111111111", "testrg", "test-service", "eastus",
            enablePublicNetworkAccess: enablePublicNetworkAccess,
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(1, handler.ServiceLookupCount);
        Assert.Equal(1, handler.CreateCount);
        Assert.NotNull(handler.CreateRequestBody);
        using var payload = JsonDocument.Parse(handler.CreateRequestBody);
        Assert.Equal(expectedPolicy,
            payload.RootElement.GetProperty("properties").GetProperty("incomingTrafficPolicy").GetString());
    }
}
