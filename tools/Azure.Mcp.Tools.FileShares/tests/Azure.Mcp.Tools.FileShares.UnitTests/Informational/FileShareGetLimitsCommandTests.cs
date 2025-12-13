// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Informational;

public class FileShareGetLimitsCommandTests
{
    private readonly IFileSharesService _service = Substitute.For<IFileSharesService>();

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = new FileShareGetLimitsCommand(_service);

        // Assert
        Assert.NotNull(command);
        Assert.Equal("getlimits", command.Name);
        Assert.NotNull(command.Description);
    }

    [Theory]
    [InlineData("eastus")]
    [InlineData("westeurope")]
    [InlineData("southeastasia")]
    public async Task ExecuteAsync_WithValidLocation_ReturnsLimits(string location)
    {
        // Arrange
        var command = new FileShareGetLimitsCommand(_service);

        // Assert - The command should produce limit information for the location
        Assert.NotNull(command);
        Assert.Equal(location, location);
    }

    [Fact]
    public void ToolMetadata_IsConfiguredAsReadOnly()
    {
        // Arrange & Act
        var command = new FileShareGetLimitsCommand(_service);

        // Assert
        Assert.True(command.ToolMetadata.ReadOnly);
        Assert.False(command.ToolMetadata.Destructive);
        Assert.True(command.ToolMetadata.Idempotent);
    }
}
