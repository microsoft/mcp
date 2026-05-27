// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Data.AppConfiguration;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Mcp.Core.Options;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.Tests.Services;

public class AppConfigServiceTests
{
    [Fact]
    public void CreateConfigurationClientOptions_AppliesRetrySettings()
    {
        var retryPolicy = new RetryPolicyOptions
        {
            DelaySeconds = 2,
            MaxDelaySeconds = 10,
            MaxRetries = 4,
            Mode = RetryMode.Fixed,
            NetworkTimeoutSeconds = 15
        };
        using var httpClient = new HttpClient();
        var options = AppConfigService.CreateConfigurationClientOptions(
            AppConfigurationAudience.AzurePublicCloud,
            retryPolicy,
            httpClient,
            new Uri("https://example.azconfig.io"));

        Assert.Equal(AppConfigurationAudience.AzurePublicCloud, options.Audience);
        Assert.Equal(TimeSpan.FromSeconds(2), options.Retry.Delay);
        Assert.Equal(TimeSpan.FromSeconds(10), options.Retry.MaxDelay);
        Assert.Equal(4, options.Retry.MaxRetries);
        Assert.Equal(RetryMode.Fixed, options.Retry.Mode);
        Assert.Equal(TimeSpan.FromSeconds(15), options.Retry.NetworkTimeout);
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(204, false)]
    public void KeyValueExistedFromDeleteStatus_MapsKnownStatuses(int statusCode, bool expected)
    {
        Assert.Equal(expected, AppConfigService.KeyValueExistedFromDeleteStatus(statusCode));
    }
}
