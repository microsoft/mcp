// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.PrivateEndpointConnection;

/// <summary>
/// Unit tests for PrivateEndpointConnectionDeleteCommand.
/// </summary>
public class PrivateEndpointConnectionDeleteCommandTests
{
    private readonly IFileSharesService _service;
    private readonly ILogger<PrivateEndpointConnectionDeleteCommand> _logger;
    private readonly PrivateEndpointConnectionDeleteCommand _command;

    public PrivateEndpointConnectionDeleteCommandTests()
    {
        _service = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<PrivateEndpointConnectionDeleteCommand>>();
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
        Assert.Equal("Delete Private Endpoint Connection", _command.Title);
    }
}
