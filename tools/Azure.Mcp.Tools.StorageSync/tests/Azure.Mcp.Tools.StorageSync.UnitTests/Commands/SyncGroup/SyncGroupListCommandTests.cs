// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.StorageSync.Commands.SyncGroup;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.UnitTests.Commands.SyncGroup;

public class SyncGroupListCommandTests
{
    private readonly IStorageSyncService _service;
    private readonly ILogger<SyncGroupListCommand> _logger;
    private readonly SyncGroupListCommand _command;

    public SyncGroupListCommandTests()
    {
        _service = Substitute.For<IStorageSyncService>();
        _logger = Substitute.For<ILogger<SyncGroupListCommand>>();
        _command = new(_logger, _service);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.NotNull(command);
        Assert.Equal("list", command.Name);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("list", _command.Name);
    }
}


