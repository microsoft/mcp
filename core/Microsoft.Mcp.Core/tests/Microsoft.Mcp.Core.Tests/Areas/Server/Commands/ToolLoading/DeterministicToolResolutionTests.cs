// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Tests for deterministic tool resolution (Phase 3 Workstream E).
/// These tests validate that the fallback matching works when sampling is unavailable.
/// </summary>
public class DeterministicToolResolutionTests
{
    [Fact]
    public void MatchToolFromIntent_WithExactMatch_ReturnsModerateConfidence()
    {
        // Arrange
        var intent = "Get storage accounts";
        var toolsJson = """
            [
              {"name": "storage", "description": "Azure Storage operations"},
              {"name": "compute", "description": "Azure Compute operations"}
            ]
            """;

        // Act
        var (toolName, confidence) = DeterministicToolResolution.MatchToolFromIntent(intent, toolsJson);

        // Assert
        Assert.Equal("storage", toolName);
        Assert.True(confidence > 0.25f, $"Expected confidence > 0.25, got {confidence}");
    }

    [Fact]
    public void MatchToolFromIntent_WithPartialMatch_ReturnsModerateConfidence()
    {
        // Arrange
        var intent = "List my storage";
        var toolsJson = """
            [
              {"name": "storage", "description": "Manage storage accounts"},
              {"name": "compute", "description": "Manage compute resources"}
            ]
            """;

        // Act
        var (toolName, confidence) = DeterministicToolResolution.MatchToolFromIntent(intent, toolsJson);

        // Assert
        Assert.Equal("storage", toolName);
        Assert.True(confidence > 0.3f);
    }

    [Fact]
    public void MatchToolFromIntent_WithNoMatch_ReturnsNull()
    {
        // Arrange
        var intent = "Launch spacecraft to the moon";
        var toolsJson = """
            [
              {"name": "storage", "description": "Azure Storage operations"},
              {"name": "compute", "description": "Azure Compute operations"}
            ]
            """;

        // Act
        var (toolName, confidence) = DeterministicToolResolution.MatchToolFromIntent(intent, toolsJson);

        // Assert
        Assert.Null(toolName);
        Assert.True(confidence < 0.3f);
    }

    [Fact]
    public void MatchCommandFromIntent_WithExactMatch_ReturnsHighConfidence()
    {
        // Arrange
        var intent = "Get storage account details";
        var toolName = "storage";
        var toolsJson = """
            [
              {
                "name": "storage",
                "commands": [
                  {"name": "account_get", "description": "Get storage account"},
                  {"name": "account_list", "description": "List storage accounts"}
                ]
              }
            ]
            """;

        // Act
        var (commandName, confidence) = DeterministicToolResolution.MatchCommandFromIntent(intent, toolName, toolsJson);

        // Assert
        Assert.NotNull(commandName);
        Assert.True(confidence > 0.3f, $"Expected confidence > 0.3, got {confidence}");
    }

    [Fact]
    public void MatchCommandFromIntent_WithPartialMatch_ReturnsModerateConfidence()
    {
        // Arrange
        var intent = "Show me all accounts";
        var toolName = "storage";
        var toolsJson = """
            [
              {
                "name": "storage",
                "commands": [
                  {"name": "account_get", "description": "Get a single storage account"},
                  {"name": "account_list", "description": "List all storage accounts"}
                ]
              }
            ]
            """;

        // Act
        var (commandName, confidence) = DeterministicToolResolution.MatchCommandFromIntent(intent, toolName, toolsJson);

        // Assert
        Assert.NotNull(commandName);
        Assert.True(confidence > 0.3f);
    }

    [Fact]
    public void MatchToolFromIntent_WithNullIntent_ReturnsNull()
    {
        // Arrange
        var toolsJson = """
            [
              {"name": "storage", "description": "Azure Storage operations"}
            ]
            """;

        // Act
        var (toolName, confidence) = DeterministicToolResolution.MatchToolFromIntent(null!, toolsJson);

        // Assert
        Assert.Null(toolName);
        Assert.Equal(0f, confidence);
    }

    [Fact]
    public void MatchCommandFromIntent_WithWrongTool_ReturnsNull()
    {
        // Arrange
        var intent = "Get storage account";
        var toolName = "compute"; // Wrong tool
        var toolsJson = """
            [
              {
                "name": "storage",
                "commands": [
                  {"name": "account_get", "description": "Get a storage account"}
                ]
              }
            ]
            """;

        // Act
        var (commandName, confidence) = DeterministicToolResolution.MatchCommandFromIntent(intent, toolName, toolsJson);

        // Assert
        Assert.Null(commandName);
    }

    [Fact]
    public void MatchToolFromIntent_WithSimilarNames_SelectsBestMatch()
    {
        // Arrange
        var intent = "storage account operations";
        var toolsJson = """
            [
              {"name": "storage", "description": "Azure Storage"},
              {"name": "stor", "description": "Short storage"},
              {"name": "storag", "description": "Partial storage"}
            ]
            """;

        // Act
        var (toolName, confidence) = DeterministicToolResolution.MatchToolFromIntent(intent, toolsJson);

        // Assert
        Assert.Equal("storage", toolName);
        Assert.True(confidence > 0.3f);
    }
}
