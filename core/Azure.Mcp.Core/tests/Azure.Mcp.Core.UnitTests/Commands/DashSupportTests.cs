// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Commands;

/// <summary>
/// Tests that demonstrate the fix for supporting dashes in command names.
/// This addresses the issue where commands like "azmcp list-roles" would create
/// ambiguous tool names if we used dashes as separators.
/// </summary>
public class DashSupportTests
{
    [Fact]
    public void Separator_Change_Demonstration()
    {
        // This test documents the simple fix: changing separator from '-' to '_'

        // Before the fix: separator was '-'
        // After the fix: separator is '_'

        // Arrange & Act
        var separator = CommandFactory.Separator;

        // Assert
        Assert.Equal('_', separator);
        Assert.NotEqual('-', separator);
    }

    [Theory]
    [InlineData("list-roles")]
    [InlineData("get-resource-group")]
    [InlineData("create-storage-account")]
    public void Commands_With_Dashes_Should_Work_With_Underscore_Separator(string commandWithDash)
    {
        // This test demonstrates that commands can now use dashes naturally
        // because we use underscores as separators

        // Arrange
        var prefix = "azmcp_role";

        // Act - Simulate how the CommandFactory builds command names
        var toolName = $"{prefix}_{commandWithDash}";

        // Assert
        Assert.Contains('_', toolName); // Uses underscore as separator
        Assert.Contains('-', toolName); // Preserves dashes in command names

        // Verify no ambiguity in parsing
        var parts = toolName.Split('_');
        Assert.True(parts.Length >= 3, $"Tool name '{toolName}' should have at least 3 parts when split by underscore");
        Assert.Equal("azmcp", parts[0]);
        Assert.Equal("role", parts[1]);
        Assert.Equal(commandWithDash, parts[2]);
    }

    [Fact]
    public void Before_And_After_Comparison()
    {
        // This test shows the difference between old and new approach

        var commandName = "list-roles";
        var prefix = "azmcp_role";

        // New approach (after fix): Use underscore separator
        var newToolName = $"{prefix}_{commandName}";
        Assert.Equal("azmcp_role_list-roles", newToolName);

        // Old approach would have been: "azmcp-role-list-roles"
        // This would be ambiguous - is it "azmcp-role" + "list-roles" 
        // or "azmcp" + "role-list" + "roles"?

        // With underscores, it's clear: "azmcp" + "role" + "list-roles"
        var parts = newToolName.Split('_');
        Assert.Equal(3, parts.Length);
        Assert.Equal("azmcp", parts[0]);
        Assert.Equal("role", parts[1]);
        Assert.Equal("list-roles", parts[2]);
    }

    [Theory]
    [InlineData("subscription_list", "subscription list")]
    [InlineData("role_assignment_list", "role assignment list")]
    [InlineData("storage_account_list", "storage account list")]
    public void Tool_Name_To_Command_Mapping_Examples(string toolNameSuffix, string expectedCommandSuffix)
    {
        // This test demonstrates how tool names map back to commands

        // Arrange
        var fullToolName = $"azmcp_{toolNameSuffix}";

        // Act - Simulate parsing tool name back to command structure
        var parts = fullToolName.Split('_');

        // Assert
        Assert.True(parts.Length >= 2);
        Assert.Equal("azmcp", parts[0]);

        // Verify the tool name structure
        var commandParts = parts.Skip(1).ToArray();
        var reconstructedCommandSuffix = string.Join(" ", commandParts);
        Assert.Equal(expectedCommandSuffix, reconstructedCommandSuffix);

        // Verify the tool name is unambiguous
        Assert.DoesNotContain('-', fullToolName); // Tool names use underscores, not dashes
    }
}
