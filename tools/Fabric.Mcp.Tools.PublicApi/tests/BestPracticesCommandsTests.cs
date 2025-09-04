// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Fabric.Mcp.Tools.PublicApi.Commands.BestPractices;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Fabric.Mcp.Tools.PublicApi.Tests;

public class BestPracticesCommandsTests
{
    [Fact]
    public void GetExamplesCommand_HasCorrectProperties()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetExamplesCommand>();

        // Act
        var command = new GetExamplesCommand(logger);

        // Assert
        Assert.Equal("get", command.Name);
        Assert.NotEmpty(command.Description);
        Assert.Equal("Get Example File", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }

    [Fact]
    public void GetExamplesCommand_GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetExamplesCommand>();
        var command = new GetExamplesCommand(logger);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("get", systemCommand.Name);
        Assert.Contains(systemCommand.Options, opt => opt.Name == "workload-type");
        Assert.Contains(systemCommand.Options, opt => opt.Name == "file-path");
    }
}
