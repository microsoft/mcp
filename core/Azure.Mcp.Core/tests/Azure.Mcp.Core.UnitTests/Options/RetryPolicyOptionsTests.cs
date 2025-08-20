// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Options;

public class RetryPolicyOptionsTests
{
    [Fact]
    public void TestAreEqual()
    {
        // Arrange
        var policy1 = GetPolicy(3, RetryMode.Exponential, 1, 5, 30);
        var policy2 = GetPolicy(3, RetryMode.Exponential, 1, 5, 30);
        var policy3 = GetPolicy(123, RetryMode.Exponential, 1, 5, 30);

        // Assert
        Assert.True(RetryPolicyOptions.AreEqual(policy1, policy1));
        Assert.True(RetryPolicyOptions.AreEqual(policy1, policy2));
        Assert.True(RetryPolicyOptions.AreEqual(null, null));
        Assert.False(RetryPolicyOptions.AreEqual(policy1, null));
        Assert.False(RetryPolicyOptions.AreEqual(null, policy1));
        Assert.False(RetryPolicyOptions.AreEqual(policy1, policy3));
    }

    [Fact]
    public void TestInequality()
    {
        // Assert
        Assert.True(GetPolicy(3, RetryMode.Exponential, 1, 5, 30) != GetPolicy(999, RetryMode.Exponential, 1, 5, 30));
        Assert.True(GetPolicy(3, RetryMode.Exponential, 1, 5, 30) != GetPolicy(3, RetryMode.Fixed, 1, 5, 30));
        Assert.True(GetPolicy(3, RetryMode.Exponential, 1, 5, 30) != GetPolicy(3, RetryMode.Exponential, 999, 5, 30));
        Assert.True(GetPolicy(3, RetryMode.Exponential, 1, 5, 30) != GetPolicy(3, RetryMode.Exponential, 1, 999, 30));
        Assert.True(GetPolicy(3, RetryMode.Exponential, 1, 5, 30) != GetPolicy(3, RetryMode.Exponential, 1, 5, 999));
    }

    [Fact]
    public void TestEqualityOperators()
    {
        // Arrange
        var policy1 = GetPolicy(3, RetryMode.Exponential, 1, 5, 30);
        var policy2 = GetPolicy(3, RetryMode.Exponential, 1, 5, 30);

        // Assert
        Assert.True(policy1 == policy2);
        Assert.False(policy1 != policy2);
    }

    private static RetryPolicyOptions GetPolicy(int maxRetries, RetryMode mode, double delay, double maxDelay, double timeout)
    {
        return new RetryPolicyOptions
        {
            MaxRetries = maxRetries,
            Mode = mode,
            DelaySeconds = delay,
            MaxDelaySeconds = maxDelay,
            NetworkTimeoutSeconds = timeout
        };
    }
}
