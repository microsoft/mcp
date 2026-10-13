// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core.Pipeline;
using Azure.Data.AppConfiguration;
using Azure.Mcp.Tools.AppConfig.Services;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.Tests.Services;

public class AppConfigServiceTests
{
    [Fact]
    public void CreateConfigurationClientOptions_ConfiguresAudienceAndTransport()
    {
        using var httpClient = new HttpClient();
        var endpoint = new Uri("https://example.azconfig.io");
        var options = AppConfigService.CreateConfigurationClientOptions(
            AppConfigurationAudience.AzurePublicCloud,
            httpClient,
            endpoint);

        Assert.Equal(AppConfigurationAudience.AzurePublicCloud, options.Audience);
        Assert.IsType<HttpClientTransport>(options.Transport);
        Assert.Equal(endpoint, httpClient.BaseAddress);
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(204, false)]
    public void KeyValueExistedFromDeleteStatus_MapsKnownStatuses(int statusCode, bool expected)
    {
        Assert.Equal(expected, AppConfigService.KeyValueExistedFromDeleteStatus(statusCode));
    }
}
