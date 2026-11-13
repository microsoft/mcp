// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.Tests;

public sealed class KustoEndpointTests
{
    [Fact]
    public void Create_CustomCloud_UsesConfiguredSuffixAndScope()
    {
        var cloudConfiguration = CreateCustomCloudConfiguration(
            ".kusto.windows.net",
            "https://custom-kusto.contoso.example");

        var endpoint = KustoEndpoint.Create(
            "https://MyCluster.Kusto.Windows.Net/",
            cloudConfiguration);

        Assert.Equal("https://mycluster.kusto.windows.net", endpoint.ClusterUri);
        Assert.Equal("https://custom-kusto.contoso.example/.default", endpoint.Scope);
    }

    [Fact]
    public void Create_CustomCloudWithoutKustoMetadata_ThrowsInvalidOperationException()
    {
        var cloudConfiguration = CreateCustomCloudConfiguration(null, null);

        var exception = Assert.Throws<InvalidOperationException>(
            () => KustoEndpoint.Create("https://mycluster.kusto.windows.net", cloudConfiguration));

        Assert.Contains("kusto capability", exception.Message);
    }

    [Fact]
    public void Create_CustomCloud_RejectsBuiltInHostOutsideConfiguredSuffix()
    {
        var cloudConfiguration = CreateCustomCloudConfiguration(
            ".kusto.contoso.example",
            "https://kusto.contoso.example");

        Assert.Throws<ArgumentException>(
            () => KustoEndpoint.Create("https://mycluster.kusto.windows.net", cloudConfiguration));
    }

    [Theory]
    [InlineData("https://kusto.contoso.example")]
    [InlineData("https://notkusto.contoso.example")]
    [InlineData("http://cluster.kusto.contoso.example")]
    [InlineData("https://cluster.kusto.contoso.example:443")]
    [InlineData("https://cluster.kusto.contoso.example/path")]
    [InlineData("https://cluster.kusto.contoso.example?query=value")]
    [InlineData("https://cluster.kusto.contoso.example#fragment")]
    [InlineData("https://user@cluster.kusto.contoso.example")]
    [InlineData("https://cluster.kusto.contoso.example.")]
    [InlineData("https://169.254.169.254")]
    public void Create_CustomCloud_RejectsInvalidOrUntrustedUris(string clusterUri)
    {
        var cloudConfiguration = CreateCustomCloudConfiguration(
            ".kusto.contoso.example",
            "https://kusto.contoso.example");

        Assert.Throws<ArgumentException>(
            () => KustoEndpoint.Create(clusterUri, cloudConfiguration));
    }

    [Fact]
    public void Create_BuiltInCloud_IgnoresCustomMetadata()
    {
        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzurePublicCloud);
        cloudConfiguration.Kusto.Returns(
            new KustoCloudConfiguration(
                ".kusto.contoso.example",
                "https://kusto.contoso.example"));

        var endpoint = KustoEndpoint.Create(
            "https://mycluster.kusto.windows.net",
            cloudConfiguration);

        Assert.Equal("https://kusto.kusto.windows.net/.default", endpoint.Scope);
    }

    private static IAzureCloudConfiguration CreateCustomCloudConfiguration(
        string? endpointSuffix,
        string? audience)
    {
        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(AzureCloudConfiguration.AzureCloud.CustomCloud);
        cloudConfiguration.Kusto.Returns(
            endpointSuffix == null || audience == null
                ? null
                : new KustoCloudConfiguration(endpointSuffix, audience));
        return cloudConfiguration;
    }
}
