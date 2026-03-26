// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Azure;

/// <summary>
/// Disables parallelization for tests that mutate <see cref="BaseAzureService"/> global static state
/// (e.g., <see cref="BaseAzureService.DisableRetryLimits"/>).
/// </summary>
[CollectionDefinition("BaseAzureService RetryLimits", DisableParallelization = true)]
public class BaseAzureServiceRetryLimitsCollection;

[Collection("BaseAzureService RetryLimits")]
public class BaseAzureServiceRetryLimitsTests
{
    private readonly ITenantService _tenantService = Substitute.For<ITenantService>();
    private readonly TestAzureService _azureService;

    public BaseAzureServiceRetryLimitsTests()
    {
        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        cloudConfig.AuthorityHost.Returns(new Uri("https://login.microsoftonline.com"));
        _tenantService.CloudConfiguration.Returns(cloudConfig);

        _azureService = new TestAzureService(_tenantService);
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
                HasMaxRetries = true,
                DelaySeconds = 200,
                HasDelaySeconds = true,
                MaxDelaySeconds = 500,
                HasMaxDelaySeconds = true,
                NetworkTimeoutSeconds = 1000,
                HasNetworkTimeoutSeconds = true
            };
            var clientOptions = new ArmClientOptions();

            // Act
            _azureService.ConfigureRetryPolicyPublic(clientOptions, retryPolicy);

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
                HasDelaySeconds = true,
                MaxDelaySeconds = 0.005,
                HasMaxDelaySeconds = true
            };
            var clientOptions = new ArmClientOptions();

            // Act
            _azureService.ConfigureRetryPolicyPublic(clientOptions, retryPolicy);

            // Assert - values below normal min should pass through
            Assert.Equal(TimeSpan.FromSeconds(0.001), clientOptions.Retry.Delay);
            Assert.Equal(TimeSpan.FromSeconds(0.005), clientOptions.Retry.MaxDelay);
        }
        finally
        {
            BaseAzureService.ResetRetryLimits();
        }
    }

    private sealed class TestAzureService(ITenantService tenantService) : BaseAzureService(tenantService)
    {
        public T ConfigureRetryPolicyPublic<T>(T clientOptions, RetryPolicyOptions? retryPolicy) where T : ClientOptions =>
            ConfigureRetryPolicy(clientOptions, retryPolicy);
    }
}
