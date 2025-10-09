// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Tables.LiveTests
{
    public class TablesCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
    {
        [Fact]
        public async Task Should_list_storage_tables_with_tenant_id()
        {
            var result = await CallToolAsync(
                "azmcp_tables_list",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "tenant", Settings.TenantId },
                { "storage-account", Settings.ResourceBaseName },
                });

            var actual = result.AssertProperty("tables");
            Assert.Equal(JsonValueKind.Array, actual.ValueKind);
            Assert.NotEmpty(actual.EnumerateArray());
        }

        [Fact()]
        public async Task Should_list_storage_tables_with_tenant_name()
        {
            Assert.SkipWhen(Settings.IsServicePrincipal, TenantNameReason);

            var result = await CallToolAsync(
                "azmcp_tables_list",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "tenant", Settings.TenantName },
                { "storage-account", Settings.ResourceBaseName },
                });

            var actual = result.AssertProperty("tables");
            Assert.Equal(JsonValueKind.Array, actual.ValueKind);
            Assert.NotEmpty(actual.EnumerateArray());
        }
    }
}
