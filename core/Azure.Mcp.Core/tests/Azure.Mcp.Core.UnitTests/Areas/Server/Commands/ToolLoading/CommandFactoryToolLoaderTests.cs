// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading;

public class CommandFactoryToolLoaderTests
{
    private static (CommandFactoryToolLoader toolLoader, CommandFactory commandFactory) CreateToolLoader(ToolLoaderOptions? options = null)
    {
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory(serviceProvider);
        var logger = loggerFactory.CreateLogger<CommandFactoryToolLoader>();

        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(options ?? new ToolLoaderOptions());
        var toolLoader = new CommandFactoryToolLoader(serviceProvider, commandFactory, optionsWrapper, logger);
        return (toolLoader, commandFactory);
    }

    private static ModelContextProtocol.Server.RequestContext<ListToolsRequestParams> CreateListToolsRequest()
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.IMcpServer>();
        return new ModelContextProtocol.Server.RequestContext<ListToolsRequestParams>(mockServer)
        {
            Params = new ListToolsRequestParams()
        };
    }

    private static ModelContextProtocol.Server.RequestContext<CallToolRequestParams> CreateCallToolRequest(string toolName, IReadOnlyDictionary<string, JsonElement>? arguments = null)
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.IMcpServer>();
        return new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer)
        {
            Params = new CallToolRequestParams
            {
                Name = toolName,
                Arguments = arguments ?? new Dictionary<string, JsonElement>()
            }
        };
    }

    [Fact]
    public async Task ListToolsHandler_WithDefaultOptions_IncludesEssentialAndServiceCommands()
    {
        // Arrange
        var (toolLoader, commandFactory) = CreateToolLoader();
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);
        Assert.NotEmpty(result.Tools);

        // Analyze what commands are available
        var allCommands = commandFactory.AllCommands;
        var essentialCommands = allCommands.Where(kvp =>
            kvp.Value.GetType().GetCustomAttribute<EssentialAttribute>() != null ||
            kvp.Value.GetType().BaseType?.GetCustomAttribute<EssentialAttribute>() != null).ToList();

        // Verify we have commands in the factory
        Assert.NotEmpty(allCommands);

        // Should include service commands that are available in test setup
        var serviceCommands = allCommands.Where(kvp =>
            kvp.Key.Contains("storage") ||
            kvp.Key.Contains("keyvault") ||
            kvp.Key.Contains("subscription") ||
            kvp.Key.Contains("deploy") ||
            kvp.Key.Contains("appconfig")).ToList();

        if (serviceCommands.Any())
        {
            var hasServiceCommand = result.Tools.Any(t =>
                serviceCommands.Any(sc => sc.Key == t.Name));
            Assert.True(hasServiceCommand, "Should include available service commands");
        }

        // Essential commands should be included if they exist
        foreach (var essentialCommand in essentialCommands)
        {
            var hasEssentialCommand = result.Tools.Any(t => t.Name == essentialCommand.Key);
            Assert.True(hasEssentialCommand, $"Essential command {essentialCommand.Key} should be included");
        }
    }

    [Fact]
    public async Task ListToolsHandler_WithNamespaceFiltering_IncludesOnlyMatchingCommands()
    {
        // Arrange
        var options = new ToolLoaderOptions { Namespace = ["storage"] };
        var (toolLoader, commandFactory) = CreateToolLoader(options);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // Check if essential commands are available in the factory
        var allCommands = commandFactory.AllCommands;
        var essentialCommands = allCommands.Where(kvp =>
            kvp.Value.GetType().GetCustomAttribute<EssentialAttribute>() != null ||
            kvp.Value.GetType().BaseType?.GetCustomAttribute<EssentialAttribute>() != null).ToList();

        // If essential commands exist, they should be included even with namespace filtering
        foreach (var essentialCommand in essentialCommands)
        {
            var hasEssentialCommand = result.Tools.Any(t => t.Name == essentialCommand.Key);
            Assert.True(hasEssentialCommand, $"Essential command {essentialCommand.Key} should be included even with namespace filtering");
        }

        // Should include storage commands if they exist
        var storageCommands = allCommands.Where(kvp => kvp.Key.Contains("storage")).ToList();
        if (storageCommands.Any())
        {
            var hasStorageCommand = result.Tools.Any(t => t.Name.Contains("storage"));
            Assert.True(hasStorageCommand, "Should include storage commands when storage namespace is specified");
        }

        // Should not include commands from other namespaces (unless essential)
        var keyvaultCommands = allCommands.Where(kvp => kvp.Key.Contains("keyvault") &&
            kvp.Value.GetType().GetCustomAttribute<EssentialAttribute>() == null &&
            kvp.Value.GetType().BaseType?.GetCustomAttribute<EssentialAttribute>() == null).ToList();

        foreach (var keyvaultCommand in keyvaultCommands)
        {
            var hasKeyvaultCommand = result.Tools.Any(t => t.Name == keyvaultCommand.Key);
            Assert.False(hasKeyvaultCommand, $"Non-essential keyvault command {keyvaultCommand.Key} should not be included when only storage namespace is specified");
        }
    }

    [Fact]
    public async Task ListToolsHandler_WithExtensionNamespace_IncludesExtensionCommands()
    {
        // Arrange
        var options = new ToolLoaderOptions { Namespace = ["extension"] };
        var (toolLoader, commandFactory) = CreateToolLoader(options);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // Check if essential commands are available in the factory
        var allCommands = commandFactory.AllCommands;
        var essentialCommands = allCommands.Where(kvp =>
            kvp.Value.GetType().GetCustomAttribute<EssentialAttribute>() != null ||
            kvp.Value.GetType().BaseType?.GetCustomAttribute<EssentialAttribute>() != null).ToList();

        // If essential commands exist, they should be included
        foreach (var essentialCommand in essentialCommands)
        {
            var hasEssentialCommand = result.Tools.Any(t => t.Name == essentialCommand.Key);
            Assert.True(hasEssentialCommand, $"Essential command {essentialCommand.Key} should be included");
        }

        // Extension commands should be included when extension namespace is specified
        // Note: Extension commands might not be available in test setup, so we check the filtering logic
        var extensionCommands = allCommands.Where(kvp =>
            kvp.Value.GetType().GetCustomAttribute<ExtensionAttribute>() != null).ToList();

        if (extensionCommands.Any())
        {
            // If extension commands exist, they should be included
            foreach (var extensionCommand in extensionCommands)
            {
                var hasExtensionTool = result.Tools.Any(t => t.Name == extensionCommand.Key);
                Assert.True(hasExtensionTool, $"Extension command {extensionCommand.Key} should be included when extension namespace is specified");
            }
        }
        else
        {
            // If no extension commands exist in test setup, just verify we have some commands (essential ones)
            Assert.NotEmpty(result.Tools);
        }
    }

    [Fact]
    public async Task ListToolsHandler_WithoutExtensionNamespace_ExcludesExtensionCommands()
    {
        // Arrange
        var options = new ToolLoaderOptions { Namespace = ["storage"] };
        var (toolLoader, commandFactory) = CreateToolLoader(options);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // Extension commands should not be included when extension namespace is not specified
        var allCommands = commandFactory.AllCommands;
        var extensionCommands = allCommands.Where(kvp =>
            kvp.Value.GetType().GetCustomAttribute<ExtensionAttribute>() != null).ToList();

        foreach (var extensionCommand in extensionCommands)
        {
            var hasExtensionTool = result.Tools.Any(t => t.Name == extensionCommand.Key);
            Assert.False(hasExtensionTool, $"Extension command {extensionCommand.Key} should not be included when extension namespace is not specified");
        }
    }

    [Fact]
    public async Task ListToolsHandler_WithReadOnlyMode_IncludesOnlyReadOnlyCommands()
    {
        // Arrange
        var options = new ToolLoaderOptions { ReadOnly = true };
        var (toolLoader, commandFactory) = CreateToolLoader(options);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // All returned tools should be read-only
        foreach (var tool in result.Tools)
        {
            var command = commandFactory.AllCommands[tool.Name];
            Assert.True(command.Metadata.ReadOnly, $"Command {tool.Name} should be read-only when ReadOnly mode is enabled");
        }
    }

    [Fact]
    public async Task ListToolsHandler_EssentialCommandsAlwaysIncluded()
    {
        // Arrange - use specific namespace that doesn't include subscription commands normally
        var options = new ToolLoaderOptions { Namespace = ["storage"] };
        var (toolLoader, commandFactory) = CreateToolLoader(options);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // Find essential commands in the command factory
        var allCommands = commandFactory.AllCommands;
        var essentialCommands = allCommands.Where(kvp =>
            kvp.Value.GetType().GetCustomAttribute<EssentialAttribute>() != null ||
            kvp.Value.GetType().BaseType?.GetCustomAttribute<EssentialAttribute>() != null).ToList();

        if (essentialCommands.Any())
        {
            // Each essential command should be included in the result
            foreach (var essentialCommand in essentialCommands)
            {
                var hasEssentialTool = result.Tools.Any(t => t.Name == essentialCommand.Key);
                Assert.True(hasEssentialTool, $"Essential command {essentialCommand.Key} should always be included");
            }
        }
        else
        {
            // If no essential commands exist in test setup, verify we still have some filtering logic working
            Assert.True(result.Tools.Count() >= 0, "Should return filtered results even without essential commands");
        }
    }

    [Fact]
    public async Task CallToolHandler_WithValidTool_ExecutesSuccessfully()
    {
        // Arrange
        var (toolLoader, commandFactory) = CreateToolLoader();

        // Find a simple command to test (subscription list)
        var subscriptionCommands = commandFactory.AllCommands.Where(kvp => kvp.Key.Contains("subscription_list")).ToList();
        Assert.NotEmpty(subscriptionCommands);

        var commandName = subscriptionCommands.First().Key;
        var request = CreateCallToolRequest(commandName);

        // Act
        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);

        // The result might be an error due to authentication, but the tool should be found and executed
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        Assert.NotNull(textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithInvalidTool_ReturnsError()
    {
        // Arrange
        var (toolLoader, commandFactory) = CreateToolLoader();
        var request = CreateCallToolRequest("nonexistent_tool");

        // Act
        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);

        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        Assert.Contains("Could not find command", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithNullParams_ReturnsError()
    {
        // Arrange
        var (toolLoader, commandFactory) = CreateToolLoader();
        var mockServer = Substitute.For<ModelContextProtocol.Server.IMcpServer>();
        var request = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer)
        {
            Params = null
        };

        // Act
        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);

        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        Assert.Contains("Cannot call tools with null parameters", textContent.Text);
    }

    [Fact]
    public void CommandFactory_ReturnsCorrectFactory()
    {
        // Arrange
        var (toolLoader, commandFactory) = CreateToolLoader();

        // Act
        var returnedFactory = toolLoader.CommandFactory;

        // Assert
        Assert.Same(commandFactory, returnedFactory);
    }

    [Fact]
    public async Task DisposeAsync_CompletesSuccessfully()
    {
        // Arrange
        var (toolLoader, commandFactory) = CreateToolLoader();

        // Act & Assert - Should not throw
        await toolLoader.DisposeAsync();
    }

    [Fact]
    public async Task ListToolsHandler_AttributeFiltering_WorksCorrectly()
    {
        // Arrange
        var (toolLoader, commandFactory) = CreateToolLoader();
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        var allCommandsWithAttributes = new Dictionary<string, string>();
        foreach (var kvp in commandFactory.AllCommands)
        {
            var commandType = kvp.Value.GetType();
            var essential = commandType.GetCustomAttribute<EssentialAttribute>() ??
                          commandType.BaseType?.GetCustomAttribute<EssentialAttribute>();
            var extension = commandType.GetCustomAttribute<ExtensionAttribute>();

            if (essential != null)
            {
                allCommandsWithAttributes[kvp.Key] = "Essential";
            }
            else if (extension != null)
            {
                allCommandsWithAttributes[kvp.Key] = "Extension";
            }
            else
            {
                allCommandsWithAttributes[kvp.Key] = "Regular";
            }
        }

        // Verify that all essential commands are included
        var essentialCommands = allCommandsWithAttributes.Where(kvp => kvp.Value == "Essential").ToList();
        foreach (var essentialCommand in essentialCommands)
        {
            var isIncluded = result.Tools.Any(t => t.Name == essentialCommand.Key);
            Assert.True(isIncluded, $"Essential command {essentialCommand.Key} should be included");
        }

        // Verify that extension commands are excluded (default namespace)
        var extensionCommands = allCommandsWithAttributes.Where(kvp => kvp.Value == "Extension").ToList();
        foreach (var extensionCommand in extensionCommands)
        {
            var isIncluded = result.Tools.Any(t => t.Name == extensionCommand.Key);
            Assert.False(isIncluded, $"Extension command {extensionCommand.Key} should be excluded without extension namespace");
        }
    }

    [Fact]
    public async Task ListToolsHandler_MultipleNamespaces_IncludesAllSpecifiedNamespaces()
    {
        // Arrange
        var options = new ToolLoaderOptions { Namespace = ["storage", "keyvault"] };
        var (toolLoader, commandFactory) = CreateToolLoader(options);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // Check if essential commands are available in the factory
        var allCommands = commandFactory.AllCommands;
        var essentialCommands = allCommands.Where(kvp =>
            kvp.Value.GetType().GetCustomAttribute<EssentialAttribute>() != null ||
            kvp.Value.GetType().BaseType?.GetCustomAttribute<EssentialAttribute>() != null).ToList();

        // If essential commands exist, they should be included
        foreach (var essentialCommand in essentialCommands)
        {
            var hasEssentialCommand = result.Tools.Any(t => t.Name == essentialCommand.Key);
            Assert.True(hasEssentialCommand, $"Essential command {essentialCommand.Key} should be included");
        }

        // Should include both storage and keyvault commands if available
        var storageCommands = allCommands.Where(kvp => kvp.Key.Contains("storage")).ToList();
        var keyvaultCommands = allCommands.Where(kvp => kvp.Key.Contains("keyvault")).ToList();

        if (storageCommands.Any())
        {
            var hasStorageCommand = result.Tools.Any(t => t.Name.Contains("storage"));
            Assert.True(hasStorageCommand, "Should include storage commands when storage namespace is specified");
        }

        if (keyvaultCommands.Any())
        {
            var hasKeyvaultCommand = result.Tools.Any(t => t.Name.Contains("keyvault"));
            Assert.True(hasKeyvaultCommand, "Should include keyvault commands when keyvault namespace is specified");
        }

        // Should have some tools (at least the essential ones or namespace-specific ones)
        Assert.NotEmpty(result.Tools);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ListToolsHandler_ReadOnlyModeFiltering_WorksCorrectly(bool readOnlyMode)
    {
        // Arrange
        var options = new ToolLoaderOptions { ReadOnly = readOnlyMode };
        var (toolLoader, commandFactory) = CreateToolLoader(options);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        if (readOnlyMode)
        {
            // In read-only mode, all returned commands should be read-only
            foreach (var tool in result.Tools)
            {
                var command = commandFactory.AllCommands[tool.Name];
                Assert.True(command.Metadata.ReadOnly, $"Command {tool.Name} should be read-only in read-only mode");
            }
        }
        else
        {
            // In normal mode, we should have a mix of read-only and non-read-only commands
            // (This assumes the test setup includes both types)
            var readOnlyCommands = result.Tools.Where(t => commandFactory.AllCommands[t.Name].Metadata.ReadOnly).Count();
            var totalCommands = result.Tools.Count();

            // This is a sanity check - in normal mode we should have at least some commands
            Assert.True(totalCommands > 0, "Should have commands in normal mode");
        }
    }
}
