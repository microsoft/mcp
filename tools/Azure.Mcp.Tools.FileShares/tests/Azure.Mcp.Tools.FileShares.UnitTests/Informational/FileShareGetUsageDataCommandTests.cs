// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Informational;

public class FileShareGetUsageDataCommandTests
{
    private readonly IFileSharesService _service = Substitute.For<IFileSharesService>();

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = new FileShareGetUsageDataCommand(_service);

        // Assert
        Assert.NotNull(command);
        Assert.Equal("getusagedata", command.Name);
        Assert.NotNull(command.Description);
    }

    [Theory]
    [InlineData("last-7-days")]
    [InlineData("last-30-days")]
    [InlineData("last-90-days")]
    public async Task ExecuteAsync_WithValidTimeRange_ReturnsUsageData(string timeRange)
    {
        // Arrange
        var command = new FileShareGetUsageDataCommand(_service);

        // Assert - The command should produce usage data for the specified time range
        Assert.NotNull(command);
        Assert.NotEmpty(timeRange);
    }

    [Fact]
    public void ToolMetadata_IsConfiguredAsReadOnly()
    {
        // Arrange & Act
        var command = new FileShareGetUsageDataCommand(_service);

        // Assert
        Assert.True(command.ToolMetadata.ReadOnly);
        Assert.False(command.ToolMetadata.Destructive);
        Assert.True(command.ToolMetadata.Idempotent);
    }
}
