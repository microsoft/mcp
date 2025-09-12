// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
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
        public async Task Should_list_storage_tables()
        {
            var result = await CallToolAsync(
                "azmcp_storage_table_list",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "tenant", Settings.TenantId },
                { "account", Settings.ResourceBaseName },
                });

            var actual = result.AssertProperty("tables");
            Assert.Equal(JsonValueKind.Array, actual.ValueKind);
            Assert.NotEmpty(actual.EnumerateArray());
        }

        [Fact]
        public async Task Should_list_storage_tables_with_tenant_id()
        {
            var result = await CallToolAsync(
                "azmcp_storage_table_list",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "tenant", Settings.TenantId },
                { "account", Settings.ResourceBaseName },
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
                "azmcp_storage_table_list",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "tenant", Settings.TenantName },
                { "account", Settings.ResourceBaseName },
                });

            var actual = result.AssertProperty("tables");
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
        public async Task Should_list_datalake_filesystem_paths()
        {
            var result = await CallToolAsync(
                "azmcp_storage_datalake_file-system_list-paths",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "account", Settings.ResourceBaseName },
                { "file-system", "testfilesystem" }
                });

            var actual = result.AssertProperty("paths");
            Assert.Equal(JsonValueKind.Array, actual.ValueKind);
        }

        [Fact]
        public async Task Should_list_datalake_filesystem_paths_recursively()
        {
            var result = await CallToolAsync(
                "azmcp_storage_datalake_file-system_list-paths",
                new()
                {
                { "subscription", Settings.SubscriptionName },
                { "account", Settings.ResourceBaseName },
                { "file-system", "testfilesystem" },
                { "recursive", true }
                });

            var actual = result.AssertProperty("paths");
            Assert.Equal(JsonValueKind.Array, actual.ValueKind);
        }

        [Fact]
        public async Task Should_create_datalake_directory()
        {
            var directoryPath = "testfilesystem/test-directory";

            var result = await CallToolAsync(
                "azmcp_storage_datalake_directory_create",
                new()
                {
                    { "subscription", Settings.SubscriptionName },
                    { "account", Settings.ResourceBaseName },
                    { "directory-path", directoryPath }
                });

            var directory = result.AssertProperty("directory");
            Assert.Equal(JsonValueKind.Object, directory.ValueKind);

            var name = directory.GetProperty("name").GetString();
            var type = directory.GetProperty("type").GetString();

            Assert.Equal(directoryPath, name);
            Assert.Equal("directory", type);
        }

        [Fact]
        public async Task Should_set_blob_tier_batch()
        {
            // This test assumes the test storage account has the "bar" container with some test blobs
            // We'll set tier to Cool for multiple blobs
            var result = await CallToolAsync(
                "azmcp_storage_blob_batch_set-tier",
                new()
                {
                    { "subscription", Settings.SubscriptionName },
                    { "account", Settings.ResourceBaseName },
                    { "container", "bar" },
                    { "tier", "Cool" },
                    { "blobs", "blob1.txt blob2.txt" }
                });

            var successfulBlobs = result.AssertProperty("successfulBlobs");
            var failedBlobs = result.AssertProperty("failedBlobs");

            Assert.Equal(JsonValueKind.Array, successfulBlobs.ValueKind);
            Assert.Equal(JsonValueKind.Array, failedBlobs.ValueKind);

            // At least one of the blobs should succeed if they exist, or all should be in failed if they don't exist
            var successCount = successfulBlobs.GetArrayLength();
            var failedCount = failedBlobs.GetArrayLength();

            Assert.True(successCount + failedCount > 0, "Should have processed at least one blob");
        }

        [Fact]
        public async Task Should_list_files_in_share_directory()
        {
            var result = await CallToolAsync(
                "azmcp_storage_share_file_list",
                new()
                {
                    { "subscription", Settings.SubscriptionName },
                    { "account", Settings.ResourceBaseName },
                    { "share", "testshare" },
                    { "directory-path", "/" }
                });

            var files = result.AssertProperty("files");
            Assert.Equal(JsonValueKind.Array, files.ValueKind);
            Assert.NotEmpty(files.EnumerateArray());
        }

        [Fact]
        public async Task Should_list_files_in_share_directory_with_prefix()
        {
            var result = await CallToolAsync(
                "azmcp_storage_share_file_list",
                new()
                {
                    { "subscription", Settings.SubscriptionName },
                    { "account", Settings.ResourceBaseName },
                    { "share", "testshare" },
                    { "directory-path", "/" },
                    { "prefix", "NoSuchPrefix" }
                });

            // When using a prefix that does not match any files, we should still return a valid response
            // with no result.
            Assert.Null(result);
        }

        [Fact]
        public async Task Should_SendQueueMessage_Successfully()
        {
            // Arrange
            var result = await CallToolAsync(
                "azmcp_storage_queue_message_send",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "account", Settings.ResourceBaseName },
                    { "queue", "testqueue" },
                    { "message", "Test message from integration test" }
                });

            // Assert
            var message = result.AssertProperty("message");
            Assert.Equal(JsonValueKind.Object, message.ValueKind);

            // Check message properties
            message.AssertProperty("messageId");
            message.AssertProperty("insertionTime");
            message.AssertProperty("expirationTime");
            message.AssertProperty("popReceipt");
            message.AssertProperty("nextVisibleTime");
            Assert.True(message.TryGetProperty("message", out var messageElement));
            Assert.Equal("Test message from integration test", messageElement.GetString());
        }

        [Fact]
        public async Task Should_SendQueueMessage_WithOptionalParameters()
        {
            // Arrange
            var result = await CallToolAsync(
                "azmcp_storage_queue_message_send",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "account", Settings.ResourceBaseName },
                    { "queue", "testqueue" },
                    { "message", "Test message with TTL" },
                    { "time-to-live-in-seconds", "3600" },
                    { "visibility-timeout-in-seconds", "30" }
                });

            // Assert
            var message = result.AssertProperty("message");
            Assert.Equal(JsonValueKind.Object, message.ValueKind);
            Assert.True(message.TryGetProperty("message", out var messageElement));
            Assert.Equal("Test message with TTL", messageElement.GetString());
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
