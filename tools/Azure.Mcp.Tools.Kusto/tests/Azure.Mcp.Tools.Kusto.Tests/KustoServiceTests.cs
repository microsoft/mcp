// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.Tests;

public sealed class KustoServiceTests
{
    [Fact]
    public async Task ListDatabasesAsync_CustomCloudWithoutKustoMetadata_DoesNotDiscoverCluster()
    {
        var azureService = Substitute.For<IAzureService>();
        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(AzureCloudConfiguration.AzureCloud.CustomCloud);
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        var cacheService = Substitute.For<ICacheService>();
        var service = new KustoService(azureService, cacheService);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ListDatabasesAsync(
                "00000000-0000-0000-0000-000000000001",
                "cluster",
                tenant: null,
                cancellationToken: TestContext.Current.CancellationToken));

        azureService.DidNotReceive().GetClient(Arg.Any<string>());
        await azureService.DidNotReceive()
            .GetTokenCredentialAsync(
                Arg.Any<string?>(),
                TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task QueryItemsAsync_InvalidDirectClusterUri_DoesNotAccessCacheOrCredentials()
    {
        var azureService = Substitute.For<IAzureService>();
        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(AzureCloudConfiguration.AzureCloud.CustomCloud);
        cloudConfiguration.Kusto.Returns(
            new KustoCloudConfiguration(
                ".kusto.contoso.example",
                "https://kusto.contoso.example"));
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        var cacheService = Substitute.For<ICacheService>();
        var service = new KustoService(azureService, cacheService);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.QueryItemsAsync(
                "https://attacker.example",
                "database",
                "Table | take 1",
                cancellationToken: TestContext.Current.CancellationToken));

        await cacheService.DidNotReceive()
            .GetAsync<KustoClient>(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<TimeSpan?>(),
                TestContext.Current.CancellationToken);
        await azureService.DidNotReceive()
            .GetTokenCredentialAsync(
                Arg.Any<string?>(),
                TestContext.Current.CancellationToken);
    }
}
