// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.LoadTesting.Services;
using Azure.ResourceManager;
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
        Uri endpoint = LoadTestingService.CreateValidatedDataPlaneUri(AzureServiceTestHelpers.CreateAzureService(armEnvironment: ArmEnvironment.AzurePublicCloud),
            dataPlaneUri);

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
        Uri endpoint = LoadTestingService.CreateValidatedDataPlaneUri(AzureServiceTestHelpers.CreateAzureService(armEnvironment: ArmEnvironment.AzureGovernment),
            dataPlaneUri);

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
            LoadTestingService.CreateValidatedDataPlaneUri(AzureServiceTestHelpers.CreateAzureService(armEnvironment: ArmEnvironment.AzurePublicCloud),
                dataPlaneUri));
    }

    [Theory]
    [InlineData("http://127.0.0.1/health")]
    [InlineData("http://169.254.169.254/latest/meta-data")]
    [InlineData("file:///etc/hosts")]
    [InlineData("not-a-valid-uri")]
    public async Task CreateTestAsync_UnsafeTarget_ThrowsBeforeAzureAccess(string endpointUrl)
    {
        var service = new LoadTestingService(AzureServiceTestHelpers.CreateAzureService());

        await Assert.ThrowsAsync<SecurityException>(() =>
            service.CreateTestAsync(
                "subscription",
                "test-resource",
                "test-id",
                endpointUrl: endpointUrl,
                cancellationToken: TestContext.Current.CancellationToken));
    }
}
