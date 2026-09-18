// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureMigrate.Tests.Services;

public class PlatformLandingZoneServiceEndpointValidationTests
{
    // Note: only deterministic, DNS-independent rejection cases are asserted here. The validator performs
    // live DNS resolution for public hostnames (failing closed), which would make "allowed" assertions
    // network-dependent. IP literals, reserved hostnames, and disallowed schemes are rejected before any
    // DNS lookup, so they are stable in offline test environments.
    [Theory]
    [InlineData("http://169.254.169.254/metadata/instance")] // cloud metadata endpoint
    [InlineData("http://localhost/lz.zip")]
    [InlineData("https://127.0.0.1/lz.zip")]
    [InlineData("https://10.0.0.5/lz.zip")]
    [InlineData("https://192.168.1.10/lz.zip")]
    [InlineData("https://172.16.0.1/lz.zip")]
    [InlineData("ftp://example.com/lz.zip")] // non-HTTP(S) scheme
    public void ValidateDownloadUrl_InternalOrUnsafeUrl_ThrowsSecurityException(string url)
    {
        Assert.Throws<SecurityException>(() => PlatformLandingZoneService.ValidateDownloadUrl(url, logger: null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateDownloadUrl_EmptyUrl_ThrowsArgumentException(string url)
    {
        Assert.Throws<ArgumentException>(() => PlatformLandingZoneService.ValidateDownloadUrl(url, logger: null));
    }
}
