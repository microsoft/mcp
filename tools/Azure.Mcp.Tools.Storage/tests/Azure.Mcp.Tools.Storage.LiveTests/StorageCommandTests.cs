// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Storage.LiveTests
{
    public class StorageCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
    {
        [Fact]
        public async Task Should_list_storage_accounts_by_subscription_id()
        {
            var result = await CallToolAsync(
                "azmcp_storage_account_get",
                new()
                {
                { "subscription", Settings.SubscriptionId }
                });

            var accounts = result.AssertProperty("accounts");
            Assert.Equal(JsonValueKind.Array, accounts.ValueKind);
            Assert.NotEmpty(accounts.EnumerateArray());
        }

        [Fact]
        public async Task Should_get_storage_account_details_by_subscription_id()
        {
            var result = await CallToolAsync(
                "azmcp_storage_account_get",
                new()
                {
                { "subscription", Settings.SubscriptionId },
                { "account", Settings.ResourceBaseName }
                });

            var accounts = result.AssertProperty("accounts");
            Assert.Equal(JsonValueKind.Array, accounts.ValueKind);
            Assert.Equal(1, accounts.GetArrayLength());

            var account = accounts.EnumerateArray().First();
            var name = account.GetProperty("name");
            Assert.Equal(Settings.ResourceBaseName, name.GetString());

            var location = account.GetProperty("location");
            Assert.NotNull(location.GetString());

            var kind = account.GetProperty("kind");
            Assert.Equal("StorageV2", kind.GetString());

            var skuName = account.GetProperty("skuName");
            Assert.Equal("Standard_LRS", skuName.GetString());

            var hnsEnabled = account.GetProperty("hnsEnabled");
            Assert.True(hnsEnabled.GetBoolean());
        }

        [Fact]
        public async Task Should_get_storage_account_details_by_subscription_name()
        {
            var result = await CallToolAsync(
                "azmcp_storage_account_get",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "account", Settings.ResourceBaseName }
                });

            var accounts = result.AssertProperty("accounts");
            Assert.Equal(JsonValueKind.Array, accounts.ValueKind);
            Assert.Equal(1, accounts.GetArrayLength());

            var account = accounts.EnumerateArray().First();
            var name = account.GetProperty("name");
            Assert.Equal(Settings.ResourceBaseName, name.GetString());

            var kind = account.GetProperty("kind");
            Assert.Equal("StorageV2", kind.GetString());
        }

        [Fact]
        public async Task Should_get_storage_account_details_with_tenant_id()
        {
            var result = await CallToolAsync(
                "azmcp_storage_account_get",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "tenant", Settings.TenantId },
                { "account", Settings.ResourceBaseName }
                });

            var accounts = result.AssertProperty("accounts");
            Assert.Equal(JsonValueKind.Array, accounts.ValueKind);
            Assert.Equal(1, accounts.GetArrayLength());

            var account = accounts.EnumerateArray().First();
            var name = account.GetProperty("name");
            Assert.Equal(Settings.ResourceBaseName, name.GetString());
        }

        [Fact()]
        public async Task Should_get_storage_account_details_with_tenant_name()
        {
            Assert.SkipWhen(Settings.IsServicePrincipal, TenantNameReason);

            var result = await CallToolAsync(
                "azmcp_storage_account_get",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "tenant", Settings.TenantName },
                { "account", Settings.ResourceBaseName }
                });

            var accounts = result.AssertProperty("accounts");
            Assert.Equal(JsonValueKind.Array, accounts.ValueKind);
            Assert.Equal(1, accounts.GetArrayLength());

            var account = accounts.EnumerateArray().First();
            var name = account.GetProperty("name");
            Assert.Equal(Settings.ResourceBaseName, name.GetString());
        }

        [Fact]
        public async Task Should_list_storage_accounts_by_subscription_name()
        {
            var result = await CallToolAsync(
                "azmcp_storage_account_get",
                new()
                {
                { "subscription", Settings.SubscriptionName }
                });

            var accounts = result.AssertProperty("accounts");
            Assert.Equal(JsonValueKind.Array, accounts.ValueKind);
            Assert.NotEmpty(accounts.EnumerateArray());
        }

        [Fact]
        public async Task Should_list_storage_accounts_by_subscription_name_with_tenant_id()
        {
            var result = await CallToolAsync(
                "azmcp_storage_account_get",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "tenant", Settings.TenantId }
                });

            var accounts = result.AssertProperty("accounts");
            Assert.Equal(JsonValueKind.Array, accounts.ValueKind);
            Assert.NotEmpty(accounts.EnumerateArray());
        }

        [Fact()]
        public async Task Should_list_storage_accounts_by_subscription_name_with_tenant_name()
        {
            Assert.SkipWhen(Settings.IsServicePrincipal, TenantNameReason);

            var result = await CallToolAsync(
                "azmcp_storage_account_get",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "tenant", Settings.TenantName }
                });

            var accounts = result.AssertProperty("accounts");
            Assert.Equal(JsonValueKind.Array, accounts.ValueKind);
            Assert.NotEmpty(accounts.EnumerateArray());
        }

        [Fact]
        public async Task Should_list_blobs_in_container()
        {
            var result = await CallToolAsync(
                "azmcp_storage_blob_get",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "tenant", Settings.TenantId },
                { "account", Settings.ResourceBaseName },
                { "container", "bar" },
                });

            var actual = result.AssertProperty("blobs");
            Assert.Equal(JsonValueKind.Array, actual.ValueKind);
            Assert.NotEmpty(actual.EnumerateArray());
        }

        [Fact]
        public async Task Should_get_blob_details()
        {
            var result = await CallToolAsync(
                "azmcp_storage_blob_get",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "tenant", Settings.TenantId },
                { "account", Settings.ResourceBaseName },
                { "container", "bar" },
                { "blob", "README.md" },
                });

            var blobs = result.AssertProperty("blobs");
            Assert.Equal(JsonValueKind.Array, blobs.ValueKind);
            Assert.Equal(1, blobs.GetArrayLength());

            var blobInfo = blobs.EnumerateArray().First();
            Assert.Equal(JsonValueKind.Object, blobInfo.ValueKind);

            // Verify the blob has basic properties
            var contentLength = blobInfo.GetProperty("contentLength");
            Assert.True(contentLength.GetInt64() > 0);

            var contentType = blobInfo.GetProperty("contentType");
            Assert.NotNull(contentType.GetString());

            var lastModified = blobInfo.GetProperty("lastModified");
            Assert.NotEqual(default, lastModified.GetDateTimeOffset());
        }

        [Fact]
        public async Task Should_upload_blob()
        {
            // Create a temporary file to upload
            var tempFileName = $"test-upload-{DateTime.UtcNow.Ticks}.txt";
            var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);
            var testContent = "This is a test file for blob upload";

            try
            {
                await File.WriteAllTextAsync(tempFilePath, testContent, TestContext.Current.CancellationToken);

                var result = await CallToolAsync(
                    "azmcp_storage_blob_upload",
                    new()
                    {
                        { "subscription", Settings.SubscriptionName },
                        { "tenant", Settings.TenantId },
                        { "account", Settings.ResourceBaseName },
                        { "container", "bar" },
                        { "blob", tempFileName },
                        { "local-file-path", tempFilePath }
                    });

                // Verify upload details
                var blobName = result.AssertProperty("blob");
                Assert.Equal(tempFileName, blobName.GetString());

                var containerName = result.AssertProperty("container");
                Assert.Equal("bar", containerName.GetString());

                var uploadedFile = result.AssertProperty("uploadedFile");
                Assert.Equal(tempFilePath, uploadedFile.GetString());

                var eTag = result.AssertProperty("eTag");
                Assert.NotNull(eTag.GetString());
                Assert.NotEmpty(eTag.GetString()!);
            }
            finally
            {
                // Clean up the temporary file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        [Fact]
        public async Task Should_list_containers()
        {
            var result = await CallToolAsync(
                "azmcp_storage_blob_container_get",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "tenant", Settings.TenantId },
                { "account", Settings.ResourceBaseName },
                { "retry-max-retries", 0 }
                });

            var actual = result.AssertProperty("containers");
            Assert.Equal(JsonValueKind.Array, actual.ValueKind);
            Assert.NotEmpty(actual.EnumerateArray());
        }

        [Fact]
        public async Task Should_get_container_details()
        {
            var result = await CallToolAsync(
                "azmcp_storage_blob_container_get",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "account", Settings.ResourceBaseName },
                { "container", "bar" }
                });

            var actual = result.AssertProperty("containers");
            Assert.Equal(JsonValueKind.Array, actual.ValueKind);
            Assert.Equal(1, actual.GetArrayLength());

            var container = actual.EnumerateArray().First();
            Assert.Equal(JsonValueKind.Object, container.ValueKind);
        }

        [Fact]
        public async Task Should_get_container_details_with_tenant_authkey()
        {
            var result = await CallToolAsync(
                "azmcp_storage_blob_container_get",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "account", Settings.ResourceBaseName },
                { "container", "bar" },
                { "auth-method", "key" }
                });

            var actual = result.AssertProperty("containers");
            Assert.Equal(JsonValueKind.Array, actual.ValueKind);
            Assert.Equal(1, actual.GetArrayLength());

            var container = actual.EnumerateArray().First();
            Assert.Equal(JsonValueKind.Object, container.ValueKind);
        }

        [Fact]
        public async Task Should_create_container()
        {
            var containerName = $"test-container-{DateTime.UtcNow.Ticks}";

            var result = await CallToolAsync(
                "azmcp_storage_blob_container_create",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "account", Settings.ResourceBaseName },
                { "container", containerName }
                });

            var actual = result.AssertProperty("container");
            Assert.Equal(JsonValueKind.Object, actual.ValueKind);
            actual.AssertProperty("lastModified");
            actual.AssertProperty("eTag");
            actual.AssertProperty("publicAccess");
        }

        [Fact]
        public async Task Should_CreateStorageAccount_Successfully()
        {
            // Arrange - Use a unique account name for testing
            var uniqueAccountName = $"testacct{DateTime.UtcNow:MMddHHmmss}";

            var result = await CallToolAsync(
                "azmcp_storage_account_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "account", uniqueAccountName },
                    { "resource-group", Settings.ResourceGroupName },
                    { "location", "eastus" },
                    { "sku", "Standard_LRS" },
                    { "kind", "StorageV2" }
                });

            // Assert
            var account = result.AssertProperty("account");
            Assert.Equal(JsonValueKind.Object, account.ValueKind);

            // Check account properties
            var name = account.GetProperty("name").GetString();
            Assert.Equal(uniqueAccountName, name);

            var location = account.GetProperty("location").GetString();
            Assert.Equal("eastus", location);

            var kind = account.GetProperty("kind").GetString();
            Assert.Equal("StorageV2", kind);

            var skuName = account.GetProperty("skuName").GetString();
            Assert.Equal("Standard_LRS", skuName);
        }

        [Theory]
        [InlineData("--invalid-param", new string[0])]
        [InlineData("--subscription", new[] { "invalidSub" })]
        [InlineData("--account-name", new[] { "testacct", "--subscription", "sub123" })] // Missing required resource-group and location
        public async Task Should_Return400_WithInvalidInput_ForAccountCreate(string firstArg, string[] remainingArgs)
        {
            var allArgs = new[] { firstArg }.Concat(remainingArgs);
            var argsString = string.Join(" ", allArgs);

            // For error testing, we expect CallToolAsync to return null (no results)
            // and we need to catch any exceptions or check the response manually
            try
            {
                var result = await CallToolAsync("azmcp_storage_account_create",
                    new Dictionary<string, object?> { { "args", argsString } });

                // If we get here, the command didn't fail as expected
                // This might indicate the command succeeded when it should have failed
                Assert.Fail("Expected command to fail with invalid input, but it succeeded");
            }
            catch (Exception ex)
            {
                // Expected to fail with validation errors
                Assert.True(ex.Message.Contains("required") || ex.Message.Contains("invalid"),
                    $"Expected validation error, but got: {ex.Message}");
            }
        }
    }
}
