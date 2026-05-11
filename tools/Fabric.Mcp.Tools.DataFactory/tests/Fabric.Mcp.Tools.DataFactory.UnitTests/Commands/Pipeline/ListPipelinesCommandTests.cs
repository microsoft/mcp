// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DataFactory.MCP.Abstractions.Interfaces;
using Fabric.Mcp.Tools.DataFactory.Commands.Pipeline;
using global::DataFactory.MCP.Handlers.Pipeline;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fabric.Mcp.Tools.DataFactory.UnitTests.Commands.Pipeline;

public class ListPipelinesCommandTests
{
    private readonly ILogger<ListPipelinesCommand> _logger = Substitute.For<ILogger<ListPipelinesCommand>>();
    private readonly PipelineHandler _handler = new(Substitute.For<IFabricPipelineService>());
    private readonly ListPipelinesCommand _command;

    public ListPipelinesCommandTests()
    {
        _command = new ListPipelinesCommand(_logger, _handler);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list-pipelines", _command.Name);
        Assert.Equal("List Pipelines", _command.Title);
        Assert.Contains("Lists all pipelines", _command.Description);
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
        Assert.Equal("list-pipelines", cmd.Name);
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
        Assert.Throws<ArgumentNullException>(() => new ListPipelinesCommand(null!, _handler));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHandlerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ListPipelinesCommand(_logger, null!));
    }
}
