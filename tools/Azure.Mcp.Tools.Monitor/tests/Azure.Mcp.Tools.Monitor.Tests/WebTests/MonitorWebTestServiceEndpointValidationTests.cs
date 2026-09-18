// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tools.Monitor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.WebTests;

public class MonitorWebTestServiceEndpointValidationTests
{
    [Theory]
    [InlineData("https://8.8.8.8/health")]
    [InlineData("http://1.1.1.1/status")]
    public void CreateValidatedRequestUri_PublicTarget_ReturnsEndpoint(string endpoint)
    {
        Assert.Equal(
            endpoint,
            MonitorWebTestService.CreateValidatedRequestUri(endpoint, logger: null).AbsoluteUri);
    }

    [Theory]
    [InlineData("http://127.0.0.1/health")]
    [InlineData("http://169.254.169.254/latest/meta-data")]
    [InlineData("http://10.0.0.1/health")]
    [InlineData("file:///etc/hosts")]
    public void CreateValidatedRequestUri_PrivateOrUnsupportedTarget_ThrowsSecurityException(string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            MonitorWebTestService.CreateValidatedRequestUri(endpoint, logger: null));
    }

    [Fact]
    public void ResolveValidatedRequestUri_RetainedPrivateTarget_ThrowsSecurityException()
    {
        Assert.Throws<SecurityException>(() =>
            MonitorWebTestService.ResolveValidatedRequestUri(
                requestUrl: null,
                existingRequestUri: new Uri("http://127.0.0.1/health"),
                logger: null));
    }

    [Fact]
    public void ResolveValidatedRequestUri_RetainedPublicTarget_ReturnsExistingUri()
    {
        var existingRequestUri = new Uri("https://8.8.8.8/health");

        Assert.Same(
            existingRequestUri,
            MonitorWebTestService.ResolveValidatedRequestUri(
                requestUrl: null,
                existingRequestUri,
                logger: null));
    }
}
