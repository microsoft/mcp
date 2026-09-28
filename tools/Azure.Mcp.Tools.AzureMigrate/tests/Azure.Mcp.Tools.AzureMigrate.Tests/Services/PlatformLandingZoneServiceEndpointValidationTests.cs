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
    [InlineData("{\"downloadUrl\":\"http://169.254.169.254/metadata/instance\"}")] // cloud metadata endpoint
    [InlineData("{\"downloadUrl\":\"http://localhost/lz.zip\"}")]
    [InlineData("{\"downloadUrl\":\"https://127.0.0.1/lz.zip\"}")]
    [InlineData("{\"downloadUrl\":\"https://10.0.0.5/lz.zip\"}")]
    [InlineData("{\"downloadUrl\":\"https://192.168.1.10/lz.zip\"}")]
    [InlineData("{\"downloadUrl\":\"https://172.16.0.1/lz.zip\"}")]
    [InlineData("{\"downloadUrl\":\"ftp://example.com/lz.zip\"}")] // non-HTTP(S) scheme
    [InlineData("{\"properties\":{\"downloadUrl\":\"http://169.254.169.254/latest/meta-data\"}}")] // nested properties path
    [InlineData("http://127.0.0.1/lz.zip")] // non-JSON raw URL fallback
    public void TryGetValidatedDownloadUrl_InternalOrUnsafeUrl_ThrowsSecurityException(string response)
    {
        Assert.Throws<SecurityException>(() =>
            PlatformLandingZoneService.TryGetValidatedDownloadUrl(response, logger: null));
    }

    [Theory]
    [InlineData("{}")] // no download URL yet
    [InlineData("{\"state\":\"Generating\"}")] // unrelated payload
    [InlineData("not json")] // parse failure with no URL to fall back to
    public void TryGetValidatedDownloadUrl_NoDownloadUrl_ReturnsNull(string response)
    {
        Assert.Null(PlatformLandingZoneService.TryGetValidatedDownloadUrl(response, logger: null));
    }
}
