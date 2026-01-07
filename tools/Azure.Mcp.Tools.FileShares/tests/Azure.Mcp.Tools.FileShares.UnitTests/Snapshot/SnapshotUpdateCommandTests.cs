// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.Snapshot;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Snapshot;

/// <summary>
/// Unit tests for SnapshotUpdateCommand.
/// </summary>
public class SnapshotUpdateCommandTests
{
    private readonly IFileSharesService _service;
    private readonly ILogger<SnapshotUpdateCommand> _logger;
    private readonly SnapshotUpdateCommand _command;

    public SnapshotUpdateCommandTests()
    {
        _service = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<SnapshotUpdateCommand>>();
        _command = new(_logger, _service);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.NotNull(command);
        Assert.Equal("update", command.Name);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("update", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Update File Share Snapshot", _command.Title);
    }
}
