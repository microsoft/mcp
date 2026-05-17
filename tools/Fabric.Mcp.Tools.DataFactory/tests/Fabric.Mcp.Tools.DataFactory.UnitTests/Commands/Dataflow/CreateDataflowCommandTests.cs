// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DataFactory.MCP.Abstractions.Interfaces;
using Fabric.Mcp.Tools.DataFactory.Commands.Dataflow;
using global::DataFactory.MCP.Handlers.Dataflow;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fabric.Mcp.Tools.DataFactory.UnitTests.Commands.Dataflow;

public class CreateDataflowCommandTests
{
    private readonly ILogger<CreateDataflowCommand> _logger = Substitute.For<ILogger<CreateDataflowCommand>>();
    private readonly DataflowHandler _handler = new(Substitute.For<IFabricDataflowService>());
    private readonly CreateDataflowCommand _command;

    public CreateDataflowCommandTests()
    {
        _command = new CreateDataflowCommand(_logger, _handler);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create-dataflow", _command.Name);
        Assert.Equal("Create Dataflow", _command.Title);
        Assert.Contains("Creates a new dataflow", _command.Description);
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = _command.Metadata;
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Idempotent);
        Assert.False(metadata.Destructive);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.Secret);
        Assert.False(metadata.LocalRequired);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        var cmd = _command.GetCommand();
        Assert.Equal("create-dataflow", cmd.Name);
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
        Assert.Throws<ArgumentNullException>(() => new CreateDataflowCommand(null!, _handler));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHandlerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new CreateDataflowCommand(_logger, null!));
    }
}
