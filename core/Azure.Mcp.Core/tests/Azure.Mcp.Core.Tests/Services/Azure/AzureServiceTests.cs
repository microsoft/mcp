// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Services.Azure;

public sealed class AzureServiceTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task ResolveTenantIdAsync_EmptyOrWhitespaceTenant_ReturnsNull(string? tenant)
    {
        var service = new AzureService(
            Substitute.For<ICacheService>(),
            Substitute.For<ILogger<AzureService>>(),
            Substitute.For<ISubscriptionResolver>(),
            Substitute.For<IAzureTokenCredentialProvider>(),
            Substitute.For<IHttpClientFactory>(),
            Substitute.For<IAzureCloudConfiguration>(),
            Substitute.For<IEndpointValidator>());

        var result = await service.ResolveTenantIdAsync(tenant, TestContext.Current.CancellationToken);

        Assert.Null(result);
    }
}
