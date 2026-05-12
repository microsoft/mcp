// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DataFactory.MCP.Abstractions.Interfaces;
using Fabric.Mcp.Tools.DataFactory.Commands.Dataflow;
using global::DataFactory.MCP.Handlers.Dataflow;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fabric.Mcp.Tools.DataFactory.UnitTests.Commands.Dataflow;

public class ExecuteQueryCommandTests
{
    private readonly ILogger<ExecuteQueryCommand> _logger = Substitute.For<ILogger<ExecuteQueryCommand>>();
    private readonly DataflowQueryHandler _handler = new(Substitute.For<IFabricDataflowService>());
    private readonly ExecuteQueryCommand _command;

    public ExecuteQueryCommandTests()
    {
        _command = new ExecuteQueryCommand(_logger, _handler);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("execute-query", _command.Name);
        Assert.Equal("Execute Dataflow Query", _command.Title);
        Assert.Contains("Executes an M (Power Query) expression", _command.Description);
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
        Assert.Equal("execute-query", cmd.Name);
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
        Assert.Throws<ArgumentNullException>(() => new ExecuteQueryCommand(null!, _handler));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHandlerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ExecuteQueryCommand(_logger, null!));
    }
}
