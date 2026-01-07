// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Informational;

/// <summary>
/// Unit tests for FileShareGetLimitsCommand.
/// </summary>
public class FileShareGetLimitsCommandTests
{
    private readonly IFileSharesService _service;
    private readonly ILogger<FileShareGetLimitsCommand> _logger;
    private readonly FileShareGetLimitsCommand _command;

    public FileShareGetLimitsCommandTests()
    {
        _service = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<FileShareGetLimitsCommand>>();
        _command = new(_logger, _service);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.NotNull(command);
        Assert.Equal("limits", command.Name);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("limits", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Get File Share Limits", _command.Title);
    }
}
