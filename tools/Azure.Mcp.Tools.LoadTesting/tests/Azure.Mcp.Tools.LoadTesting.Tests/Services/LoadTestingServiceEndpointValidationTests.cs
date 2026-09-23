// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.LoadTesting.Services;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.LoadTesting.Tests.Services;

public class LoadTestingServiceEndpointValidationTests
{
    [Theory]
    [InlineData(
        "00000000-0000-0000-0000-000000000000.eastus.cnt-prod.loadtesting.azure.com",
        "https://00000000-0000-0000-0000-000000000000.eastus.cnt-prod.loadtesting.azure.com/")]
    public void CreateValidatedDataPlaneUri_PublicCloudHost_ReturnsEndpoint(
        string dataPlaneUri,
        string expectedEndpoint)
    {
        var endpoint = LoadTestingService.CreateValidatedDataPlaneUri(
            dataPlaneUri,
            ArmEnvironment.AzurePublicCloud);

        Assert.Equal(expectedEndpoint, endpoint.AbsoluteUri);
    }

    [Theory]
    [InlineData(
        "00000000-0000-0000-0000-000000000000.region.cnt-prod.loadtesting.azure.us",
        "https://00000000-0000-0000-0000-000000000000.region.cnt-prod.loadtesting.azure.us/")]
    public void CreateValidatedDataPlaneUri_GovernmentCloudHost_ReturnsEndpoint(
        string dataPlaneUri,
        string expectedEndpoint)
    {
        var endpoint = LoadTestingService.CreateValidatedDataPlaneUri(
            dataPlaneUri,
            ArmEnvironment.AzureGovernment);

        Assert.Equal(expectedEndpoint, endpoint.AbsoluteUri);
    }

    [Theory]
    [InlineData("evil.example")]
    [InlineData("resource.eastus.cnt-prod.loadtesting.azure.com.evil.example")]
    [InlineData("resource.region.cnt-prod.loadtesting.azure.us")]
    [InlineData("127.0.0.1")]
    public void CreateValidatedDataPlaneUri_InvalidPublicCloudHost_ThrowsSecurityException(string dataPlaneUri)
    {
        Assert.Throws<SecurityException>(() =>
            LoadTestingService.CreateValidatedDataPlaneUri(
                dataPlaneUri,
                ArmEnvironment.AzurePublicCloud));
    }

    [Theory]
    [InlineData("http://127.0.0.1/health")]
    [InlineData("http://169.254.169.254/latest/meta-data")]
    [InlineData("file:///etc/hosts")]
    [InlineData("not-a-valid-uri")]
    public async Task CreateTestAsync_UnsafeTarget_ThrowsBeforeAzureAccess(string endpointUrl)
    {
        var service = new LoadTestingService(
            Substitute.For<IAzureService>(),
            Substitute.For<ILogger<LoadTestingService>>());

        await Assert.ThrowsAsync<SecurityException>(() =>
            service.CreateTestAsync(
                "subscription",
                "test-resource",
                "test-id",
                endpointUrl: endpointUrl,
                cancellationToken: TestContext.Current.CancellationToken));
    }
}
