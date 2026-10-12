// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Quota.Services;
using Azure.Mcp.Tools.Quota.Services.Util;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Quota.Tests.Services;

public sealed class QuotaServiceTests
{
    [Fact]
    public async Task GetAzureQuotaAsync_ReturnsNoLimitForUnsupportedProvider()
    {
        const string resourceType = "Microsoft.UnsupportedProvider/resources";
        TokenCredential credential = Substitute.For<TokenCredential>();
        IAzureService azureService = Substitute.For<IAzureService>();
        IAzureCloudConfiguration cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        azureService.ResolveTenantIdAsync(null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>(null));
        azureService.GetTokenCredentialAsync(null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(credential));
        using HttpClient httpClient = new();
        azureService.GetClient(Arg.Any<string?>()).Returns(httpClient);
        QuotaService service = new(azureService, NullLoggerFactory.Instance);

        Dictionary<string, List<UsageInfo>> result = await service.GetAzureQuotaAsync(
            [resourceType],
            "00000000-0000-0000-0000-000000000000",
            "eastus",
            TestContext.Current.CancellationToken);

        UsageInfo usage = Assert.Single(result[resourceType]);
        Assert.Equal(resourceType, usage.Name);
        Assert.Equal(0, usage.Limit);
        Assert.Equal(0, usage.Used);
        Assert.Equal("No Limit", usage.Description);
    }
}
