// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Storage.Commands.Share.File;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Share.File;

public class FileListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<FileListCommand> _logger;
    private readonly FileListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public FileListCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<FileListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        // Act & Assert
        Assert.Equal("list", _command.Name);
    }

    [Fact]
    public void Title_ReturnsExpectedValue()
    {
        // Act & Assert
        Assert.Equal("List Storage Share Files and Directories", _command.Title);
    }

    [Fact]
    public void Metadata_IsReadOnly()
    {
        // Act
        var metadata = _command.Metadata;

        // Assert
        Assert.False(metadata.Destructive);
        Assert.True(metadata.ReadOnly);
    }
}
