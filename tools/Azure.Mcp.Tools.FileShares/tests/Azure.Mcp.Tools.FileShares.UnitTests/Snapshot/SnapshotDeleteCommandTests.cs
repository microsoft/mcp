// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.Snapshot;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Snapshot;

/// <summary>
/// Unit tests for SnapshotDeleteCommand.
/// </summary>
public class SnapshotDeleteCommandTests
{
    private readonly IFileSharesService _service;
    private readonly ILogger<SnapshotDeleteCommand> _logger;
    private readonly SnapshotDeleteCommand _command;

    public SnapshotDeleteCommandTests()
    {
        _service = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<SnapshotDeleteCommand>>();
        _command = new(_logger, _service);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.NotNull(command);
        Assert.Equal("delete", command.Name);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("delete", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Delete File Share Snapshot", _command.Title);
    }
}
