// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.UnitTests.Server.Helpers;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading;

public class CommandFactoryToolLoaderTests
{
    private readonly MockCommandFactory _commandFactory;
    private readonly ServiceProvider _serviceProvider;
    private readonly CommandFactoryToolLoader _toolLoader;
    private readonly ILogger<CommandFactoryToolLoader> _logger;

    public CommandFactoryToolLoaderTests()
    {
        _commandFactory = new MockCommandFactory();

        _serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<CommandFactoryToolLoader>();
        var toolLoaderOptions = Options.Create(new ToolLoaderOptions());

        _toolLoader = new CommandFactoryToolLoader(_serviceProvider, _commandFactory, toolLoaderOptions, _logger);
    }

    private static ModelContextProtocol.Server.RequestContext<ListToolsRequestParams> CreateRequest()
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.IMcpServer>();
        return new ModelContextProtocol.Server.RequestContext<ListToolsRequestParams>(mockServer)
        {
            Params = new ListToolsRequestParams()
        };
    }

    [Fact]
    public async Task ListToolsHandler_ReturnsToolsWithExpectedProperties()
    {
        var request = CreateRequest();

        var result = await _toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Verify basic structure
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // Verify that we have tools from the command factory
        Assert.True(result.Tools.Count > 0, "Expected at least one tool to be returned");

        // Get the visible commands from the command factory for comparison
        var visibleCommands = CommandHelper.GetVisibleCommands(_commandFactory.AllCommands).ToList();
        Assert.Equal(visibleCommands.Count, result.Tools.Count);

        // Verify each tool has the expected properties
        foreach (var tool in result.Tools)
        {
            Assert.NotNull(tool.Name);
            Assert.NotEmpty(tool.Name);
            Assert.NotNull(tool.Description);
            Assert.True(tool.InputSchema.ValueKind != JsonValueKind.Null, "InputSchema should not be null");

            // Verify this tool corresponds to a command from the factory
            var correspondingCommand = visibleCommands.FirstOrDefault(kvp => kvp.Key == tool.Name);
            Assert.NotNull(correspondingCommand.Value);
            Assert.Equal(correspondingCommand.Value.GetCommand().Description, tool.Description);
        }

        // Verify tool names match command names from factory
        var toolNames = result.Tools.Select(t => t.Name).OrderBy(n => n).ToList();
        var commandNames = visibleCommands.Select(kvp => kvp.Key).OrderBy(n => n).ToList();
        Assert.Equal(commandNames, toolNames);
    }

    [Fact]
    public async Task ListToolsHandler_WithReadOnlyOption_ReturnsOnlyReadOnlyTools()
    {
        var readOnlyOptions = new ToolLoaderOptions { ReadOnly = true };
        var toolLoader = new CommandFactoryToolLoader(_serviceProvider, _commandFactory, Options.Create(readOnlyOptions), _logger);
        var request = CreateRequest();

        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Verify basic structure
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // When ReadOnly is enabled, only tools with ReadOnlyHint = true should be returned
        // This may result in fewer tools or potentially no tools if none are marked as read-only
        foreach (var tool in result.Tools)
        {
            Assert.True(tool.Annotations?.ReadOnlyHint == true,
                $"Tool '{tool.Name}' should have ReadOnlyHint = true when ReadOnly mode is enabled");
        }
    }

    [Fact]
    public async Task ListToolsHandler_WithMultipleServiceFilters_ReturnsToolsFromAllSpecifiedServices()
    {
        const string Group1 = "storage";
        const string Group2 = "appconfig";

        var commandFactory = Substitute.For<ICommandFactory>();
        commandFactory.GroupCommands(Arg.Any<string[]>()).Returns(_commandFactory.MockGroupedCommands);

        var multiServiceOptions = new ToolLoaderOptions
        {
            Namespace = new[] { Group1, Group2, }
        };
        var toolLoader = new CommandFactoryToolLoader(_serviceProvider, commandFactory, Options.Create(multiServiceOptions), _logger);
        var request = CreateRequest();

        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Verify basic structure
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // Group3 has a Hidden command. We expect a command from Group1 and Group2.
        Assert.Equal(2, result.Tools.Count);

        // Make sure the group commands used the names we specified.
        // (even thought the return value is a dummy.)
        commandFactory.Received().GroupCommands(
            Arg.Is<string[]>(groups => groups.Contains(Group1) && groups.Contains(Group2)));
    }

    [Fact]
    public async Task CallToolHandler_WithValidTool_ExecutesSuccessfully()
    {
        // Get the first available command for testing
        var availableCommands = CommandHelper.GetVisibleCommands(_commandFactory.AllCommands);
        var firstCommand = availableCommands.First();

        var mockServer = Substitute.For<ModelContextProtocol.Server.IMcpServer>();
        var request = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer)
        {
            Params = new CallToolRequestParams
            {
                Name = firstCommand.Key,
                Arguments = new Dictionary<string, JsonElement>()
            }
        };

        var result = await _toolLoader.CallToolHandler(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task CallToolHandler_WithNullParams_ReturnsError()
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.IMcpServer>();
        var request = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer)
        {
            Params = null
        };

        var result = await _toolLoader.CallToolHandler(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);

        var textContent = result.Content.First() as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Contains("Cannot call tools with null parameters", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithUnknownTool_ReturnsError()
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.IMcpServer>();
        var request = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer)
        {
            Params = new CallToolRequestParams
            {
                Name = "non-existent-tool",
                Arguments = new Dictionary<string, JsonElement>()
            }
        };

        var result = await _toolLoader.CallToolHandler(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);

        var textContent = result.Content.First() as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Contains("Could not find command: non-existent-tool", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_BeforeListToolsHandler_ExecutesSuccessfully()
    {
        // Arrange
        var targetCommand = _commandFactory.Command1;
        var targetCommandKey = _commandFactory.Command1.Name;

        var mockServer = Substitute.For<ModelContextProtocol.Server.IMcpServer>();
        var arguments = new Dictionary<string, JsonElement>();

        var callToolRequest = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer)
        {
            Params = new CallToolRequestParams
            {
                Name = targetCommandKey,
                Arguments = arguments
            }
        };

        // Act - Call CallToolHandler BEFORE ListToolsHandler
        var callResult = await _toolLoader.CallToolHandler(callToolRequest, CancellationToken.None);

        // Assert based on what we know might happen
        Assert.NotNull(callResult);
        Assert.NotNull(callResult.Content);
        Assert.NotEmpty(callResult.Content);

        // If the command fails due to missing parameters, that's expected behavior we want to test
        // The key is that the tool lookup works correctly whether the command succeeds or fails
        var textContent = callResult.Content.First() as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.NotEmpty(textContent.Text);

        // The response should be valid JSON regardless of success/failure
        var jsonDoc = JsonDocument.Parse(textContent.Text);
        Assert.NotNull(jsonDoc);

        // Now call ListToolsHandler to verify it still works after CallToolHandler
        var listToolsRequest = CreateRequest();
        var listResult = await _toolLoader.ListToolsHandler(listToolsRequest, CancellationToken.None);

        // Assert that ListToolsHandler still works
        Assert.NotNull(listResult);
        Assert.NotNull(listResult.Tools);
        Assert.NotEmpty(listResult.Tools);

        // Verify the tool we called is in the list
        var calledTool = listResult.Tools.FirstOrDefault(t => t.Name == targetCommandKey);
        Assert.NotNull(calledTool);
        Assert.Equal(targetCommandKey, calledTool.Name);

        // This test passes if we can call a tool before listing tools, regardless of the tool's success/failure
        // The important thing is that the tool lookup mechanism works correctly
    }

}
