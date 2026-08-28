// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Helpers;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Services.Azure;

/// <summary>
/// Disables parallelization for tests that mutate <see cref="BaseAzureService"/> global static state
/// (e.g., <see cref="BaseAzureService.DisableRetryLimits"/>).
/// </summary>
[CollectionDefinition("BaseAzureService RetryLimits", DisableParallelization = true)]
public class BaseAzureServiceRetryLimitsCollection;

[Collection("BaseAzureService RetryLimits")]
public class BaseAzureServiceRetryLimitsTests
{
    private readonly IAzureService _azureService = Substitute.For<IAzureService>();
    private readonly TestAzureService _testAzureService;

    public BaseAzureServiceRetryLimitsTests()
    {
        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        cloudConfig.AuthorityHost.Returns(new Uri("https://login.microsoftonline.com"));
        _azureService.CloudConfiguration.Returns(cloudConfig);

        _testAzureService = new TestAzureService(_azureService);
    }

    [Fact]
    public void ConfigureRetryPolicy_DisableRetryLimits_BypassesBoundsOnAllValues()
    {
        try
        {
            // Arrange
            BaseAzureService.DisableRetryLimits();
            var retryPolicy = new RetryPolicyOptions
            {
                MaxRetries = 100,
                DelaySeconds = 200,
                MaxDelaySeconds = 500,
                NetworkTimeoutSeconds = 1000,
            };
            var clientOptions = new ArmClientOptions();

            // Act
            _testAzureService.ConfigureRetryPolicyPublic(clientOptions, retryPolicy);

            // Assert - values should pass through unclamped
            Assert.Equal(100, clientOptions.Retry.MaxRetries);
            Assert.Equal(TimeSpan.FromSeconds(200), clientOptions.Retry.Delay);
            Assert.Equal(TimeSpan.FromSeconds(500), clientOptions.Retry.MaxDelay);
            Assert.Equal(TimeSpan.FromSeconds(1000), clientOptions.Retry.NetworkTimeout);
        }
        finally
        {
            BaseAzureService.ResetRetryLimits();
        }
    }

    [Fact]
    public void ConfigureRetryPolicy_DisableRetryLimits_AllowsVerySmallDelays()
    {
        try
        {
            // Arrange
            BaseAzureService.DisableRetryLimits();
            var retryPolicy = new RetryPolicyOptions
            {
                DelaySeconds = 0.001,
                MaxDelaySeconds = 0.005,
            };
            var clientOptions = new ArmClientOptions();

            // Act
            _testAzureService.ConfigureRetryPolicyPublic(clientOptions, retryPolicy);

            // Assert - values below normal min should pass through
            Assert.Equal(TimeSpan.FromSeconds(0.001), clientOptions.Retry.Delay);
            Assert.Equal(TimeSpan.FromSeconds(0.005), clientOptions.Retry.MaxDelay);
        }
        finally
        {
            BaseAzureService.ResetRetryLimits();
        }
    }

    private sealed class TestAzureService(IAzureService azureService) : BaseAzureService(azureService)
    {
        public T ConfigureRetryPolicyPublic<T>(T clientOptions, RetryPolicyOptions? retryPolicy) where T : ClientOptions =>
            AzureHelper.ConfigureRetryPolicy(clientOptions, retryPolicy);
    }
}
