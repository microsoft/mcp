// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DataFactory.MCP.Abstractions.Interfaces;
using Fabric.Mcp.Tools.DataFactory.Commands.Dataflow;
using global::DataFactory.MCP.Handlers.Dataflow;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fabric.Mcp.Tools.DataFactory.UnitTests.Commands.Dataflow;

public class ListDataflowsCommandTests
{
    private readonly ILogger<ListDataflowsCommand> _logger = Substitute.For<ILogger<ListDataflowsCommand>>();
    private readonly DataflowHandler _handler = new(Substitute.For<IFabricDataflowService>());
    private readonly ListDataflowsCommand _command;

    public ListDataflowsCommandTests()
    {
        _command = new ListDataflowsCommand(_logger, _handler);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list-dataflows", _command.Name);
        Assert.Equal("List Dataflows", _command.Title);
        Assert.Contains("Lists all dataflows", _command.Description);
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = _command.Metadata;
        Assert.True(metadata.ReadOnly);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.Destructive);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.Secret);
        Assert.False(metadata.LocalRequired);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        var cmd = _command.GetCommand();
        Assert.Equal("list-dataflows", cmd.Name);
        Assert.NotNull(cmd.Description);
    }

    [Fact]
    public void CommandOptions_ContainsRequiredOptions()
    {
        var cmd = _command.GetCommand();
        Assert.NotEmpty(cmd.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ListDataflowsCommand(null!, _handler));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHandlerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ListDataflowsCommand(_logger, null!));
    }
}
