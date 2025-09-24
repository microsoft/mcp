// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading.Filters;
using Azure.Mcp.Core.Commands;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading.Filters;

[Trait("Area", "Server")]
[Trait("Component", "ToolLoading")]
public class FilterCombinationIntegrationTests
{
    [Fact]
    public void FilterPriorities_AreInCorrectOrder()
    {
        // Arrange
        var filters = new ICommandFilter[]
        {
            new VisibilityFilter(),        // Priority 50
            new CoreInfrastructureFilter(), // Priority 10
            new ReadOnlyFilter(true),      // Priority 40
            new ExtensionFilter(true)      // Priority 30
        };

        // Act
        var sortedFilters = filters.OrderBy(f => f.Priority).ToArray();

        // Assert
        Assert.Equal("CoreInfrastructure", sortedFilters[0].Name);
        Assert.Equal("Extension", sortedFilters[1].Name);
        Assert.Equal("ReadOnly", sortedFilters[2].Name);
        Assert.Equal("Visibility", sortedFilters[3].Name);
    }

    [Fact]
    public void FilterChain_CoreInfrastructureCommand_ReadOnlyMode_ReadOnlyCommand_AllowsThrough()
    {
        // Arrange
        var filters = new ICommandFilter[]
        {
            new CoreInfrastructureFilter(),
            new ExtensionFilter(false), // Exclude extensions
            new ReadOnlyFilter(true),   // ReadOnly mode
            new VisibilityFilter()
        };

        var command = Substitute.For<IBaseCommand>();
        command.Metadata.Returns(new ToolMetadata { ReadOnly = true });
        var commandName = "subscription_list";

        // Act
        var results = filters.Select(f => f.ShouldIncludeCommand(commandName, command)).ToArray();

        // Assert - All filters should allow this command through
        Assert.True(results[0]); // CoreInfrastructure: Yes (subscription command)
        Assert.True(results[1]); // Extension: Yes (not an extension)
        Assert.True(results[2]); // ReadOnly: Yes (marked as ReadOnly)
        Assert.True(results[3]); // Visibility: Should be true for valid command
    }

    [Fact]
    public void FilterChain_CoreInfrastructureCommand_ReadOnlyMode_NonReadOnlyCommand_BlockedByReadOnly()
    {
        // Arrange
        var filters = new ICommandFilter[]
        {
            new CoreInfrastructureFilter(),
            new ExtensionFilter(false),
            new ReadOnlyFilter(true),
            new VisibilityFilter()
        };

        var command = Substitute.For<IBaseCommand>();
        command.Metadata.Returns(new ToolMetadata { ReadOnly = false }); // Not ReadOnly
        var commandName = "subscription_list";

        // Act
        var results = filters.Select(f => f.ShouldIncludeCommand(commandName, command)).ToArray();

        // Assert
        Assert.True(results[0]);  // CoreInfrastructure: Yes
        Assert.True(results[1]);  // Extension: Yes
        Assert.False(results[2]); // ReadOnly: No (not marked as ReadOnly)
        // Note: Visibility filter would still be called but the command would be filtered out
    }

    [Fact]
    public void FilterChain_ExtensionCommand_ExtensionsDisabled_BlockedByExtension()
    {
        // Arrange
        var filters = new ICommandFilter[]
        {
            new CoreInfrastructureFilter(),
            new ExtensionFilter(false), // Extensions disabled
            new ReadOnlyFilter(false),
            new VisibilityFilter()
        };

        var command = Substitute.For<IBaseCommand>();
        command.Metadata.Returns(new ToolMetadata { ReadOnly = true });
        var commandName = "extension_azqr";

        // Act
        var results = filters.Select(f => f.ShouldIncludeCommand(commandName, command)).ToArray();

        // Assert
        Assert.False(results[0]); // CoreInfrastructure: No (not core)
        Assert.False(results[1]); // Extension: No (extensions disabled)
        Assert.True(results[2]);  // ReadOnly: Yes (not in ReadOnly mode)
        // Command would be filtered out before reaching visibility
    }

