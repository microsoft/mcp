// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.Tests;

public class AvmDocsServiceEndpointValidationTests
{
    [Theory]
    [InlineData(
        "https://github.com/Azure/terraform-azurerm-avm-res-storage-storageaccount",
        "https://api.github.com/repos/Azure/terraform-azurerm-avm-res-storage-storageaccount/releases")]
    [InlineData(
        "https://github.com/Azure/github.com-foo",
        "https://api.github.com/repos/Azure/github.com-foo/releases")]
    public void BuildValidatedReleasesUrl_GitHubRepo_ReturnsApiUrl(string repoUrl, string expected)
    {
        var result = AvmDocsService.BuildValidatedReleasesUrl(repoUrl);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("https://evil.com/Azure/terraform-azurerm-avm-res-storage-storageaccount")]
    [InlineData("https://raw.githubusercontent.com.evil.com/Azure/module")]
    [InlineData("https://10.0.0.1/Azure/module")]
    [InlineData("http://github.com/Azure/module")] // non-HTTPS
    public void BuildValidatedReleasesUrl_NonGitHubHost_ThrowsSecurityException(string repoUrl)
    {
        Assert.Throws<SecurityException>(() => AvmDocsService.BuildValidatedReleasesUrl(repoUrl));
    }
}
