// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.PrivateEndpointConnection;

/// <summary>
/// Unit tests for PrivateEndpointConnectionUpdateCommand.
/// </summary>
public class PrivateEndpointConnectionUpdateCommandTests
{
    private readonly IFileSharesService _service;
    private readonly ILogger<PrivateEndpointConnectionUpdateCommand> _logger;
    private readonly PrivateEndpointConnectionUpdateCommand _command;

    public PrivateEndpointConnectionUpdateCommandTests()
    {
        _service = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<PrivateEndpointConnectionUpdateCommand>>();
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
        Assert.Equal("Update Private Endpoint Connection", _command.Title);
    }
}
