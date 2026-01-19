// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Informational;

/// <summary>
/// Unit tests for FileShareGetUsageDataCommand.
/// </summary>
public class FileShareGetUsageDataCommandTests
{
    private readonly IFileSharesService _service;
    private readonly ILogger<FileShareGetUsageDataCommand> _logger;
    private readonly FileShareGetUsageDataCommand _command;

    public FileShareGetUsageDataCommandTests()
    {
        _service = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<FileShareGetUsageDataCommand>>();
        _command = new(_logger, _service);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.NotNull(command);
        Assert.Equal("usage", command.Name);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("usage", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Get File Share Usage Data", _command.Title);
    }
}
