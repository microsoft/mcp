// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Helpers;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Helpers;

public class CommandHelperResolverTests
{
    // --- ResolveDefaultSubscription tests ---

    [Fact]
    public void ResolveDefaultSubscription_ProfileTakesPriority()
    {
        var result = CommandHelper.ResolveDefaultSubscription("cli-sub", "env-sub");
        Assert.Equal("cli-sub", result);
    }

    [Fact]
    public void ResolveDefaultSubscription_FallsBackToEnv_WhenProfileNull()
    {
        var result = CommandHelper.ResolveDefaultSubscription(null, "env-sub");
        Assert.Equal("env-sub", result);
    }

    [Fact]
    public void ResolveDefaultSubscription_FallsBackToEnv_WhenProfileEmpty()
    {
        var result = CommandHelper.ResolveDefaultSubscription("", "env-sub");
        Assert.Equal("env-sub", result);
    }

    [Fact]
    public void ResolveDefaultSubscription_ReturnsNull_WhenBothNull()
    {
        var result = CommandHelper.ResolveDefaultSubscription(null, null);
        Assert.Null(result);
    }

    [Fact]
    public void ResolveDefaultSubscription_ReturnsEnv_WhenBothEmpty()
    {
        var result = CommandHelper.ResolveDefaultSubscription("", "");
        Assert.Equal("", result);
    }

    // --- ResolveSubscription tests ---

    [Fact]
    public void ResolveSubscription_ExplicitOptionWins()
    {
        var result = CommandHelper.ResolveSubscription("explicit-sub", "default-sub");
        Assert.Equal("explicit-sub", result);
    }

    [Fact]
    public void ResolveSubscription_FallsBackToDefault_WhenOptionNull()
    {
        var result = CommandHelper.ResolveSubscription(null, "default-sub");
        Assert.Equal("default-sub", result);
    }

    [Fact]
    public void ResolveSubscription_FallsBackToDefault_WhenOptionEmpty()
    {
        var result = CommandHelper.ResolveSubscription("", "default-sub");
        Assert.Equal("default-sub", result);
    }

    [Fact]
    public void ResolveSubscription_PlaceholderSubscription_FallsBackToDefault()
    {
        var result = CommandHelper.ResolveSubscription("Azure subscription 1", "default-sub");
        Assert.Equal("default-sub", result);
    }

    [Fact]
    public void ResolveSubscription_PlaceholderDefault_FallsBackToDefault()
    {
        var result = CommandHelper.ResolveSubscription("Some default name", "default-sub");
        Assert.Equal("default-sub", result);
    }

    [Fact]
    public void ResolveSubscription_PlaceholderWithNoDefault_ReturnsPlaceholder()
    {
        var result = CommandHelper.ResolveSubscription("Azure subscription 1", null);
        Assert.Equal("Azure subscription 1", result);
    }

    [Fact]
    public void ResolveSubscription_BothNull_ReturnsNull()
    {
        var result = CommandHelper.ResolveSubscription(null, null);
        Assert.Null(result);
    }

    [Fact]
    public void ResolveSubscription_EmptyOptionAndEmptyDefault_ReturnsEmpty()
    {
        var result = CommandHelper.ResolveSubscription("", "");
        Assert.Equal("", result);
    }
}
