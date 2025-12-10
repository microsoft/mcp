// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.UnitTests.Commands.CloudEndpoint;

public class CloudEndpointCreateCommandTests
{
    private readonly IStorageSyncService _service;
    private readonly ILogger<CloudEndpointCreateCommand> _logger;
    private readonly CloudEndpointCreateCommand _command;

    public CloudEndpointCreateCommandTests()
    {
        _service = Substitute.For<IStorageSyncService>();
        _logger = Substitute.For<ILogger<CloudEndpointCreateCommand>>();
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
}


