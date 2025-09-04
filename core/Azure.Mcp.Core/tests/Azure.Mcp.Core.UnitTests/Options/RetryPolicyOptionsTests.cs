// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Options;

public class RetryPolicyOptionsTests
{
    [Fact]
    public void TestAreEqual_WithFlagsSet()
    {
        var policy1 = GetPolicy(3, RetryMode.Exponential, 1, 5, 30);
        var policy2 = GetPolicy(3, RetryMode.Exponential, 1, 5, 30);
        var policyDifferentValue = GetPolicy(123, RetryMode.Exponential, 1, 5, 30);

        Assert.True(RetryPolicyOptions.AreEqual(policy1, policy1));
        Assert.True(RetryPolicyOptions.AreEqual(policy1, policy2));
        Assert.True(RetryPolicyOptions.AreEqual(null, null));
        Assert.False(RetryPolicyOptions.AreEqual(policy1, null));
        Assert.False(RetryPolicyOptions.AreEqual(null, policy1));
        Assert.False(RetryPolicyOptions.AreEqual(policy1, policyDifferentValue));
    }

    [Fact]
    public void TestInequality_WithFlagsSet()
    {
        Assert.True(GetPolicy(3, RetryMode.Exponential, 1, 5, 30) != GetPolicy(999, RetryMode.Exponential, 1, 5, 30));
        Assert.True(GetPolicy(3, RetryMode.Exponential, 1, 5, 30) != GetPolicy(3, RetryMode.Fixed, 1, 5, 30));
        Assert.True(GetPolicy(3, RetryMode.Exponential, 1, 5, 30) != GetPolicy(3, RetryMode.Exponential, 999, 5, 30));
        Assert.True(GetPolicy(3, RetryMode.Exponential, 1, 5, 30) != GetPolicy(3, RetryMode.Exponential, 1, 999, 30));
        Assert.True(GetPolicy(3, RetryMode.Exponential, 1, 5, 30) != GetPolicy(3, RetryMode.Exponential, 1, 5, 999));
    }

    [Fact]
    public void TestEqualityOperators_WithFlagsSet()
    {
        var policy1 = GetPolicy(3, RetryMode.Exponential, 1, 5, 30);
        var policy2 = GetPolicy(3, RetryMode.Exponential, 1, 5, 30);
        Assert.True(policy1 == policy2);
        Assert.False(policy1 != policy2);
    }

    [Fact]
    public void Policies_With_DifferentValues_But_UnsetFlags_AreEqual()
    {
        // Create two policies differing in values but with no flags set (simulate deserialization or manual construction without flags)
        var p1 = new RetryPolicyOptions { MaxRetries = 3, Mode = RetryMode.Exponential, DelaySeconds = 1, MaxDelaySeconds = 5, NetworkTimeoutSeconds = 30 };
        var p2 = new RetryPolicyOptions { MaxRetries = 999, Mode = RetryMode.Fixed, DelaySeconds = 10, MaxDelaySeconds = 50, NetworkTimeoutSeconds = 0 };
        // Since no Has* flags are set, differences are ignored.
        Assert.True(RetryPolicyOptions.AreEqual(p1, p2));
        Assert.True(p1 == p2);
    }

    [Fact]
    public void Policies_With_Mismatched_Flags_NotEqual()
    {
        var pSpecified = GetPolicy(3, RetryMode.Exponential, 1, 5, 30); // flags set
        var pUnspecified = new RetryPolicyOptions { MaxRetries = 3, Mode = RetryMode.Exponential, DelaySeconds = 1, MaxDelaySeconds = 5, NetworkTimeoutSeconds = 30 }; // flags unset
        Assert.False(RetryPolicyOptions.AreEqual(pSpecified, pUnspecified));
        Assert.True(pSpecified != pUnspecified);
    }

    private static RetryPolicyOptions GetPolicy(int maxRetries, RetryMode mode, double delay, double maxDelay, double timeout)
    {
        return new RetryPolicyOptions
        {
            MaxRetries = maxRetries,
            Mode = mode,
            DelaySeconds = delay,
            MaxDelaySeconds = maxDelay,
            NetworkTimeoutSeconds = timeout,
            HasMaxRetries = true,
            HasMode = true,
            HasDelaySeconds = true,
            HasMaxDelaySeconds = true,
            HasNetworkTimeoutSeconds = true
        };
    }
}
