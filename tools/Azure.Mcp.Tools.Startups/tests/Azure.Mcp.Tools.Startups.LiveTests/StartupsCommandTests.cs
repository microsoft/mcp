// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Startups.LiveTests
{
    public class StartupsCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output)
        : CommandTestsBase(liveTestFixture, output), IClassFixture<LiveTestFixture>
    {
        [Fact]
        public async Task Should_get_startups_guidance()
        {
            var result = await CallToolAsync(
                "azmcp_startups_guidance",
                new());

            var guidance = result.AssertProperty("guidance");
            Assert.Equal(JsonValueKind.Object, guidance.ValueKind);

            // Verify the guidance has the expected properties
            var programInfo = guidance.GetProperty("programInfo");
            Assert.NotNull(programInfo.GetString());

            var benefits = guidance.GetProperty("benefits");
            Assert.Equal(JsonValueKind.Array, benefits.ValueKind);
            Assert.NotEmpty(benefits.EnumerateArray());
        }

        [Fact]
        public async Task Should_deploy_static_web_resources()
        {
            var result = await CallToolAsync(
                "azmcp_startups_deploy",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "storage-account", Settings.ResourceBaseName },
                    { "source-path", Settings.ResourceBaseName },
                    { "overwrite", true }
                });

            var deployment = result.AssertProperty("deployment");
            Assert.Equal(JsonValueKind.Object, deployment.ValueKind);

            // Verify the deployment has basic properties
            var status = deployment.GetProperty("status");
            Assert.Equal("Succeeded", status.GetString());

            var endpoint = deployment.GetProperty("endpoint");
            Assert.NotNull(endpoint.GetString());
            Assert.StartsWith("https://", endpoint.GetString());
        }

        [Fact]
        public async Task Should_deploy_with_tenant_id()
        {
            var result = await CallToolAsync(
                "azmcp_startups_deploy",
                new()
                {
                    { "tenant", Settings.TenantId },
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "storage-account", Settings.ResourceBaseName },
                    { "source-path", Settings.ResourceBaseName },
                    { "overwrite", true }
                });

            var deployment = result.AssertProperty("deployment");
            Assert.Equal(JsonValueKind.Object, deployment.ValueKind);

            var status = deployment.GetProperty("status");
            Assert.Equal("Succeeded", status.GetString());
        }

        [Fact]
        public async Task Should_deploy_with_subscription_name()
        {
            var result = await CallToolAsync(
                "azmcp_startups_deploy",
                new()
                {
                    { "subscription", Settings.SubscriptionName },
                    { "resource-group", Settings.ResourceGroupName },
                    { "storage-account", Settings.ResourceBaseName },
                    { "source-path", Settings.ResourceBaseName },
                    { "overwrite", true }
                });

            var deployment = result.AssertProperty("deployment");
            Assert.Equal(JsonValueKind.Object, deployment.ValueKind);

            var status = deployment.GetProperty("status");
            Assert.Equal("Succeeded", status.GetString());
        }

        [Fact]
        public async Task Should_verify_web_resources_uploaded()
        {
            // First deploy the resources
            await CallToolAsync(
                "azmcp_startups_deploy",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "storage-account", Settings.ResourceBaseName },
                    { "source-path", Settings.ResourceBaseName },
                    { "overwrite", true }
                });

            // Then verify the blobs are in the $web container
            var result = await CallToolAsync(
                "azmcp_storage_blob_list",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "account", Settings.ResourceBaseName },
                    { "container", "$web" }
                });

            var blobs = result.AssertProperty("blobs");
            Assert.Equal(JsonValueKind.Array, blobs.ValueKind);
            Assert.NotEmpty(blobs.EnumerateArray());

            // Get details of index.html which should exist
            var indexResult = await CallToolAsync(
                "azmcp_storage_blob_details",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "account", Settings.ResourceBaseName },
                    { "container", "$web" },
                    { "blob", "index.html" }
                });

            var blob = indexResult.AssertProperty("blob");
            Assert.Equal(JsonValueKind.Object, blob.ValueKind);
            
            // Verify the blob properties
            var name = blob.GetProperty("name");
            Assert.Equal("index.html", name.GetString());
            
            var contentType = blob.GetProperty("contentType");
            Assert.Equal("text/html", contentType.GetString());
        }
    }
}
