// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Fabric.Mcp.Tools.OneLake.Services;
using NSubstitute;

namespace Fabric.Mcp.Tools.OneLake.Tests.Services;

/// <summary>
/// Verifies that all public OneLakeService methods that accept user-supplied file/directory paths
/// reject directory traversal sequences before making any HTTP calls.
/// </summary>
public class OneLakePathTraversalTests
{
    /// <summary>
    /// Guard handler that fails the test if an HTTP call is attempted.
    /// Path validation must throw before any HTTP request is sent.
    /// </summary>
    private sealed class UnexpectedCallHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new InvalidOperationException("HTTP call should not be reached: path validation must throw first.");
    }

    private static OneLakeService CreateService()
    {
        var credential = Substitute.For<TokenCredential>();
        return new OneLakeService(new HttpClient(new UnexpectedCallHandler()), credential);
    }

    // ---- GetFileInfoAsync ----

    [Theory]
    [InlineData("../../secret.txt")]
    [InlineData("Files/../../other-item/data")]
    [InlineData("../credentials.env")]
    [InlineData("Files/sub/../../../escape")]
    [InlineData("%2e%2e/secret.txt")]            // percent-encoded lowercase
    [InlineData("%2E%2E/secret.txt")]            // percent-encoded uppercase
    [InlineData("Files/%2e%2e/other-item/data")] // encoded in middle segment
    [InlineData("Files/%2E%2E/%2e%2e/escape")]   // multiple encoded segments
    [InlineData("~/secret.txt")]                 // tilde home-dir segment
    [InlineData("Files/~/data")]                  // tilde in middle
    public async Task GetFileInfoAsync_ThrowsArgumentException_ForTraversalPath(string filePath)
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetFileInfoAsync("workspace", "item", filePath, CancellationToken.None));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Files/normal-path.txt")]
    [InlineData("Files/subdir/file.csv")]
    [InlineData("Tables/schema/tablename")]
    public async Task GetFileInfoAsync_DoesNotThrow_ForValidPath(string validPath)
    {
        // Validation passes; the first HTTP call will hit the guard and throw InvalidOperationException.
        var service = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetFileInfoAsync("workspace", "item", validPath, CancellationToken.None));
    }

    // ---- ReadFileAsync ----

    [Theory]
    [InlineData("../../secret.txt")]
    [InlineData("Files/../../other-item/data")]
    [InlineData("../credentials.env")]
    [InlineData("%2e%2e/secret.txt")]
    [InlineData("%2E%2E/secret.txt")]
    [InlineData("Files/%2e%2e/other-item/data")]
    [InlineData("~/secret.txt")]
    [InlineData("Files/~/data")]
    public async Task ReadFileAsync_ThrowsArgumentException_ForTraversalPath(string filePath)
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ReadFileAsync("workspace", "item", filePath, cancellationToken: CancellationToken.None));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---- WriteFileAsync ----

    [Theory]
    [InlineData("../../secret.txt")]
    [InlineData("Files/../../other-item/data")]
    [InlineData("../credentials.env")]
    [InlineData("%2e%2e/secret.txt")]
    [InlineData("%2E%2E/secret.txt")]
    [InlineData("Files/%2e%2e/other-item/data")]
    [InlineData("~/secret.txt")]
    [InlineData("Files/~/data")]
    public async Task WriteFileAsync_ThrowsArgumentException_ForTraversalPath(string filePath)
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.WriteFileAsync("workspace", "item", filePath, Stream.Null, cancellationToken: CancellationToken.None));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---- PutBlobAsync ----

    [Theory]
    [InlineData("../../secret.txt")]
    [InlineData("Files/../../other-item/data")]
    [InlineData("../credentials.env")]
    [InlineData("%2e%2e/secret.txt")]
    [InlineData("%2E%2E/secret.txt")]
    [InlineData("Files/%2e%2e/other-item/data")]
    [InlineData("~/secret.txt")]
    [InlineData("Files/~/data")]
    public async Task PutBlobAsync_ThrowsArgumentException_ForTraversalPath(string blobPath)
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.PutBlobAsync("workspace", "item", blobPath, Stream.Null, 0, cancellationToken: CancellationToken.None));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---- GetBlobAsync ----

    [Theory]
    [InlineData("../../secret.txt")]
    [InlineData("Files/../../other-item/data")]
    [InlineData("../credentials.env")]
    [InlineData("%2e%2e/secret.txt")]
    [InlineData("%2E%2E/secret.txt")]
    [InlineData("Files/%2e%2e/other-item/data")]
    [InlineData("~/secret.txt")]
    [InlineData("Files/~/data")]
    public async Task GetBlobAsync_ThrowsArgumentException_ForTraversalPath(string blobPath)
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetBlobAsync("workspace", "item", blobPath, cancellationToken: CancellationToken.None));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---- DeleteFileAsync ----

    [Theory]
    [InlineData("../../secret.txt")]
    [InlineData("Files/../../other-item/data")]
    [InlineData("../credentials.env")]
    [InlineData("%2e%2e/secret.txt")]
    [InlineData("%2E%2E/secret.txt")]
    [InlineData("Files/%2e%2e/other-item/data")]
    [InlineData("~/secret.txt")]
    [InlineData("Files/~/data")]
    public async Task DeleteFileAsync_ThrowsArgumentException_ForTraversalPath(string filePath)
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.DeleteFileAsync("workspace", "item", filePath, CancellationToken.None));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---- DeleteDirectoryAsync ----

    [Theory]
    [InlineData("../../dir")]
    [InlineData("Files/../../other-item")]
    [InlineData("../subdir")]
    [InlineData("%2e%2e/dir")]
    [InlineData("%2E%2E/dir")]
    [InlineData("Files/%2e%2e/other-item")]
    [InlineData("~/dir")]
    [InlineData("Files/~/subdir")]
    public async Task DeleteDirectoryAsync_ThrowsArgumentException_ForTraversalPath(string directoryPath)
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.DeleteDirectoryAsync("workspace", "item", directoryPath, cancellationToken: CancellationToken.None));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---- CreateDirectoryAsync ----

    [Theory]
    [InlineData("../../dir")]
    [InlineData("Files/../../other-item")]
    [InlineData("../subdir")]
    [InlineData("%2e%2e/dir")]
    [InlineData("%2E%2E/dir")]
    [InlineData("Files/%2e%2e/other-item")]
    [InlineData("~/dir")]
    [InlineData("Files/~/subdir")]
    public async Task CreateDirectoryAsync_ThrowsArgumentException_ForTraversalPath(string directoryPath)
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateDirectoryAsync("workspace", "item", directoryPath, CancellationToken.None));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---- ListBlobsAsync ----

    [Theory]
    [InlineData("../../secret")]
    [InlineData("Files/../../other-item")]
    [InlineData("../hidden")]
    [InlineData("%2e%2e/secret")]
    [InlineData("%2E%2E/secret")]
    [InlineData("Files/%2e%2e/other-item")]
    [InlineData("~/secret")]
    [InlineData("Files/~/data")]
    public async Task ListBlobsAsync_ThrowsArgumentException_ForTraversalPath(string path)
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListBlobsAsync("workspace", "item", path, cancellationToken: CancellationToken.None));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ListBlobsAsync_DoesNotThrow_WhenPathIsNull()
    {
        // null path means "list all" - should proceed past validation
        var service = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ListBlobsAsync("workspace", "item", path: null, cancellationToken: CancellationToken.None));
    }

    // ---- ListPathAsync ----

    [Theory]
    [InlineData("../../secret")]
    [InlineData("Files/../../other-item")]
    [InlineData("../hidden")]
    [InlineData("%2e%2e/secret")]
    [InlineData("%2E%2E/secret")]
    [InlineData("Files/%2e%2e/other-item")]
    [InlineData("~/secret")]
    [InlineData("Files/~/data")]
    public async Task ListPathAsync_ThrowsArgumentException_ForTraversalPath(string path)
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListPathAsync("workspace", "item", path, cancellationToken: CancellationToken.None));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---- ListBlobsRawAsync ----

    [Theory]
    [InlineData("../../secret")]
    [InlineData("Files/../../other-item")]
    [InlineData("../hidden")]
    [InlineData("%2e%2e/secret")]
    [InlineData("%2E%2E/secret")]
    [InlineData("Files/%2e%2e/other-item")]
    [InlineData("~/secret")]
    [InlineData("Files/~/data")]
    public async Task ListBlobsRawAsync_ThrowsArgumentException_ForTraversalPath(string path)
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListBlobsRawAsync("workspace", "item", path, cancellationToken: CancellationToken.None));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---- ListPathRawAsync ----

    [Theory]
    [InlineData("../../secret")]
    [InlineData("Files/../../other-item")]
    [InlineData("../hidden")]
    [InlineData("%2e%2e/secret")]
    [InlineData("%2E%2E/secret")]
    [InlineData("Files/%2e%2e/other-item")]
    [InlineData("~/secret")]
    [InlineData("Files/~/data")]
    public async Task ListPathRawAsync_ThrowsArgumentException_ForTraversalPath(string path)
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListPathRawAsync("workspace", "item", path, cancellationToken: CancellationToken.None));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---- DeleteBlobAsync ----

    [Theory]
    [InlineData("../../secret.txt")]
    [InlineData("Files/../../other-item/data")]
    [InlineData("../credentials.env")]
    [InlineData("%2e%2e/secret.txt")]
    [InlineData("%2E%2E/secret.txt")]
    [InlineData("Files/%2e%2e/other-item/data")]
    [InlineData("~/secret.txt")]
    [InlineData("Files/~/data")]
    public async Task DeleteBlobAsync_ThrowsArgumentException_ForTraversalPath(string blobPath)
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.DeleteBlobAsync("workspace", "item", blobPath, CancellationToken.None));
        Assert.Contains("directory traversal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
