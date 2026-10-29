// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.IoTHub.Commands;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.Tests;

public class IoTHubValidationTests()
{
    [Theory]
    [InlineData("rg1")]
    [InlineData("a")]
    [InlineData("my-rg_(test).v2")]
    [InlineData("资源组")]
    [InlineData("\U00020000rg")]
    public void IsValidResourceGroupName_AcceptsArmResourceGroupNames(string resourceGroup)
    {
        Assert.True(IoTHubValidation.IsValidResourceGroupName(resourceGroup));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("rg1/../rg2")]
    [InlineData(@"rg1\..\rg2")]
    [InlineData("..")]
    [InlineData(".")]
    [InlineData("rg.")]
    [InlineData("rg1%2F..%2Frg2")]
    [InlineData("rg?x")]
    [InlineData("rg#x")]
    [InlineData("rg x")]
    [InlineData("rg1\n")]
    [InlineData("rg\u0301")]
    [InlineData("rg\u203F")]
    public void IsValidResourceGroupName_RejectsValuesOutsideArmNamingRules(string? resourceGroup)
    {
        Assert.False(IoTHubValidation.IsValidResourceGroupName(resourceGroup));
    }

    [Fact]
    public void IsValidResourceGroupName_EnforcesArmLengthLimit()
    {
        Assert.True(IoTHubValidation.IsValidResourceGroupName(new string('a', 90)));
        Assert.False(IoTHubValidation.IsValidResourceGroupName(new string('a', 91)));
    }
}
