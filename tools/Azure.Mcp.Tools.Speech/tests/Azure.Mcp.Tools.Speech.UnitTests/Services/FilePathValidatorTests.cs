// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Speech.Services;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Services;

public class FilePathValidatorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly List<string> _filesToCleanup = [];

    public FilePathValidatorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"FilePathValidatorTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        foreach (var file in _filesToCleanup.Where(File.Exists))
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    private string CreateTempFile(string relativePath)
    {
        var fullPath = Path.Combine(_tempDir, relativePath);
        var dir = Path.GetDirectoryName(fullPath)!;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(fullPath, "test content");
        _filesToCleanup.Add(fullPath);
        return fullPath;
    }

    [Fact]
    public void ValidateAndCanonicalize_ValidLocalPath_ReturnsCanonicalPath()
    {
        var testFile = CreateTempFile("audio.wav");
        var result = FilePathValidator.ValidateAndCanonicalize(testFile);
        Assert.Equal(Path.GetFullPath(testFile), result);
    }

    [Fact]
    public void ValidateAndCanonicalize_RelativePath_ResolvesToAbsolute()
    {
        // A relative path should be resolved to an absolute one
        var result = FilePathValidator.ValidateAndCanonicalize("somefile.wav");
        Assert.True(Path.IsPathRooted(result));
    }

    [Fact]
    public void ValidateAndCanonicalize_PathWithDotDot_ResolvesCorrectly()
    {
        var testFile = CreateTempFile("sub/audio.wav");
        var pathWithTraversal = Path.Combine(_tempDir, "sub", "..", "sub", "audio.wav");
        var result = FilePathValidator.ValidateAndCanonicalize(pathWithTraversal);
        Assert.Equal(Path.GetFullPath(testFile), result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateAndCanonicalize_EmptyOrWhitespace_ThrowsArgumentException(
        string? path
    )
    {
        var ex = Assert.Throws<ArgumentException>(
            () => FilePathValidator.ValidateAndCanonicalize(path!)
        );
        Assert.Contains("empty or whitespace", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateAndCanonicalize_NullByte_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => FilePathValidator.ValidateAndCanonicalize("audio\0.wav")
        );
        Assert.Contains("invalid characters", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(@"\\server\share\file.wav")]
    [InlineData("//server/share/file.wav")]
    public void ValidateAndCanonicalize_UncPath_ThrowsArgumentException(string path)
    {
        var ex = Assert.Throws<ArgumentException>(
            () => FilePathValidator.ValidateAndCanonicalize(path)
        );
        Assert.Contains("UNC", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(@"\\?\C:\file.wav")]
    [InlineData(@"\\.\C:\file.wav")]
    public void ValidateAndCanonicalize_DevicePath_ThrowsArgumentException(string path)
    {
        var ex = Assert.Throws<ArgumentException>(
            () => FilePathValidator.ValidateAndCanonicalize(path)
        );
        Assert.Contains("not allowed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateAndCanonicalize_WithAllowedDirectory_PathInside_Succeeds()
    {
        var testFile = CreateTempFile("audio.wav");
        var result = FilePathValidator.ValidateAndCanonicalize(testFile, [_tempDir]);
        Assert.Equal(Path.GetFullPath(testFile), result);
    }

    [Fact]
    public void ValidateAndCanonicalize_WithAllowedDirectory_PathOutside_Throws()
    {
        var outsidePath = Path.Combine(Path.GetTempPath(), "outside_file.wav");
        var allowedDir = Path.Combine(Path.GetTempPath(), "confined_area");

        var ex = Assert.Throws<ArgumentException>(
            () => FilePathValidator.ValidateAndCanonicalize(outsidePath, [allowedDir])
        );
        Assert.Contains("allowed directories", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateAndCanonicalize_WithMultipleAllowedDirectories_MatchesAny()
    {
        var testFile = CreateTempFile("audio.wav");
        var otherDir = Path.Combine(Path.GetTempPath(), "other_dir");

        var result = FilePathValidator.ValidateAndCanonicalize(
            testFile,
            [otherDir, _tempDir]
        );
        Assert.Equal(Path.GetFullPath(testFile), result);
    }

    [Fact]
    public void ValidateAndCanonicalize_TraversalOutOfAllowedDirectory_Throws()
    {
        // Construct a path that appears to be inside the allowed dir but traverses out
        var traversalPath = Path.Combine(_tempDir, "..", "some_other_file.wav");

        var ex = Assert.Throws<ArgumentException>(
            () => FilePathValidator.ValidateAndCanonicalize(traversalPath, [_tempDir])
        );
        Assert.Contains("allowed directories", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateAndCanonicalize_EmptyAllowedDirectories_NoConfinement()
    {
        var testFile = CreateTempFile("audio.wav");
        // Empty list means no confinement
        var result = FilePathValidator.ValidateAndCanonicalize(testFile, []);
        Assert.Equal(Path.GetFullPath(testFile), result);
    }

    [Fact]
    public void ValidateAndCanonicalize_NullAllowedDirectories_NoConfinement()
    {
        var testFile = CreateTempFile("audio.wav");
        var result = FilePathValidator.ValidateAndCanonicalize(testFile, null);
        Assert.Equal(Path.GetFullPath(testFile), result);
    }

    [Fact]
    public void ValidateAndCanonicalize_SubdirectoryOfAllowedDirectory_Succeeds()
    {
        var testFile = CreateTempFile(Path.Combine("subdir", "audio.wav"));
        var result = FilePathValidator.ValidateAndCanonicalize(testFile, [_tempDir]);
        Assert.Equal(Path.GetFullPath(testFile), result);
    }
}
