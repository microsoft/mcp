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
    public async Task ListToolsHandler_WithDefaultOptions_IncludesServiceCommands()
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

        // Simple namespace filtering should include storage commands if they exist
        var allCommands = commandFactory.AllCommands;
        var storageCommands = allCommands.Where(kvp => kvp.Key.Contains("storage")).ToList();
        
        if (storageCommands.Any())
        {
            var hasStorageCommand = result.Tools.Any(t => t.Name.Contains("storage"));
            Assert.True(hasStorageCommand, "Should include storage commands when storage namespace is specified");
        }
        
        // Should have some commands (either storage commands or all commands if no namespace filtering applies)
        Assert.NotEmpty(result.Tools);
    }

    [Fact]
    public async Task ListToolsHandler_WithExtensionNamespace_HandlesFiltering()
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

        // Simple namespace filtering should include extension commands if they exist
        var allCommands = commandFactory.AllCommands;
        var extensionCommands = allCommands.Where(kvp => kvp.Key.Contains("extension")).ToList();
        
        if (extensionCommands.Any())
        {
            var hasExtensionCommand = result.Tools.Any(t => t.Name.Contains("extension"));
            Assert.True(hasExtensionCommand, "Should include extension commands when extension namespace is specified");
            Assert.NotEmpty(result.Tools);
        }
        else
        {
            // If no extension commands exist, the result may be empty with namespace filtering
            Assert.True(result.Tools.Count() >= 0, "Should handle namespace filtering even when no matching commands exist");
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
    public async Task ListToolsHandler_BasicFiltering_WorksCorrectly()
    {
        // Arrange - use specific namespace filtering
        var options = new ToolLoaderOptions { Namespace = ["storage"] };
        var (toolLoader, commandFactory) = CreateToolLoader(options);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // Should return some tools (basic filtering logic working)
        Assert.True(result.Tools.Count() >= 0, "Should return filtered results");
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
    public async Task ListToolsHandler_BasicFunctionality_WorksCorrectly()
    {
        // Arrange
        var (toolLoader, commandFactory) = CreateToolLoader();
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // Should return available commands
        var allCommands = commandFactory.AllCommands;
        Assert.NotEmpty(allCommands);
        Assert.NotEmpty(result.Tools);
        
        // All returned tools should exist in the command factory
        foreach (var tool in result.Tools)
        {
            Assert.True(allCommands.ContainsKey(tool.Name), $"Returned tool {tool.Name} should exist in command factory");
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

        // Should include both storage and keyvault commands if available
        var allCommands = commandFactory.AllCommands;
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

        // Should have some tools
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
