// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words tmdl

using System.Text;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FabricItemDefinitionFileServiceTests() : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("fabric-definition-tests-").FullName;
    private readonly FabricItemDefinitionFileService _service = new(NullLogger<FabricItemDefinitionFileService>.Instance);
    private CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public async Task ReadAndWrite_PreserveSelectedPartsBytesPathsAndFormat()
    {
        var source = Path.Combine(_root, "source");
        Directory.CreateDirectory(Path.Combine(source, "definition"));
        Directory.CreateDirectory(Path.Combine(source, "StaticResources"));
        byte[] text = [0xef, 0xbb, 0xbf, .. Encoding.UTF8.GetBytes("line one\r\nline two\n")];
        byte[] binary = Enumerable.Range(0, 256).Select(static value => (byte)value).ToArray();
        await File.WriteAllBytesAsync(Path.Combine(source, "definition", "model.tmdl"), text, CancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(source, "StaticResources", "image.bin"), binary, CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(source, ".platform"), "{}", CancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(source, "empty.txt"), [], CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(source, "unselected.txt"), "Do not package this file.", CancellationToken);
        string[] paths = [".platform", "definition/model.tmdl", "StaticResources/image.bin", "empty.txt"];

        var definition = await _service.ReadAsync(source, paths.Select(static path => new FabricItemDefinitionFile(path)).ToArray(),
            "TMDL", CancellationToken);
        Assert.Equal("TMDL", definition.Format);
        Assert.Equal(paths, definition.Parts.Select(static part => part.Path));
        Assert.All(definition.Parts, static part => Assert.Equal("InlineBase64", part.PayloadType));
        Assert.Equal(text, Convert.FromBase64String(definition.Parts[1].Payload));
        Assert.Equal(binary, Convert.FromBase64String(definition.Parts[2].Payload));
        Assert.Equal("", definition.Parts[3].Payload);

        var output = await _service.WriteAsync(Path.Combine(_root, "output"), definition, CancellationToken);
        Assert.Equal(Path.Combine(_root, "output"), output);
        Assert.False(File.Exists(Path.Combine(output, "unselected.txt")));
        Assert.Equal(text, await File.ReadAllBytesAsync(Path.Combine(output, "definition", "model.tmdl"), CancellationToken));
        Assert.Equal(binary, await File.ReadAllBytesAsync(Path.Combine(output, "StaticResources", "image.bin"), CancellationToken));
        Assert.Equal("{}", await File.ReadAllTextAsync(Path.Combine(output, ".platform"), CancellationToken));
        var roundTrip = await _service.ReadAsync(output, paths.Select(static path => new FabricItemDefinitionFile(path)).ToArray(),
            definition.Format, CancellationToken);
        Assert.Equal(definition.Format, roundTrip.Format);
        Assert.Equal(definition.Parts, roundTrip.Parts);
        AssertNoStaging();
    }

    [Fact]
    public async Task Read_MapsSourceFileWithoutRenamingIt()
    {
        Directory.CreateDirectory(Path.Combine(_root, "notebooks"));
        var source = Path.Combine(_root, "notebooks", "sales.ipynb");
        await File.WriteAllTextAsync(source, "{\"cells\":[]}", CancellationToken);

        var definition = await _service.ReadAsync(_root,
            [new("artifact.content.ipynb", @"notebooks\sales.ipynb")], "ipynb", CancellationToken);

        Assert.Equal("artifact.content.ipynb", Assert.Single(definition.Parts).Path);
        Assert.True(File.Exists(source));
        Assert.False(File.Exists(Path.Combine(_root, "artifact.content.ipynb")));
        Assert.Equal("{\"cells\":[]}", Encoding.UTF8.GetString(Convert.FromBase64String(definition.Parts[0].Payload)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("../outside.txt")]
    [InlineData("/outside.txt")]
    [InlineData("a/../outside.txt")]
    [InlineData("a/./file.txt")]
    [InlineData("a//file.txt")]
    [InlineData("a/")]
    [InlineData(@"a\file.txt")]
    [InlineData("C:/outside.txt")]
    [InlineData("file.txt:stream")]
    [InlineData("a./file.txt")]
    [InlineData("a /file.txt")]
    [InlineData("file?.txt")]
    [InlineData("CON")]
    [InlineData("dir/nul.txt")]
    [InlineData("LPT1.bin")]
    [InlineData("COM\u00b9.txt")]
    [InlineData("\0file.txt")]
    [InlineData("\nfile.txt")]
    public async Task BothDirections_RejectUnsafePartPaths(string path)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ReadAsync(_root, [new(path)], cancellationToken: CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.WriteAsync(Path.Combine(_root, "output"), new([Part(path)]), CancellationToken));
        Assert.Empty(Directory.EnumerateFileSystemEntries(_root));
    }

    [Theory]
    [InlineData("a.txt", "a.txt")]
    [InlineData("a.txt", "A.TXT")]
    [InlineData("a", "a/b.txt")]
    [InlineData("A/b.txt", "a")]
    public async Task BothDirections_RejectDuplicateOrConflictingPaths(string first, string second)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ReadAsync(_root, [new(first), new(second)], cancellationToken: CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.WriteAsync(Path.Combine(_root, "output"), new([Part(first), Part(second)]), CancellationToken));
        Assert.Empty(Directory.EnumerateFileSystemEntries(_root));
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData(@"..\outside.txt")]
    [InlineData("/outside.txt")]
    [InlineData("C:/outside.txt")]
    public async Task Read_RejectsEscapingSourceMappings(string source)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ReadAsync(_root, [new("safe.txt", source)], cancellationToken: CancellationToken));
    }

    [Theory]
    [InlineData("relative")]
    [InlineData(@"\\server\share\folder")]
    [InlineData(@"\\?\C:\folder")]
    [InlineData(@"\\.\C:\folder")]
    public async Task BothDirections_RejectNonLocalOrRelativeRoots(string directory)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ReadAsync(directory, [new("a.txt")], cancellationToken: CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.WriteAsync(directory, new([Part("a.txt")]), CancellationToken));
    }

    [Fact]
    public async Task BothDirections_RejectOversizedPathComponents()
    {
        var path = new string('a', 256);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ReadAsync(_root, [new(path)], cancellationToken: CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.WriteAsync(Path.Combine(_root, "output"), new([Part(path)]), CancellationToken));
        var deepPath = string.Join('/', Enumerable.Repeat(new string('a', 200), 6));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.WriteAsync(Path.Combine(_root, "output"), new([Part(deepPath)]), CancellationToken));
    }

    [Fact]
    public async Task BothDirections_RejectEmptyPartsAndBlankFormats()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ReadAsync(_root, [], cancellationToken: CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.WriteAsync(Path.Combine(_root, "output"), new([]), CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ReadAsync(_root, [new("a.txt")], " ", CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.WriteAsync(Path.Combine(_root, "output"), new([Part("a.txt")], ""), CancellationToken));
    }

    [Fact]
    public async Task BothDirections_RejectNullModels()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.ReadAsync(_root, null!, cancellationToken: CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.ReadAsync(_root, [null!], cancellationToken: CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.WriteAsync(Path.Combine(_root, "output"), null!, CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.WriteAsync(Path.Combine(_root, "output"), new(null!), CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.WriteAsync(Path.Combine(_root, "output"), new([null!]), CancellationToken));
    }

    [Fact]
    public async Task BothDirections_EnforceExactPartAndTotalByteBudgets()
    {
        var service = CreateService(new(3, 3, 5));
        await File.WriteAllBytesAsync(Path.Combine(_root, "a"), [1, 2, 3], CancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(_root, "b"), [4, 5], CancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(_root, "empty"), [], CancellationToken);
        FabricItemDefinitionFile[] selected = [new("a"), new("b"), new("empty")];

        var definition = await service.ReadAsync(_root, selected, cancellationToken: CancellationToken);
        await service.WriteAsync(Path.Combine(_root, "at-limit"), definition, CancellationToken);
        Assert.Equal(3, definition.Parts.Count);

        await File.WriteAllBytesAsync(Path.Combine(_root, "empty"), [6], CancellationToken);
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            service.ReadAsync(_root, selected, cancellationToken: CancellationToken));
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            service.WriteAsync(Path.Combine(_root, "over-total"), new([Part("a", [1, 2, 3]), Part("b", [4, 5, 6])]), CancellationToken));
        await File.WriteAllBytesAsync(Path.Combine(_root, "a"), [1, 2, 3, 4], CancellationToken);
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            service.ReadAsync(_root, [new("a")], cancellationToken: CancellationToken));
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            service.WriteAsync(Path.Combine(_root, "over-part"), new([Part("a", [1, 2, 3, 4])]), CancellationToken));
        Assert.False(Directory.Exists(Path.Combine(_root, "over-total")));
        Assert.False(Directory.Exists(Path.Combine(_root, "over-part")));
        AssertNoStaging();
    }

    [Fact]
    public async Task BothDirections_EnforcePartCountBeforeFileAccess()
    {
        var service = CreateService(new(1, 3, 3));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ReadAsync(_root, [new("a"), new("b")], cancellationToken: CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.WriteAsync(Path.Combine(_root, "output"), new([Part("a"), Part("b")]), CancellationToken));
        Assert.Empty(Directory.EnumerateFileSystemEntries(_root));
    }

    [Theory]
    [InlineData("%%%=", "InlineBase64")]
    [InlineData("abc", "InlineBase64")]
    [InlineData("YQ==", "Unknown")]
    [InlineData(null, "InlineBase64")]
    [InlineData("YQ==", null)]
    public async Task Write_ValidatesEveryPayloadBeforeCreatingOutput(string? payload, string? payloadType)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.WriteAsync(Path.Combine(_root, "output"),
                new([Part("valid.txt"), new("invalid.txt", payload!, payloadType!)]), CancellationToken));
        Assert.Empty(Directory.EnumerateFileSystemEntries(_root));
    }

    [Fact]
    public async Task Write_RejectsEncodedBudgetEvenWhenWhitespaceDecodesToSmallContent()
    {
        var service = CreateService(new(1, 3, 3));
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            service.WriteAsync(Path.Combine(_root, "output"),
                new([new("a", "YQ==    ", "InlineBase64")]), CancellationToken));
        Assert.Empty(Directory.EnumerateFileSystemEntries(_root));
    }

    [Fact]
    public async Task Write_NeverOverwritesExistingFilesOrDirectories()
    {
        var existingFile = Path.Combine(_root, "existing.txt");
        await File.WriteAllTextAsync(existingFile, "keep", CancellationToken);
        var existingDirectory = Path.Combine(_root, "existing");
        Directory.CreateDirectory(existingDirectory);
        await File.WriteAllTextAsync(Path.Combine(existingDirectory, "keep.txt"), "keep", CancellationToken);
        foreach (var destination in new[] { existingFile, existingDirectory })
        {
            await Assert.ThrowsAsync<IOException>(() =>
                _service.WriteAsync(destination, new([Part("other.txt")]), CancellationToken));
        }
        Assert.Equal("keep", await File.ReadAllTextAsync(existingFile, CancellationToken));
        Assert.Equal(["keep.txt"], Directory.EnumerateFiles(existingDirectory).Select(Path.GetFileName));
        AssertNoStaging();
    }

    [Fact]
    public async Task Read_ReportsMissingFilesAndWriteRequiresExistingParent()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _service.ReadAsync(_root, [new("missing.txt")], cancellationToken: CancellationToken));
        await Assert.ThrowsAnyAsync<IOException>(() =>
            _service.WriteAsync(Path.Combine(_root, "missing-parent", "output"), new([Part("a")]), CancellationToken));
        Assert.Empty(Directory.EnumerateFileSystemEntries(_root));
    }

    [Fact]
    public async Task BothDirections_ObserveCancellationBeforeIo()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _service.ReadAsync(_root, [new("a")], cancellationToken: cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _service.WriteAsync(Path.Combine(_root, "output"), new([Part("a")]), cancellation.Token));
        Assert.Empty(Directory.EnumerateFileSystemEntries(_root));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Write_FailureOrCancellationDoesNotPublishPartialOutput(bool cancel)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
        var count = 0;
        var service = new FabricItemDefinitionFileService(NullLogger<FabricItemDefinitionFileService>.Instance, null, path =>
        {
            if (++count == 2)
            {
                if (cancel)
                {
                    cancellation.Cancel();
                }
                else
                {
                    throw new IOException("Simulated disk failure.");
                }
            }
            return new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        });
        var write = () => service.WriteAsync(Path.Combine(_root, "output"),
            new([Part("a"), Part("nested/b")]), cancellation.Token);

        if (cancel)
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(write);
        }
        else
        {
            await Assert.ThrowsAsync<IOException>(write);
        }
        Assert.Equal(2, count);
        Assert.Empty(Directory.EnumerateFileSystemEntries(_root));
    }

    [Fact]
    public async Task Write_DestinationCreatedDuringStagingIsPreserved()
    {
        var destination = Path.Combine(_root, "output");
        var service = new FabricItemDefinitionFileService(NullLogger<FabricItemDefinitionFileService>.Instance, null, path =>
        {
            Directory.CreateDirectory(destination);
            File.WriteAllText(Path.Combine(destination, "keep.txt"), "keep");
            return new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        });

        await Assert.ThrowsAsync<IOException>(() =>
            service.WriteAsync(destination, new([Part("a")]), CancellationToken));

        Assert.Equal(["keep.txt"], Directory.EnumerateFiles(destination).Select(Path.GetFileName));
        AssertNoStaging();
    }

    [Fact]
    public async Task Write_ConcurrentExportsHaveIndependentState()
    {
        var results = await Task.WhenAll(
            _service.WriteAsync(Path.Combine(_root, "first"), new([Part("a", [1])], "first-format"), CancellationToken),
            _service.WriteAsync(Path.Combine(_root, "second"), new([Part("b", [2])], "second-format"), CancellationToken));

        Assert.Equal(new byte[] { 1 }, await File.ReadAllBytesAsync(Path.Combine(results[0], "a"), CancellationToken));
        Assert.Equal(new byte[] { 2 }, await File.ReadAllBytesAsync(Path.Combine(results[1], "b"), CancellationToken));
        AssertNoStaging();
    }

    [Fact]
    public async Task BothDirections_RejectSymbolicLinkDirectories()
    {
        var real = Directory.CreateDirectory(Path.Combine(_root, "real")).FullName;
        var link = Path.Combine(_root, "linked");
        CreateLink(link, real, directory: true);
        try
        {
            await Assert.ThrowsAsync<IOException>(() =>
                _service.ReadAsync(link, [new("a")], cancellationToken: CancellationToken));
            await Assert.ThrowsAsync<IOException>(() =>
                _service.WriteAsync(Path.Combine(link, "output"), new([Part("a")]), CancellationToken));
            Assert.Empty(Directory.EnumerateFileSystemEntries(real));
        }
        finally
        {
            Directory.Delete(link);
        }
    }

    [Fact]
    public async Task Read_RejectsSymbolicLinkFiles()
    {
        var target = Path.Combine(_root, "target");
        await File.WriteAllTextAsync(target, "not selected", CancellationToken);
        var link = Path.Combine(_root, "linked");
        CreateLink(link, target, directory: false);
        try
        {
            await Assert.ThrowsAsync<IOException>(() =>
                _service.ReadAsync(_root, [new("a", "linked")], cancellationToken: CancellationToken));
            Assert.Equal("not selected", await File.ReadAllTextAsync(target, CancellationToken));
        }
        finally
        {
            File.Delete(link);
        }
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(-1, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 2, 1)]
    public static void Constructor_RejectsInvalidLimits(int parts, int partBytes, int totalBytes)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateService(new(parts, partBytes, totalBytes)));
    }

    private static FabricItemDefinitionFileService CreateService(FabricItemDefinitionFileLimits limits) =>
        new(NullLogger<FabricItemDefinitionFileService>.Instance, limits);

    private static FabricItemDefinitionPart Part(string path, byte[]? content = null) =>
        new(path, Convert.ToBase64String(content ?? [1]), "InlineBase64");

    private void AssertNoStaging() =>
        Assert.Empty(Directory.EnumerateDirectories(_root, ".fabric-definition-*"));

    private static void CreateLink(string path, string target, bool directory)
    {
        try
        {
            if (directory)
            {
                Directory.CreateSymbolicLink(path, target);
            }
            else
            {
                File.CreateSymbolicLink(path, target);
            }
        }
        catch (IOException exception) when (OperatingSystem.IsWindows() && (exception.HResult & 0xffff) == 1314)
        {
            Assert.Skip("Creating symbolic links requires Windows Developer Mode or the symbolic-link privilege.");
        }
    }
}
