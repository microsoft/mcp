// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using ModelContextProtocol.Server;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server;

public class McpServerToolAttributeTests
{
    [Fact]
    public void AllExecuteAsyncMethodsWithMcpServerToolAttribute_ShouldHaveValidTitle()
    {
        // Arrange
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();

        var titleValidationErrors = new List<string>();

        // Act - Get all command types and check their ExecuteAsync methods
        foreach (var (commandName, command) in commandFactory.AllCommands)
        {
            // Get the ExecuteAsync method
            var executeAsyncMethod = command.GetType().GetMethod("ExecuteAsync");

            if (executeAsyncMethod == null)
                continue;

            // Check if the method has the McpServerTool attribute
            var mcpServerToolAttribute = executeAsyncMethod.GetCustomAttribute<McpServerToolAttribute>();

            if (mcpServerToolAttribute == null)
                continue;

            var commandTypeName = command.GetType().FullName;

            // Check 1: Title property must not be null or whitespace
            if (string.IsNullOrWhiteSpace(mcpServerToolAttribute.Title))
            {
                titleValidationErrors.Add($"{commandTypeName}: Missing or empty Title property");
                continue; // Skip further validation if title is null/empty
            }

            var title = mcpServerToolAttribute.Title.Trim();

            // Check 2: Title must not be just whitespace after trimming
            if (title.Length == 0)
            {
                titleValidationErrors.Add($"{commandTypeName}: Title contains only whitespace");
                continue;
            }

            // Check 3: Title should be reasonably descriptive (at least 5 characters)
            if (title.Length < 5)
            {
                titleValidationErrors.Add($"{commandTypeName}: Title too short ('{title}') - should be at least 5 characters");
            }

            // Check 4: Title should not be generic/placeholder
            var genericTitles = new[] { "TODO", "PLACEHOLDER", "FIXME", "TBD", "Command", "Tool" };
            if (genericTitles.Any(generic => title.Equals(generic, StringComparison.OrdinalIgnoreCase)))
            {
                titleValidationErrors.Add($"{commandTypeName}: Title is generic placeholder ('{title}')");
            }
        }

        // Assert
        Assert.True(titleValidationErrors.Count == 0,
            $"The following commands have ExecuteAsync methods with invalid McpServerTool Title properties:\n" +
            string.Join("\n", titleValidationErrors));
    }
}
