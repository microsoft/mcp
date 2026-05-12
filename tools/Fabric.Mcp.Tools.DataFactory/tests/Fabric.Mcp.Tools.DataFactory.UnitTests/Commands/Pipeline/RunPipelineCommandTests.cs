// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DataFactory.MCP.Abstractions.Interfaces;
using Fabric.Mcp.Tools.DataFactory.Commands.Pipeline;
using global::DataFactory.MCP.Handlers.Pipeline;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fabric.Mcp.Tools.DataFactory.UnitTests.Commands.Pipeline;

public class RunPipelineCommandTests
{
    private readonly ILogger<RunPipelineCommand> _logger = Substitute.For<ILogger<RunPipelineCommand>>();
    private readonly PipelineHandler _handler = new(Substitute.For<IFabricPipelineService>());
    private readonly RunPipelineCommand _command;

    public RunPipelineCommandTests()
    {
        _command = new RunPipelineCommand(_logger, _handler);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("run-pipeline", _command.Name);
        Assert.Equal("Run Pipeline", _command.Title);
        Assert.Contains("Triggers a run of a specified pipeline", _command.Description);
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
        Assert.Equal("run-pipeline", cmd.Name);
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
        Assert.Throws<ArgumentNullException>(() => new RunPipelineCommand(null!, _handler));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHandlerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new RunPipelineCommand(_logger, null!));
    }
}
