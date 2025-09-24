// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading.Filters;
using Azure.Mcp.Core.Commands;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading.Filters;

[Trait("Area", "Server")]
[Trait("Component", "ToolLoading")]
public class ReadOnlyFilterTests
{
    [Fact]
    public void Constructor_SetsReadOnlyModeProperty()
    {
        // Arrange & Act
        var filterReadOnly = new ReadOnlyFilter(true);
        var filterNotReadOnly = new ReadOnlyFilter(false);

        // Assert
        Assert.True(filterReadOnly.IsReadOnlyMode);
        Assert.False(filterNotReadOnly.IsReadOnlyMode);
    }

    [Fact]
    public void Properties_HaveExpectedValues()
    {
        // Arrange
        var filter = new ReadOnlyFilter(true);

        // Assert
        Assert.Equal(40, filter.Priority);
        Assert.Equal("ReadOnly", filter.Name);
    }

    [Fact]
    public void ShouldIncludeCommand_NotReadOnlyMode_AlwaysReturnsTrue()
    {
        // Arrange
        var filter = new ReadOnlyFilter(false);
        var readOnlyCommand = Substitute.For<IBaseCommand>();
        var nonReadOnlyCommand = Substitute.For<IBaseCommand>();

        // Configure commands
        readOnlyCommand.Metadata.Returns(new ToolMetadata { ReadOnly = true });
        nonReadOnlyCommand.Metadata.Returns(new ToolMetadata { ReadOnly = false });

        // Act & Assert
        Assert.True(filter.ShouldIncludeCommand("readonly_command", readOnlyCommand));
        Assert.True(filter.ShouldIncludeCommand("nonreadonly_command", nonReadOnlyCommand));
    }

    [Fact]
    public void ShouldIncludeCommand_ReadOnlyMode_ReadOnlyCommand_ReturnsTrue()
    {
        // Arrange
        var filter = new ReadOnlyFilter(true);
        var command = Substitute.For<IBaseCommand>();
        command.Metadata.Returns(new ToolMetadata { ReadOnly = true });

        // Act
        var result = filter.ShouldIncludeCommand("readonly_command", command);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldIncludeCommand_ReadOnlyMode_NonReadOnlyCommand_ReturnsFalse()
    {
        // Arrange
        var filter = new ReadOnlyFilter(true);
        var command = Substitute.For<IBaseCommand>();
        command.Metadata.Returns(new ToolMetadata { ReadOnly = false });

        // Act
        var result = filter.ShouldIncludeCommand("nonreadonly_command", command);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldIncludeCommand_NullCommand_ReturnsFalse()
    {
        // Arrange
        var filter = new ReadOnlyFilter(true);

        // Act
        var result = filter.ShouldIncludeCommand("any_command", null!);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldIncludeCommand_VariousMetadataStates_BehavesCorrectly(bool readOnlyMode)
    {
        // Arrange
        var filter = new ReadOnlyFilter(readOnlyMode);

        var readOnlyCommand = Substitute.For<IBaseCommand>();
        readOnlyCommand.Metadata.Returns(new ToolMetadata { ReadOnly = true });

        var nonReadOnlyCommand = Substitute.For<IBaseCommand>();
        nonReadOnlyCommand.Metadata.Returns(new ToolMetadata { ReadOnly = false });

        // Act & Assert
        if (readOnlyMode)
        {
            // In ReadOnly mode, only ReadOnly commands should be included
            Assert.True(filter.ShouldIncludeCommand("cmd1", readOnlyCommand));
            Assert.False(filter.ShouldIncludeCommand("cmd2", nonReadOnlyCommand));
        }
        else
        {
            // Not in ReadOnly mode, all commands should be included
            Assert.True(filter.ShouldIncludeCommand("cmd1", readOnlyCommand));
            Assert.True(filter.ShouldIncludeCommand("cmd2", nonReadOnlyCommand));
        }
    }

    [Fact]
    public void ShouldIncludeCommand_ReadOnlyMode_DefaultMetadata_ReturnsFalse()
    {
        // Arrange
        var filter = new ReadOnlyFilter(true);
        var command = Substitute.For<IBaseCommand>();
        command.Metadata.Returns(new ToolMetadata()); // Default ReadOnly = false

        // Act
        var result = filter.ShouldIncludeCommand("default_command", command);

        // Assert
        Assert.False(result);
    }
}
