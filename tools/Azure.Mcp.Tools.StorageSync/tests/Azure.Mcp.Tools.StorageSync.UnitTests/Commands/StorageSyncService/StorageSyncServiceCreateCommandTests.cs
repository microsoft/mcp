// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.UnitTests.Commands.StorageSyncService;

/// <summary>
/// Unit tests for StorageSyncServiceCreateCommand.
/// </summary>
public class StorageSyncServiceCreateCommandTests
{
    private readonly IStorageSyncService _service;
    private readonly ILogger<StorageSyncServiceCreateCommand> _logger;
    private readonly StorageSyncServiceCreateCommand _command;

    public StorageSyncServiceCreateCommandTests()
    {
        _service = Substitute.For<IStorageSyncService>();
        _logger = Substitute.For<ILogger<StorageSyncServiceCreateCommand>>();
        _command = new(_logger, _service);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.NotNull(command);
        Assert.Equal("create", command.Name);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("create", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Create Storage Sync Service", _command.Title);
    }
}

