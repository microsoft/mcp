// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Microsoft.Mcp.Core.Services.Http;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Services.Http;

public class HttpClientFactoryConfiguratorTests
{
    [Theory]
    [InlineData("api.loganalytics.io", "https://api.loganalytics.io/v1/workspaces", true)]
    [InlineData("api.loganalytics.io", "http://api.loganalytics.io:8080/v1/workspaces", true)]
    [InlineData("api.loganalytics.io", "https://other.loganalytics.io/v1/workspaces", false)]
    [InlineData(".ods.azure.com", "https://workspace.ods.azure.com/api", true)]
    [InlineData(".ods.azure.com", "https://ods.azure.com/", true)]
    [InlineData(".ods.azure.com", "https://other.azure.com/", false)]
    [InlineData("*.ods.azure.com", "https://workspace.ods.azure.com/api", true)]
    [InlineData("10.0.0.0/8", "https://10.1.2.3:443/api", true)]
    [InlineData("10.0.0.0/8", "http://11.0.0.1/", false)]
    [InlineData("10.0.0.0/7", "https://10.5.5.5/api", true)]
    [InlineData("10.0.0.0/7", "https://11.200.1.1/api", true)]
    [InlineData("10.0.0.0/7", "https://12.1.1.1/api", false)]
    [InlineData("172.16.0.0/12", "https://172.20.1.5/", true)]
    [InlineData("172.16.0.0/12", "https://172.35.1.5/", false)]
    [InlineData("192.168.0.0/16", "https://192.168.1.1:8080/", true)]
    [InlineData("192.168.1.0/31", "https://192.168.1.0/", true)]
    [InlineData("192.168.1.0/31", "https://192.168.1.1:8080/", true)]
    [InlineData("192.168.1.0/31", "https://192.168.1.2/", false)]
    [InlineData("::1", "https://[::1]:8080/api", true)]
    [InlineData("[::1]", "https://[::1]:8080/api", true)]
    [InlineData("fe80::1", "https://[fe80::1]/", true)]
    [InlineData("localhost", "http://localhost:5000/healthz", true)]
    [InlineData("localhost:5000", "http://localhost:5000/healthz", true)]
    [InlineData("localhost:5000", "http://localhost:5001/healthz", false)]
    [InlineData("*", "https://anything.com/path", true)]
    public void ConvertGlobToRegex_MatchesExpectedUris(string pattern, string uri, bool expectedMatch)
    {
        var regex = HttpClientFactoryConfigurator.ConvertGlobToRegex(pattern);
        Assert.NotEmpty(regex);

        var isMatch = Regex.IsMatch(uri, regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        Assert.Equal(expectedMatch, isMatch);
    }

    [Fact]
    public void ConvertGlobToRegex_EmptyOrNull_ReturnsEmptyString()
    {
        Assert.Empty(HttpClientFactoryConfigurator.ConvertGlobToRegex(string.Empty));
        Assert.Empty(HttpClientFactoryConfigurator.ConvertGlobToRegex("   "));
    }
}
