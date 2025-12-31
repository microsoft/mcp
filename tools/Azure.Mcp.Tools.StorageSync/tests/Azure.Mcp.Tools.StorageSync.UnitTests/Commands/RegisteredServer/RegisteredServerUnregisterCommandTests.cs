// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.StorageSync.Commands.RegisteredServer;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.UnitTests.Commands.RegisteredServer;

public class RegisteredServerUnregisterCommandTests
{
    private readonly IStorageSyncService _service;
    private readonly ILogger<RegisteredServerUnregisterCommand> _logger;
    private readonly RegisteredServerUnregisterCommand _command;

    public RegisteredServerUnregisterCommandTests()
    {
        _service = Substitute.For<IStorageSyncService>();
        _logger = Substitute.For<ILogger<RegisteredServerUnregisterCommand>>();
        _command = new(_logger, _service);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.NotNull(command);
        Assert.Equal("unregister", command.Name);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("unregister", _command.Name);
    }
}


