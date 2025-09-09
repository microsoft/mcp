// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands.DataLake.Directory;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.DataLake.Directory;

public class DirectoryCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<DirectoryCreateCommand> _logger;
    private readonly DirectoryCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownAccount = "account123";
    private readonly string _knownDirectoryPath = "filesystem123/data/logs";
    private readonly string _knownSubscription = "sub123";

    public DirectoryCreateCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<DirectoryCreateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_CreatesDirectory()
    {
        // Arrange
        var expectedDirectory = new DataLakePathInfo(
            _knownDirectoryPath,
            "directory",
            null,
            DateTimeOffset.Now,
            "\"etag1\""
        );

        _storageService.CreateDirectory(
            Arg.Is(_knownAccount),
            Arg.Is(_knownDirectoryPath),
            Arg.Is(_knownSubscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>()).Returns(expectedDirectory);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--directory-path", _knownDirectoryPath,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<DirectoryCreateCommand.DirectoryCreateCommandResult>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.Directory);
        Assert.Equal(expectedDirectory.Name, result.Directory.Name);
        Assert.Equal(expectedDirectory.Type, result.Directory.Type);
        Assert.Equal("directory", result.Directory.Type);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _storageService.CreateDirectory(
            Arg.Is(_knownAccount),
            Arg.Is(_knownDirectoryPath),
            Arg.Is(_knownSubscription),
            null,
            Arg.Any<RetryPolicyOptions>()).ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--directory-path", _knownDirectoryPath,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--directory-path filesystem123/data/logs --subscription sub123", false)] // Missing account
    [InlineData("--account account123 --subscription sub123", false)] // Missing directory-path
    [InlineData("--account account123 --directory-path filesystem123/data/logs", false)] // Missing subscription
    [InlineData("--account account123 --directory-path filesystem123/data/logs --subscription sub123", true)] // Valid
    public async Task ExecuteAsync_ValidatesRequiredParameters(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedDirectory = new DataLakePathInfo("filesystem123/data/logs", "directory", null, DateTimeOffset.Now, "\"etag1\"");
            _storageService.CreateDirectory(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>()).Returns(expectedDirectory);
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

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Create a directory", command.Description);
    }
}
