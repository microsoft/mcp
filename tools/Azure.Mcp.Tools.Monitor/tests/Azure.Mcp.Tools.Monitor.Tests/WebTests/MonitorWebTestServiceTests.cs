// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.ApplicationInsights;
using Azure.ResourceManager.ApplicationInsights.Mocking;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.WebTests;

public class MonitorWebTestServiceTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task GetWebTest_MissingWebTest_ThrowsKeyNotFoundException(bool hasResponse, bool hasValue)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var azureService = AzureServiceTestHelpers.CreateAzureService();
        var resourceGroup = Substitute.For<ResourceGroupResource>();
        var applicationInsights = Substitute.For<MockableApplicationInsightsResourceGroupResource>();
        var response = Substitute.For<Response<ApplicationInsightsWebTestResource>>();
        response.HasValue.Returns(hasValue);
        response.Value.Returns(Substitute.For<ApplicationInsightsWebTestResource>());
        azureService.GetResourceGroupResource("subscription", "resource-group", null, cancellationToken)
            .Returns(resourceGroup);
        resourceGroup.GetCachedClient(Arg.Any<Func<ArmClient, MockableApplicationInsightsResourceGroupResource>>())
            .Returns(applicationInsights);
        applicationInsights.GetApplicationInsightsWebTestAsync("web-test", cancellationToken)
            .Returns(hasResponse ? response : null!);
        var service = new MonitorWebTestService(azureService, NullLogger<MonitorWebTestService>.Instance);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetWebTest("subscription", "resource-group", "web-test", cancellationToken: cancellationToken));

        Assert.Contains("web-test", exception.Message);
    }
}
