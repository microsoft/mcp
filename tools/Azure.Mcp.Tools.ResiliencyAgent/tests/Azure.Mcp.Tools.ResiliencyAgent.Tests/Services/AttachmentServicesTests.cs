// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.Services;

public sealed class AttachmentServicesTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "AzureMcpResiliencyAttachments_" + Guid.NewGuid().ToString("N"));

    public AttachmentServicesTests()
    {
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public async Task SnapshotAsync_PreservesExactBytesAndMetadata()
    {
        string path = Path.Combine(_root, "main.bicep");
        byte[] expected = "param location string = 'eastus2'\n"u8.ToArray();
        await File.WriteAllBytesAsync(path, expected, TestContext.Current.CancellationToken);
        var snapshotter = new LocalFileSnapshotter(TimeProvider.System);

        ValidatedLocalFile validated = snapshotter.Validate(path);
        AgentAttachment snapshot =
            await snapshotter.SnapshotAsync(validated, TestContext.Current.CancellationToken);

        Assert.Equal("main.bicep", snapshot.Name);
        Assert.Equal("text/plain", snapshot.MimeType);
        Assert.Equal(expected, snapshot.Content);
        Assert.Equal(
            Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(expected)),
            snapshot.Sha256);
        Assert.True(snapshot.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("parameters.bicepparam")]
    [InlineData("prod.tfvars")]
    public async Task SnapshotAsync_AcceptsParameterFilesAsPlainText(string fileName)
    {
        string path = Path.Combine(_root, fileName);
        byte[] expected = "location = \"eastus2\"\n"u8.ToArray();
        await File.WriteAllBytesAsync(path, expected, TestContext.Current.CancellationToken);
        var snapshotter = new LocalFileSnapshotter(TimeProvider.System);

        ValidatedLocalFile validated = snapshotter.Validate(path);
        AgentAttachment snapshot =
            await snapshotter.SnapshotAsync(validated, TestContext.Current.CancellationToken);

        Assert.Equal(fileName, snapshot.Name);
        Assert.Equal("text/plain", snapshot.MimeType);
        Assert.Equal(expected, snapshot.Content);
    }

    [Theory]
    [InlineData("relative.bicep", "fully-qualified")]
    [InlineData("https://example.test/main.bicep", "URLs")]
    public void Validate_RejectsUnsafePathForms(string path, string expectedMessage)
    {
        var snapshotter = new LocalFileSnapshotter(TimeProvider.System);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => snapshotter.Validate(path));

        Assert.Contains(expectedMessage, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_RejectsUncPathBeforeFilesystemAccess()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var snapshotter = new LocalFileSnapshotter(TimeProvider.System);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => snapshotter.Validate(@"\\server\share\main.bicep"));

        Assert.Contains("UNC paths", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SnapshotAsync_RejectsFileAboveAcpLimit()
    {
        string path = Path.Combine(_root, "large.bicep");
        await File.WriteAllBytesAsync(
            path,
            new byte[1434 * 1024 + 1],
            TestContext.Current.CancellationToken);
        var snapshotter = new LocalFileSnapshotter(TimeProvider.System);

        Assert.Throws<ArgumentOutOfRangeException>(() => snapshotter.Validate(path));
    }

    [Fact]
    public void Cache_ResolvesThenSecurelyConsumesAttachment()
    {
        var cache = new AttachmentCache(TimeProvider.System);
        byte[] content = [1, 2, 3];
        var attachment = new AgentAttachment(
            "id-1",
            "main.bicep",
            "text/plain",
            content,
            "hash",
            DateTimeOffset.UtcNow.AddMinutes(5));
        cache.Add(attachment);

        Assert.Same(attachment, Assert.Single(cache.Resolve(["id-1"])));

        cache.Remove(["id-1"]);

        Assert.All(content, value => Assert.Equal(0, value));
        Assert.Throws<KeyNotFoundException>(() => cache.Resolve(["id-1"]));
    }

    [Fact]
    public void Cache_RejectsDuplicateIdsAndTooManyFiles()
    {
        var cache = new AttachmentCache(TimeProvider.System);
        var attachment = new AgentAttachment(
            "id-1",
            "main.bicep",
            "text/plain",
            [1],
            "hash",
            DateTimeOffset.UtcNow.AddMinutes(5));
        cache.Add(attachment);

        Assert.Throws<ArgumentException>(() => cache.Resolve(["id-1", "id-1"]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            cache.Resolve(Enumerable.Range(0, 11).Select(i => $"id-{i}").ToArray()));
    }

    public void Dispose()
    {
        Directory.Delete(_root, recursive: true);
    }
}
