// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.UnitTests.Commands.CloudEndpoint;

public class CloudEndpointTriggerChangeDetectionCommandTests
{
    private readonly IStorageSyncService _service;
    private readonly ILogger<CloudEndpointTriggerChangeDetectionCommand> _logger;
    private readonly CloudEndpointTriggerChangeDetectionCommand _command;

    public CloudEndpointTriggerChangeDetectionCommandTests()
    {
        _service = Substitute.For<IStorageSyncService>();
        _logger = Substitute.For<ILogger<CloudEndpointTriggerChangeDetectionCommand>>();
        _command = new(_logger, _service);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.NotNull(command);
        Assert.Equal("changedetection", command.Name);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("changedetection", _command.Name);
    }
}


