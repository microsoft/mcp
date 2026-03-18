// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Caching;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Caching;

public class CacheDurationsTests
{
    [Fact]
    public void Tenant_ShouldBeTwelveHours()
    {
        Assert.Equal(TimeSpan.FromHours(12), CacheDurations.Tenant);
    }

    [Fact]
    public void Subscription_ShouldBeTwoHours()
    {
        Assert.Equal(TimeSpan.FromHours(2), CacheDurations.Subscription);
    }

    [Fact]
    public void AuthenticatedClient_ShouldBeFifteenMinutes()
    {
        Assert.Equal(TimeSpan.FromMinutes(15), CacheDurations.AuthenticatedClient);
    }

    [Fact]
    public void ServiceData_ShouldBeFiveMinutes()
    {
        Assert.Equal(TimeSpan.FromMinutes(5), CacheDurations.ServiceData);
    }

    [Fact]
    public void Durations_ShouldBeInDescendingOrder()
    {
        Assert.True(CacheDurations.Tenant > CacheDurations.Subscription);
        Assert.True(CacheDurations.Subscription > CacheDurations.AuthenticatedClient);
        Assert.True(CacheDurations.AuthenticatedClient > CacheDurations.ServiceData);
    }
}
