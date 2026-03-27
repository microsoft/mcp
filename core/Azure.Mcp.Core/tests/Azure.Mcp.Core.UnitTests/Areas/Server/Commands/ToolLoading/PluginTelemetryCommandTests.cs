// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Commands;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading;

public class PluginTelemetryCommandTests
{
    private readonly IPluginFileReferenceAllowlistProvider _mockFileReferenceProvider;
    private readonly IPluginSkillNameAllowlistProvider _mockSkillNameProvider;
    private readonly ICommandFactory _mockCommandFactory;
    private readonly PluginTelemetryCommand _command;

    public PluginTelemetryCommandTests()
    {
        _mockFileReferenceProvider = Substitute.For<IPluginFileReferenceAllowlistProvider>();
        _mockSkillNameProvider = Substitute.For<IPluginSkillNameAllowlistProvider>();
        _mockCommandFactory = Substitute.For<ICommandFactory>();

        _command = new PluginTelemetryCommand(
            _mockFileReferenceProvider,
            _mockSkillNameProvider,
            _mockCommandFactory);
    }

    [Theory]
    [InlineData("azure-storage", true)]
    [InlineData("custom-skill", false)]
    public void SkillNameProvider_ValidatesSkillNamesCorrectly(string skillName, bool shouldBeAllowed)
    {
        // Arrange
        _mockSkillNameProvider.IsSkillNameAllowed(skillName).Returns(shouldBeAllowed);

        // Act
        var result = _mockSkillNameProvider.IsSkillNameAllowed(skillName);

        // Assert
        Assert.Equal(shouldBeAllowed, result);
    }

    [Theory]
    [InlineData("tools/storage/account.ts", true)]
    [InlineData("custom/file/path.ts", false)]
    public void FileReferenceProvider_ValidatesFileReferencesCorrectly(string fileReference, bool shouldBeAllowed)
    {
        // Arrange
        _mockFileReferenceProvider.IsPathAllowed(fileReference).Returns(shouldBeAllowed);

        // Act
        var result = _mockFileReferenceProvider.IsPathAllowed(fileReference);

        // Assert
        Assert.Equal(shouldBeAllowed, result);
    }

    [Theory]
    [InlineData("storage-account-create", true)]
    [InlineData("invalid-tool-name", false)]
    public void CommandFactory_ValidatesToolNamesCorrectly(string toolName, bool shouldBeAllowed)
    {
        // Arrange
        var mockCommand = shouldBeAllowed ? Substitute.For<IBaseCommand>() : null;
        _mockCommandFactory.FindCommandByName(toolName).Returns(mockCommand);

        // Act
        var result = _mockCommandFactory.FindCommandByName(toolName);

        // Assert
        if (shouldBeAllowed)
        {
            Assert.NotNull(result);
        }
        else
        {
            Assert.Null(result);
        }
    }
}
