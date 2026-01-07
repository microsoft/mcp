// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.FileShare;

/// <summary>
/// Unit tests for FileShareCheckNameAvailabilityCommand.
/// </summary>
public class FileShareCheckNameAvailabilityCommandTests
{
    private readonly IFileSharesService _service;
    private readonly ILogger<FileShareCheckNameAvailabilityCommand> _logger;
    private readonly FileShareCheckNameAvailabilityCommand _command;

    public FileShareCheckNameAvailabilityCommandTests()
    {
        _service = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<FileShareCheckNameAvailabilityCommand>>();
        _command = new(_logger, _service);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.NotNull(command);
        Assert.Equal("check-name-availability", command.Name);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("check-name-availability", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Check File Share Name Availability", _command.Title);
    }
}
