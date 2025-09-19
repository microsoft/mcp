// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.VirtualDesktop.LiveTests;

[Trait("Area", "VirtualDesktop")]
public class VirtualDesktopCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_ListHostpools_WithSubscriptionId()
    {
        var result = await CallToolAsync(
            "azmcp_virtualdesktop_hostpool_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var hostpools = result.AssertProperty("hostpools");
        Assert.Equal(JsonValueKind.Array, hostpools.ValueKind);

        // Check results format if any hostpools exist
        foreach (var hostpool in hostpools.EnumerateArray())
        {
            Assert.Equal(JsonValueKind.Object, hostpool.ValueKind);
            var name = hostpool.GetProperty("name").GetString();
            Assert.False(string.IsNullOrEmpty(name));
            hostpool.AssertProperty("resourceGroupName");
            hostpool.AssertProperty("location");
            hostpool.AssertProperty("hostPoolType");
        }
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_ListHostpools_WithSubscriptionName()
    {
        var result = await CallToolAsync(
            "azmcp_virtualdesktop_hostpool_list",
            new()
            {
                { "subscription", Settings.SubscriptionName }
            });

        var hostpools = result.AssertProperty("hostpools");
        Assert.Equal(JsonValueKind.Array, hostpools.ValueKind);

        // Check results format if any hostpools exist
        foreach (var hostpool in hostpools.EnumerateArray())
        {
            Assert.Equal(JsonValueKind.Object, hostpool.ValueKind);
            var name = hostpool.GetProperty("name").GetString();
            Assert.False(string.IsNullOrEmpty(name));
            hostpool.AssertProperty("resourceGroupName");
            hostpool.AssertProperty("location");
            hostpool.AssertProperty("hostPoolType");
        }
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_ListHostpools_WithResourceGroup_WithSubscriptionId()
    {
        // First get all hostpools to find one with a resource group
        var allHostpoolsResult = await CallToolAsync(
            "azmcp_virtualdesktop_hostpool_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var allHostpools = allHostpoolsResult.AssertProperty("hostpools");
        if (allHostpools.GetArrayLength() > 0)
        {
            var firstHostpool = allHostpools[0];
            var resourceGroupName = firstHostpool.GetProperty("resourceGroupName").GetString()!;

            // Now test with resource group filter
            var result = await CallToolAsync(
                "azmcp_virtualdesktop_hostpool_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", resourceGroupName }
                });

            var hostpools = result.AssertProperty("hostpools");
            Assert.Equal(JsonValueKind.Array, hostpools.ValueKind);

            // All returned hostpools should be from the specified resource group
            foreach (var hostpool in hostpools.EnumerateArray())
            {
                Assert.Equal(JsonValueKind.Object, hostpool.ValueKind);
                var name = hostpool.GetProperty("name").GetString();
                Assert.False(string.IsNullOrEmpty(name));
                var hostpoolResourceGroup = hostpool.GetProperty("resourceGroupName").GetString();
                Assert.Equal(resourceGroupName, hostpoolResourceGroup);
                hostpool.AssertProperty("location");
                hostpool.AssertProperty("hostPoolType");
            }
        }
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_ListHostpools_WithResourceGroup_WithSubscriptionName()
    {
        // First get all hostpools to find one with a resource group
        var allHostpoolsResult = await CallToolAsync(
            "azmcp_virtualdesktop_hostpool_list",
            new()
            {
                { "subscription", Settings.SubscriptionName }
            });

        var allHostpools = allHostpoolsResult.AssertProperty("hostpools");
        if (allHostpools.GetArrayLength() > 0)
        {
            var firstHostpool = allHostpools[0];
            var resourceGroupName = firstHostpool.GetProperty("resourceGroupName").GetString()!;

            // Now test with resource group filter
            var result = await CallToolAsync(
                "azmcp_virtualdesktop_hostpool_list",
                new()
                {
                    { "subscription", Settings.SubscriptionName },
                    { "resource-group", resourceGroupName }
                });

            var hostpools = result.AssertProperty("hostpools");
            Assert.Equal(JsonValueKind.Array, hostpools.ValueKind);

            // All returned hostpools should be from the specified resource group
            foreach (var hostpool in hostpools.EnumerateArray())
            {
                Assert.Equal(JsonValueKind.Object, hostpool.ValueKind);
                var name = hostpool.GetProperty("name").GetString();
                Assert.False(string.IsNullOrEmpty(name));
                var hostpoolResourceGroup = hostpool.GetProperty("resourceGroupName").GetString();
                Assert.Equal(resourceGroupName, hostpoolResourceGroup);
                hostpool.AssertProperty("location");
                hostpool.AssertProperty("hostPoolType");
            }
        }
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_ListHostpools_WithNonExistentResourceGroup()
    {
        // Test with a non-existent resource group name
        var result = await CallToolAsync(
            "azmcp_virtualdesktop_hostpool_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "non-existent-resource-group-12345" }
            });

        var hostpools = result.AssertProperty("hostpools");
        Assert.Equal(JsonValueKind.Array, hostpools.ValueKind);
        Assert.Equal(0, hostpools.GetArrayLength());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_ListSessionHosts_WithSubscriptionId()
    {
        // First get available hostpools
        var hostpoolsResult = await CallToolAsync(
            "azmcp_virtualdesktop_hostpool_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var hostpools = hostpoolsResult.AssertProperty("hostpools");
        if (hostpools.GetArrayLength() > 0)
        {
            var firstHostpool = hostpools[0].GetProperty("name").GetString()!;

            var result = await CallToolAsync(
                "azmcp_virtualdesktop_hostpool_sessionhost_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "hostpool", firstHostpool }
                });

            JsonElement? sessionHosts = (result != null) ? result.AssertProperty("sessionHosts") : null;
            if (sessionHosts != null)
            {
                Assert.Equal(JsonValueKind.Array, sessionHosts.Value.ValueKind);

                // Check results format if any session hosts exist
                foreach (var sessionHost in sessionHosts.Value.EnumerateArray())
                {
                    Assert.Equal(JsonValueKind.Object, sessionHost.ValueKind);
                }
            }
        }
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_ListSessionHosts_WithSubscriptionName()
    {
        // First get available hostpools
        var hostpoolsResult = await CallToolAsync(
            "azmcp_virtualdesktop_hostpool_list",
            new()
            {
                { "subscription", Settings.SubscriptionName }
            });

        var hostpools = hostpoolsResult.AssertProperty("hostpools");
        if (hostpools.GetArrayLength() > 0)
        {
            var firstHostpool = hostpools[0].GetProperty("name").GetString()!;

            var result = await CallToolAsync(
                "azmcp_virtualdesktop_hostpool_sessionhost_list",
                new()
                {
                    { "subscription", Settings.SubscriptionName },
                    { "hostpool", firstHostpool }
                });

            JsonElement? sessionHosts = (result != null) ? result.AssertProperty("sessionHosts") : null;
            if (sessionHosts != null)
            {
                Assert.Equal(JsonValueKind.Array, sessionHosts.Value.ValueKind);

                // Check results format if any session hosts exist
                foreach (var sessionHost in sessionHosts.Value.EnumerateArray())
                {
                    Assert.Equal(JsonValueKind.Object, sessionHost.ValueKind);
                }
            }
        }
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_ListUserSessions_WithSubscriptionId()
    {
        // First get available hostpools
        var hostpoolsResult = await CallToolAsync(
            "azmcp_virtualdesktop_hostpool_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var hostpools = hostpoolsResult.AssertProperty("hostpools");
        if (hostpools.GetArrayLength() > 0)
        {
            var firstHostpool = hostpools[0].GetProperty("name").GetString()!;

            // Get session hosts for the first hostpool
            var sessionHostsResult = await CallToolAsync(
                "azmcp_virtualdesktop_hostpool_sessionhost_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "hostpool", firstHostpool }
                });

            JsonElement? sessionHosts = (sessionHostsResult != null) ? sessionHostsResult.AssertProperty("sessionHosts") : null;
            if (sessionHosts != null && sessionHosts.Value.GetArrayLength() > 0)
            {
                var firstSessionHost = sessionHosts.Value[0].GetProperty("name").GetString()!;

                var result = await CallToolAsync(
                    "azmcp_virtualdesktop_hostpool_sessionhost_usersession_list",
                    new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "hostpool", firstHostpool },
                        { "sessionhost", firstSessionHost }
                    });

                JsonElement? userSessions = (result != null) ? result.AssertProperty("userSessions") : null;
                if (userSessions != null)
                {
                    Assert.Equal(JsonValueKind.Array, userSessions.Value.ValueKind);

                    // Check results format if any user sessions exist
                    foreach (var userSession in userSessions.Value.EnumerateArray())
                    {
                        Assert.Equal(JsonValueKind.Object, userSession.ValueKind);
                        // Verify common properties exist
                        userSession.AssertProperty("name");
                        userSession.AssertProperty("hostPoolName");
                        userSession.AssertProperty("sessionHostName");
                    }
                }
            }
        }
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_ListUserSessions_WithSubscriptionName()
    {
        // First get available hostpools
        var hostpoolsResult = await CallToolAsync(
            "azmcp_virtualdesktop_hostpool_list",
            new()
            {
                { "subscription", Settings.SubscriptionName }
            });

        var hostpools = hostpoolsResult.AssertProperty("hostpools");
        if (hostpools.GetArrayLength() > 0)
        {
            var firstHostpool = hostpools[0].GetProperty("name").GetString()!;

            // Get session hosts for the first hostpool
            var sessionHostsResult = await CallToolAsync(
                "azmcp_virtualdesktop_hostpool_sessionhost_list",
                new()
                {
                    { "subscription", Settings.SubscriptionName },
                    { "hostpool", firstHostpool }
                });

            JsonElement? sessionHosts = (sessionHostsResult != null) ? sessionHostsResult.AssertProperty("sessionHosts") : null;
            if (sessionHosts != null && sessionHosts.Value.GetArrayLength() > 0)
            {
                var firstSessionHost = sessionHosts.Value[0].GetProperty("name").GetString()!;

                var result = await CallToolAsync(
                    "azmcp_virtualdesktop_hostpool_sessionhost_usersession_list",
                    new()
                    {
                        { "subscription", Settings.SubscriptionName },
                        { "hostpool", firstHostpool },
                        { "sessionhost", firstSessionHost }
                    });

                JsonElement? userSessions = (result != null) ? result.AssertProperty("userSessions") : null;
                if (userSessions != null)
                {
                    Assert.Equal(JsonValueKind.Array, userSessions.Value.ValueKind);

                    // Check results format if any user sessions exist
                    foreach (var userSession in userSessions.Value.EnumerateArray())
                    {
                        Assert.Equal(JsonValueKind.Object, userSession.ValueKind);
                        // Verify common properties exist
                        userSession.AssertProperty("name");
                        userSession.AssertProperty("hostPoolName");
                        userSession.AssertProperty("sessionHostName");
                    }
                }
            }
        }
    }
}
