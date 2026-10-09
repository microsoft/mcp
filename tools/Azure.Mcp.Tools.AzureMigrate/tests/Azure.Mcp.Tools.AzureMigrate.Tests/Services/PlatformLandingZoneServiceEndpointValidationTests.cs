// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using System.Text.Json;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureMigrate.Tests.Services;

public sealed class PlatformLandingZoneServiceEndpointValidationTests()
{
    // Note: only deterministic, DNS-independent rejection cases are asserted here. The validator performs
    // live DNS resolution for public hostnames (failing closed), which would make "allowed" assertions
    // network-dependent. Private IP literals, reserved hostnames, and disallowed schemes are rejected before any
    // DNS lookup, so they are stable in offline test environments.
    [Theory]
    [InlineData("{\"sasUrl\":\"http://169.254.169.254/metadata/instance\"}")] // cloud metadata endpoint
    [InlineData("{\"sasUrl\":\"http://localhost/lz.zip\"}")]
    [InlineData("{\"sasUrl\":\"https://127.0.0.1/lz.zip\"}")]
    [InlineData("{\"sasUrl\":\"https://10.0.0.5/lz.zip\"}")]
    [InlineData("{\"sasUrl\":\"https://192.168.1.10/lz.zip\"}")]
    [InlineData("{\"sasUrl\":\"https://172.16.0.1/lz.zip\"}")]
    [InlineData("{\"sasUrl\":\"ftp://example.com/lz.zip\"}")] // non-HTTP(S) scheme
    [InlineData("{\"properties\":{\"sasUrl\":\"http://169.254.169.254/latest/meta-data\"}}")] // nested properties path
    [InlineData("{\"properties\":{\"sasUrl\":\"https://127.0.0.1/lz.zip\"}}")]
    public void TryGetValidatedDownloadUrl_InternalOrUnsafeUrl_ThrowsSecurityException(string response)
    {
        Assert.Throws<SecurityException>(() =>
            PlatformLandingZoneService.TryGetValidatedDownloadUrl(response, logger: null));
    }

    [Theory]
    [InlineData("{}")] // no download URL yet
    [InlineData("{\"state\":\"Generating\"}")] // unrelated payload
    [InlineData("{\"sasUrl\":null}")]
    public void TryGetValidatedDownloadUrl_NoDownloadUrl_ReturnsNull(string response)
    {
        Assert.Null(PlatformLandingZoneService.TryGetValidatedDownloadUrl(response, logger: null));
    }

    [Theory]
    [InlineData("{\"sasUrl\":\"https://8.8.8.8/output.zip?sig=Sanitized\"}")]
    [InlineData("{\"properties\":{\"sasUrl\":\"https://8.8.8.8/output.zip?sig=Sanitized\"}}")]
    public void TryGetValidatedDownloadUrl_PublicAddress_ReturnsUrl(string response)
    {
        Assert.Equal("https://8.8.8.8/output.zip?sig=Sanitized",
            PlatformLandingZoneService.TryGetValidatedDownloadUrl(response, logger: null));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("https://8.8.8.8/output.zip")]
    public void TryGetValidatedDownloadUrl_MalformedResponse_ThrowsJsonException(string response)
    {
        Assert.ThrowsAny<JsonException>(() =>
            PlatformLandingZoneService.TryGetValidatedDownloadUrl(response, logger: null));
    }
}
