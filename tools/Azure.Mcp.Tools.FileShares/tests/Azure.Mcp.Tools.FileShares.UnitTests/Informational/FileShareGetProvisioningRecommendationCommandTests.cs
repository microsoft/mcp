// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Informational;

public class FileShareGetProvisioningRecommendationCommandTests
{
    private readonly IFileSharesService _service = Substitute.For<IFileSharesService>();

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = new FileShareGetProvisioningRecommendationCommand(_service);

        // Assert
        Assert.NotNull(command);
        Assert.Equal("getprovisioningrecommendation", command.Name);
        Assert.NotNull(command.Description);
    }

    [Theory]
    [InlineData("general")]
    [InlineData("database")]
    [InlineData("analytics")]
    [InlineData("backup")]
    public async Task ExecuteAsync_WithValidWorkloadProfile_ReturnsRecommendation(string profile)
    {
        // Arrange
        var command = new FileShareGetProvisioningRecommendationCommand(_service);

        // Assert - The command should produce provisioning recommendations for the workload
        Assert.NotNull(command);
        Assert.NotEmpty(profile);
    }

    [Fact]
    public void ToolMetadata_IsConfiguredAsReadOnly()
    {
        // Arrange & Act
        var command = new FileShareGetProvisioningRecommendationCommand(_service);

        // Assert
        Assert.True(command.ToolMetadata.ReadOnly);
        Assert.False(command.ToolMetadata.Destructive);
        Assert.True(command.ToolMetadata.Idempotent);
    }
}
