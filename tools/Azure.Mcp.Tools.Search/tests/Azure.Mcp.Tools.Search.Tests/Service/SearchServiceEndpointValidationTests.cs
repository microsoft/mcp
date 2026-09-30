// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Search.Services;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Search.Tests.Service;

public sealed class SearchServiceEndpointValidationTests
{
    [Theory]
    [InlineData("Public", "https://my-search.search.windows.net/")]
    [InlineData("China", "https://my-search.search.azure.cn/")]
    [InlineData("Government", "https://my-search.search.azure.us/")]
    public void CreateAndValidateSearchEndpoint_ConfiguredCloud_ReturnsExpectedEndpoint(
        string cloud,
        string expectedEndpoint)
    {
        SearchService service = CreateService(GetCloudType(cloud), GetArmEnvironment(cloud));

        Uri endpoint = service.CreateAndValidateSearchEndpoint("my-search");

        Assert.Equal(expectedEndpoint, endpoint.AbsoluteUri);
    }

    [Theory]
    [InlineData("search.windows.net.evil.example")]
    [InlineData("search/name")]
    [InlineData("search@evil.example")]
    [InlineData("search#evil.example")]
    public void CreateAndValidateSearchEndpoint_AuthorityShapingServiceName_Throws(string serviceName)
    {
        SearchService service = CreateService(
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud,
            ArmEnvironment.AzurePublicCloud);

        Assert.Throws<ArgumentException>(() => service.CreateAndValidateSearchEndpoint(serviceName));
    }

    [Fact]
    public void CreateAndValidateSearchEndpoint_CrossCloudConfiguration_Throws()
    {
        SearchService service = CreateService(
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud,
            ArmEnvironment.AzureChina);

        Assert.Throws<System.Security.SecurityException>(() =>
            service.CreateAndValidateSearchEndpoint("my-search"));
    }

    private static SearchService CreateService(
        AzureCloudConfiguration.AzureCloud cloudType,
        ArmEnvironment armEnvironment) =>
        new(Substitute.For<ICacheService>(), CreateAzureService(cloudType, armEnvironment));

    private static IAzureService CreateAzureService(
        AzureCloudConfiguration.AzureCloud cloudType,
        ArmEnvironment armEnvironment)
    {
        IAzureCloudConfiguration cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(cloudType);
        cloudConfiguration.ArmEnvironment.Returns(armEnvironment);

        IAzureService azureService = Substitute.For<IAzureService>();
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        return azureService;
    }

    private static AzureCloudConfiguration.AzureCloud GetCloudType(string cloud) => cloud switch
    {
        "Public" => AzureCloudConfiguration.AzureCloud.AzurePublicCloud,
        "China" => AzureCloudConfiguration.AzureCloud.AzureChinaCloud,
        "Government" => AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud,
        _ => throw new ArgumentOutOfRangeException(nameof(cloud))
    };

    private static ArmEnvironment GetArmEnvironment(string cloud) => cloud switch
    {
        "Public" => ArmEnvironment.AzurePublicCloud,
        "China" => ArmEnvironment.AzureChina,
        "Government" => ArmEnvironment.AzureGovernment,
        _ => throw new ArgumentOutOfRangeException(nameof(cloud))
    };
}