    [Fact]
    public void FilterChain_ServiceCommand_AllFiltersAllow_PassesThrough()
    {
        // Arrange
        var filters = new ICommandFilter[]
        {
            new CoreInfrastructureFilter(),
            new ExtensionFilter(true),  // Extensions enabled
            new ReadOnlyFilter(false), // Not ReadOnly mode
            new VisibilityFilter()
        };

        var command = Substitute.For<IBaseCommand>();
        command.Metadata.Returns(new ToolMetadata { ReadOnly = false });
        var commandName = "storage_account_list";

        // Act
        var results = filters.Select(f => f.ShouldIncludeCommand(commandName, command)).ToArray();

        // Assert
        Assert.False(results[0]); // CoreInfrastructure: No (not core)
        Assert.True(results[1]);  // Extension: Yes (not an extension)
        Assert.True(results[2]);  // ReadOnly: Yes (not in ReadOnly mode)
        Assert.True(results[3]);  // Visibility: Should be true for valid command
    }

    [Theory]
    [InlineData("subscription_list", true, false)] // Core command
    [InlineData("storage_account_list", false, false)] // Service command
    [InlineData("extension_azqr", false, true)] // Extension command (enabled)
    [InlineData("extension_azqr", false, false)] // Extension command (disabled)
    public void FilterChain_VariousCommands_ExpectedResults(
        string commandName,
        bool expectedCore,
        bool extensionsEnabled)
    {
        // Arrange
        var coreFilter = new CoreInfrastructureFilter();
        var extensionFilter = new ExtensionFilter(extensionsEnabled);
        var readOnlyFilter = new ReadOnlyFilter(false); // Not ReadOnly mode

        var command = Substitute.For<IBaseCommand>();
        command.Metadata.Returns(new ToolMetadata { ReadOnly = false });

        // Act & Assert
        Assert.Equal(expectedCore, coreFilter.ShouldIncludeCommand(commandName, command));

        if (commandName.StartsWith("extension_"))
        {
            Assert.Equal(extensionsEnabled, extensionFilter.ShouldIncludeCommand(commandName, command));
        }
        else
        {
            Assert.True(extensionFilter.ShouldIncludeCommand(commandName, command)); // Non-extensions always pass
        }

        Assert.True(readOnlyFilter.ShouldIncludeCommand(commandName, command)); // Not ReadOnly mode
    }

    [Fact]
    public void FilterChain_AppliedSequentially_ProducesExpectedResult()
    {
        // Arrange
        var filters = new ICommandFilter[]
        {
            new CoreInfrastructureFilter(),
            new ExtensionFilter(true),
            new ReadOnlyFilter(false),
            new VisibilityFilter()
        }.OrderBy(f => f.Priority).ToArray();

        var commands = new Dictionary<string, IBaseCommand>
        {
            ["subscription_list"] = CreateCommand(true),
            ["storage_account_list"] = CreateCommand(false),
            ["extension_azqr"] = CreateCommand(false),
            ["group_list"] = CreateCommand(true)
        };

        // Act - Apply filters except VisibilityFilter (since it depends on static method)
        var filtersExceptVisibility = filters.Take(3).ToArray();
        var results = new Dictionary<string, bool>();
        var detailedResults = new Dictionary<string, Dictionary<string, bool>>();

        foreach (var kvp in commands)
        {
            detailedResults[kvp.Key] = new Dictionary<string, bool>();
            foreach (var filter in filtersExceptVisibility)
            {
                detailedResults[kvp.Key][filter.Name] = filter.ShouldIncludeCommand(kvp.Key, kvp.Value);
            }

            var shouldInclude = filtersExceptVisibility.All(filter =>
                filter.ShouldIncludeCommand(kvp.Key, kvp.Value));
            results[kvp.Key] = shouldInclude;
        }

        // Assert - Check individual filter behavior first
        // Core Infrastructure Filter: Only includes core commands
        Assert.True(results["subscription_list"]); // Core infrastructure
        Assert.True(results["group_list"]);        // Core infrastructure
        Assert.False(results["storage_account_list"]); // Service command (not core infrastructure)
        Assert.False(results["extension_azqr"]);    // Extension (not core infrastructure)
    }

    private static IBaseCommand CreateCommand(bool readOnly)
    {
        var command = Substitute.For<IBaseCommand>();
        command.Metadata.Returns(new ToolMetadata { ReadOnly = readOnly });
        return command;
    }
}
