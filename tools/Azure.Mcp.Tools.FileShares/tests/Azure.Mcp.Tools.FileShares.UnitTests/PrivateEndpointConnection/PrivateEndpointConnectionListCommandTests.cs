// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.PrivateEndpointConnection;

/// <summary>
/// Unit tests for PrivateEndpointConnectionListCommand.
/// </summary>
public class PrivateEndpointConnectionListCommandTests
{
    private readonly IFileSharesService _service;
    private readonly ILogger<PrivateEndpointConnectionListCommand> _logger;
    private readonly PrivateEndpointConnectionListCommand _command;

    public PrivateEndpointConnectionListCommandTests()
    {
        _service = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<PrivateEndpointConnectionListCommand>>();
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

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("List Private Endpoint Connections", _command.Title);
    }
}
