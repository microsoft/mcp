// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.DataLake.FileSystem;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.DataLake.FileSystem;

public class FileSystemListPathsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<FileSystemListPathsCommand> _logger;
    private readonly FileSystemListPathsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownAccount = "account123";
    private readonly string _knownFileSystem = "filesystem123";
    private readonly string _knownSubscription = "sub123";

    public FileSystemListPathsCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<FileSystemListPathsCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsPaths()
    {
        // Arrange
        var expectedPaths = new List<DataLakePathInfo>
        {
            new("file1.txt", "file", 1024, DateTimeOffset.Now, "\"etag1\""),
            new("directory1", "directory", null, DateTimeOffset.Now, "\"etag2\"")
        };

        _storageService.ListDataLakePaths(Arg.Is(_knownAccount), Arg.Is(_knownFileSystem), false, Arg.Is(_knownSubscription),
            null, Arg.Any<string>(), Arg.Any<RetryPolicyOptions>()).Returns(expectedPaths);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--file-system", _knownFileSystem,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.FileSystemListPathsCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedPaths.Count, result.Paths.Count);
        Assert.Equal(expectedPaths[0].Name, result.Paths[0].Name);
        Assert.Equal(expectedPaths[0].Type, result.Paths[0].Type);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyArray_WhenNoPaths()
    {
        // Arrange
        _storageService.ListDataLakePaths(Arg.Is(_knownAccount), Arg.Is(_knownFileSystem), false, Arg.Is(_knownSubscription),
            null, Arg.Any<string>(), Arg.Any<RetryPolicyOptions>()).Returns([]);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--file-system", _knownFileSystem,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.FileSystemListPathsCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Paths);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _storageService.ListDataLakePaths(Arg.Is(_knownAccount), Arg.Is(_knownFileSystem), false, Arg.Is(_knownSubscription),
            null, null, Arg.Any<RetryPolicyOptions>()).ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--file-system", _knownFileSystem,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithFilterPath_FiltersResults()
    {
        // Arrange
        var filterPath = "folder1/";
        var expectedPaths = new List<DataLakePathInfo>
        {
            new("folder1/file1.txt", "file", 1024, DateTimeOffset.Now, "\"etag1\""),
            new("folder1/subfolder", "directory", null, DateTimeOffset.Now, "\"etag2\"")
        };

        _storageService.ListDataLakePaths(Arg.Is(_knownAccount), Arg.Is(_knownFileSystem), false,
            Arg.Is(_knownSubscription), Arg.Is(filterPath), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedPaths);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--file-system", _knownFileSystem,
            "--subscription", _knownSubscription,
            "--filter-path", filterPath
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.FileSystemListPathsCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedPaths.Count, result.Paths.Count);
        Assert.All(result.Paths, path => Assert.StartsWith(filterPath, path.Name));
    }

    [Fact]
    public async Task ExecuteAsync_WithRecursiveTrue_ReturnsAllPaths()
    {
        // Arrange
        var expectedPaths = new List<DataLakePathInfo>
        {
            new("file1.txt", "file", 1024, DateTimeOffset.Now, "\"etag1\""),
            new("folder1", "directory", null, DateTimeOffset.Now, "\"etag2\""),
            new("folder1/file2.txt", "file", 2048, DateTimeOffset.Now, "\"etag3\""),
            new("folder1/subfolder", "directory", null, DateTimeOffset.Now, "\"etag4\""),
            new("folder1/subfolder/file3.txt", "file", 512, DateTimeOffset.Now, "\"etag5\"")
        };

        _storageService.ListDataLakePaths(Arg.Is(_knownAccount), Arg.Is(_knownFileSystem), true,
            Arg.Is(_knownSubscription), null, Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedPaths);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--file-system", _knownFileSystem,
            "--subscription", _knownSubscription,
            "--recursive"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.FileSystemListPathsCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedPaths.Count, result.Paths.Count);
        // Verify we get both top-level and nested paths
        Assert.Contains(result.Paths, p => p.Name == "file1.txt");
        Assert.Contains(result.Paths, p => p.Name == "folder1/file2.txt");
        Assert.Contains(result.Paths, p => p.Name == "folder1/subfolder/file3.txt");
    }

    [Fact]
    public async Task ExecuteAsync_WithRecursiveFalse_ReturnsOnlyTopLevelPaths()
    {
        // Arrange
        var expectedPaths = new List<DataLakePathInfo>
        {
            new("file1.txt", "file", 1024, DateTimeOffset.Now, "\"etag1\""),
            new("folder1", "directory", null, DateTimeOffset.Now, "\"etag2\"")
        };

        _storageService.ListDataLakePaths(Arg.Is(_knownAccount), Arg.Is(_knownFileSystem), false,
            Arg.Is(_knownSubscription), null, Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedPaths);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--file-system", _knownFileSystem,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.FileSystemListPathsCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedPaths.Count, result.Paths.Count);
        // Verify we only get top-level paths (no nested paths)
        Assert.All(result.Paths, path => Assert.DoesNotContain("/", path.Name.Substring(1)));
    }

    [Fact]
    public async Task ExecuteAsync_WithFilterPathAndRecursive_CombinesOptions()
    {
        // Arrange
        var filterPath = "documents/";
        var expectedPaths = new List<DataLakePathInfo>
        {
            new("documents/file1.pdf", "file", 1024, DateTimeOffset.Now, "\"etag1\""),
            new("documents/archive", "directory", null, DateTimeOffset.Now, "\"etag2\""),
            new("documents/archive/old.pdf", "file", 2048, DateTimeOffset.Now, "\"etag3\"")
        };

        _storageService.ListDataLakePaths(Arg.Is(_knownAccount), Arg.Is(_knownFileSystem), true,
            Arg.Is(_knownSubscription), Arg.Is(filterPath), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedPaths);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--file-system", _knownFileSystem,
            "--subscription", _knownSubscription,
            "--filter-path", filterPath,
            "--recursive"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.FileSystemListPathsCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedPaths.Count, result.Paths.Count);
        Assert.All(result.Paths, path => Assert.StartsWith(filterPath, path.Name));
        // Verify we get both filtered and nested paths
        Assert.Contains(result.Paths, p => p.Name == "documents/file1.pdf");
        Assert.Contains(result.Paths, p => p.Name == "documents/archive/old.pdf");
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyFilterPath_ReturnsAllPaths()
    {
        // Arrange
        var expectedPaths = new List<DataLakePathInfo>
        {
            new("file1.txt", "file", 1024, DateTimeOffset.Now, "\"etag1\""),
            new("folder1", "directory", null, DateTimeOffset.Now, "\"etag2\"")
        };

        _storageService.ListDataLakePaths(Arg.Is(_knownAccount), Arg.Is(_knownFileSystem), false,
            Arg.Is(_knownSubscription), Arg.Is(""), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedPaths);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--file-system", _knownFileSystem,
            "--subscription", _knownSubscription,
            "--filter-path", ""
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.FileSystemListPathsCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedPaths.Count, result.Paths.Count);
    }

    [Theory]
    [InlineData("--file-system filesystem123 --subscription sub123", false)] // Missing account
    [InlineData("--account account123 --subscription sub123", false)] // Missing file-system
    [InlineData("--account account123 --file-system filesystem123", false)] // Missing subscription
    [InlineData("--account account123 --file-system filesystem123 --subscription sub123", true)] // Valid
    public async Task ExecuteAsync_ValidatesRequiredParameters(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _storageService.ListDataLakePaths(Arg.Any<string>(), Arg.Any<string>(), false, Arg.Any<string>(),
                null, Arg.Any<string>(), Arg.Any<RetryPolicyOptions>()).Returns([]);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }
}
