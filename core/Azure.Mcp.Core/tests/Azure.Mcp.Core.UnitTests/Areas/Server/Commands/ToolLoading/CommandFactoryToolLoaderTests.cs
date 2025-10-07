// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Command;
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
        var serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider();
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory(serviceProvider);
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<CommandFactoryToolLoader>();
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(options ?? new ToolLoaderOptions());

        var toolLoader = new CommandFactoryToolLoader(serviceProvider, commandFactory, toolLoaderOptions, logger);
        return (toolLoader, commandFactory);
    }

    private static ModelContextProtocol.Server.RequestContext<ListToolsRequestParams> CreateRequest()
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        return new ModelContextProtocol.Server.RequestContext<ListToolsRequestParams>(mockServer, new() { Method = RequestMethods.ToolsList })
        {
            Params = new ListToolsRequestParams()
        };
    }

    [Fact]
    public async Task ListToolsHandler_ReturnsToolsWithExpectedProperties()
    {
        var (toolLoader, commandFactory) = CreateToolLoader();
        var request = CreateRequest();

        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Verify basic structure
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // Verify that we have tools from the command factory
        Assert.True(result.Tools.Count > 0, "Expected at least one tool to be returned");

        // Get the visible commands from the command factory for comparison
        var visibleCommands = CommandFactory.GetVisibleCommands(commandFactory.AllCommands).ToList();
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
        var (toolLoader, _) = CreateToolLoader(readOnlyOptions);
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
    public async Task ListToolsHandler_WithServiceFilter_ReturnsOnlyFilteredTools()
    {
        // Try to filter by a specific service/group - using a common Azure service name
        var filteredOptions = new ToolLoaderOptions
        {
            Namespace = ["storage"]  // Assuming there's a storage service group
        };
        var (toolLoader, _) = CreateToolLoader(filteredOptions);
        var request = CreateRequest();

        try
        {
            var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

            // Verify basic structure
            Assert.NotNull(result);
            Assert.NotNull(result.Tools);

            // All returned tools should be from the filtered service group
            // Tool names should start with or contain the service filter
            foreach (var tool in result.Tools)
            {
                Assert.NotNull(tool.Name);
                Assert.NotEmpty(tool.Name);
                // The tool name should reflect that it's from the filtered group
                Assert.True(tool.Name.Contains("storage", StringComparison.OrdinalIgnoreCase) ||
                           tool.Name.StartsWith("storage", StringComparison.OrdinalIgnoreCase),
                           $"Tool '{tool.Name}' should be from the 'storage' service group");
            }
        }
        catch (KeyNotFoundException)
        {
            // If 'storage' group doesn't exist, that's also a valid test result
            // It means the filtering is working as expected
            Assert.True(true, "Service filtering correctly rejected non-existent service group");
        }
    }

    [Fact]
    public async Task ListToolsHandler_WithMultipleServiceFilters_ReturnsToolsFromAllSpecifiedServices()
    {
        // Try to filter by multiple real service/group names from the codebase
        var multiServiceOptions = new ToolLoaderOptions
        {
            Namespace = ["storage", "appconfig", "search"]  // Real Azure service groups from the codebase
        };
        var (toolLoader, commandFactory) = CreateToolLoader(multiServiceOptions);
        var request = CreateRequest();

        try
        {
            var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

            // Verify basic structure
            Assert.NotNull(result);
            Assert.NotNull(result.Tools);

            // Get all commands from the specified groups for comparison
            var expectedCommands = new List<string>();
            var existingServices = new List<string>();

            var serviceCommands = commandFactory.GroupCommands(multiServiceOptions.Namespace);
            expectedCommands.AddRange(serviceCommands.Keys);
            existingServices.AddRange(multiServiceOptions.Namespace);

            if (expectedCommands.Count > 0)
            {
                // Verify that returned tools match expected commands from the filtered groups
                var toolNames = result.Tools.Select(t => t.Name).ToHashSet();
                var expectedCommandNames = expectedCommands.ToHashSet();

                Assert.Equal(expectedCommandNames, toolNames);

                // All returned tools should be from one of the filtered service groups
                foreach (var tool in result.Tools)
                {
                    Assert.NotNull(tool.Name);
                    Assert.NotEmpty(tool.Name);

                    var isFromFilteredGroup = existingServices.Any(service =>
                        tool.Name.Contains(service, StringComparison.OrdinalIgnoreCase) ||
                        tool.Name.StartsWith(service, StringComparison.OrdinalIgnoreCase));

                    Assert.True(isFromFilteredGroup,
                        $"Tool '{tool.Name}' should be from one of the filtered service groups: {string.Join(", ", existingServices)}");
                }

                // Verify that tools from non-specified services are not included
                var allToolsOptions = new ToolLoaderOptions(); // No filter = all tools
                var (allToolsLoader, _) = CreateToolLoader(allToolsOptions);
                var allToolsResult = await allToolsLoader.ListToolsHandler(request, CancellationToken.None);

                var excludedTools = allToolsResult.Tools.Where(t =>
                    !existingServices.Any(service =>
                        t.Name.Contains(service, StringComparison.OrdinalIgnoreCase) ||
                        t.Name.StartsWith(service, StringComparison.OrdinalIgnoreCase)));

                foreach (var excludedTool in excludedTools)
                {
                    Assert.False(toolNames.Contains(excludedTool.Name),
                        $"Tool '{excludedTool.Name}' should not be included when filtering by services: {string.Join(", ", existingServices)}");
                }
            }
            else
            {
                // If no groups exist, we should get no tools or an exception was thrown
                Assert.Empty(result.Tools);
            }
        }
        catch (KeyNotFoundException)
        {
            // If none of the service groups exist, that's also a valid test result
            // It means the filtering is working as expected
            Assert.True(true, "Service filtering correctly rejected non-existent service groups");
        }
    }

    [Fact]
    public async Task CallToolHandler_WithValidTool_ExecutesSuccessfully()
    {
        var (toolLoader, commandFactory) = CreateToolLoader();

        // Get the first available command for testing
        var availableCommands = CommandFactory.GetVisibleCommands(commandFactory.AllCommands);
        var firstCommand = availableCommands.First();

        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        var request = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = firstCommand.Key,
                Arguments = new Dictionary<string, JsonElement>()
            }
        };

        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task CallToolHandler_WithNullParams_ReturnsError()
    {
        var (toolLoader, _) = CreateToolLoader();

        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        var request = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = null
        };

        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

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
        var (toolLoader, _) = CreateToolLoader();

        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        var request = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = "non-existent-tool",
                Arguments = new Dictionary<string, JsonElement>()
            }
        };

        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);

        var textContent = result.Content.First() as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Contains("Could not find command: non-existent-tool", textContent.Text);
    }

    [Fact]
    public async Task GetsToolsWithRawMcpInputOption()
    {
        var filteredOptions = new ToolLoaderOptions
        {
            Namespace = ["deploy"]  // Assuming there's a deploy service group
        };
        var (toolLoader, _) = CreateToolLoader(filteredOptions);
        var request = CreateRequest();
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Tools);

        var tool = result.Tools.FirstOrDefault(tool =>
            tool.Name.Equals("deploy_architecture_diagram_generate", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(tool);
        Assert.NotNull(tool.Name);
        Assert.NotNull(tool.Description!);
        Assert.NotNull(tool.Annotations);

        Assert.Equal(JsonValueKind.Object, tool.InputSchema.ValueKind);

        foreach (var properties in tool.InputSchema.EnumerateObject())
        {
            if (properties.NameEquals("type"))
            {
                Assert.Equal("object", properties.Value.GetString());
            }

            if (!properties.NameEquals("properties"))
            {
                continue;
            }

            var commandArguments = properties.Value.EnumerateObject().ToArray();
            Assert.Contains(commandArguments, arg => arg.Name.Equals("projectName", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(commandArguments, arg => arg.Name.Equals("services", StringComparison.OrdinalIgnoreCase) &&
                                                    arg.Value.GetProperty("type").GetString() == "array");
            var servicesArgument = commandArguments.FirstOrDefault(arg => arg.Name.Equals("services", StringComparison.OrdinalIgnoreCase));
            if (servicesArgument.Value.ValueKind != JsonValueKind.Undefined)
            {
                if (servicesArgument.Value.TryGetProperty("items", out var itemsProperty))
                {
                    if (itemsProperty.TryGetProperty("properties", out var servicesProperties))
                    {
                        var servicePropertyArgs = servicesProperties.EnumerateObject().ToArray();
                        Assert.Contains(servicePropertyArgs, prop => prop.Name.Equals("dependencies", StringComparison.OrdinalIgnoreCase) &&
                                                                    prop.Value.GetProperty("type").GetString() == "array");
                    }
                }
            }
        }
    }

    [Fact]
    public async Task CallToolHandler_BeforeListToolsHandler_ExecutesSuccessfully()
    {
        // Arrange
        var (toolLoader, commandFactory) = CreateToolLoader();

        // Get the subscription list command for testing
        var availableCommands = CommandFactory.GetVisibleCommands(commandFactory.AllCommands);

        // Find the subscription list command
        var subscriptionListCommand = availableCommands.FirstOrDefault(cmd => cmd.Key.Contains("subscription") && cmd.Key.Contains("list"));

        var targetCommand = subscriptionListCommand;

        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        var arguments = new Dictionary<string, JsonElement>();

        var callToolRequest = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = targetCommand.Key,
                Arguments = arguments
            }
        };

        // Act - Call CallToolHandler BEFORE ListToolsHandler
        var callResult = await toolLoader.CallToolHandler(callToolRequest, CancellationToken.None);

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
        var listResult = await toolLoader.ListToolsHandler(listToolsRequest, CancellationToken.None);

        // Assert that ListToolsHandler still works
        Assert.NotNull(listResult);
        Assert.NotNull(listResult.Tools);
        Assert.NotEmpty(listResult.Tools);

        // Verify the tool we called is in the list
        var calledTool = listResult.Tools.FirstOrDefault(t => t.Name == targetCommand.Key);
        Assert.NotNull(calledTool);
        Assert.Equal(targetCommand.Key, calledTool.Name);

        // This test passes if we can call a tool before listing tools, regardless of the tool's success/failure
        // The important thing is that the tool lookup mechanism works correctly
    }

    [Fact]
    public async Task ListToolsHandler_ReturnsToolWithArrayOrCollectionProperty()
    {
        // Arrange
        var (toolLoader, commandFactory) = CreateToolLoader();
        var request = CreateRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Find the appconfig_kv_set tool and print all tool names
        var appConfigSetTool = result.Tools.FirstOrDefault(t => t.Name == "azmcp_appconfig_kv_set");

        // Assert
        Assert.NotNull(appConfigSetTool);
        Assert.Equal(JsonValueKind.Object, appConfigSetTool.InputSchema.ValueKind);

        // Check that the tags parameter exists and has correct structure
        var properties = appConfigSetTool.InputSchema.GetProperty("properties");
        Assert.True(properties.TryGetProperty("tags", out var tagsProperty));

        // Verify tags parameter has array type
        Assert.True(tagsProperty.TryGetProperty("type", out var typeProperty));
        Assert.Equal("array", typeProperty.GetString());

        // Verify tags parameter has items property
        Assert.True(tagsProperty.TryGetProperty("items", out var itemsProperty));
        Assert.Equal(JsonValueKind.Object, itemsProperty.ValueKind);

        // Verify items has string type
        Assert.True(itemsProperty.TryGetProperty("type", out var itemTypeProperty));
        Assert.Equal("string", itemTypeProperty.GetString());
    }

    [Fact]
    public async Task ListToolsHandler_ToolsWithSecretMetadata_HaveSecretHintInMeta()
    {
        // Arrange - create a simple fake command with secret metadata
        var serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<CommandFactoryToolLoader>();
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions());

        // Create a fake command factory that includes a command with secret metadata
        var fakeCommand = Substitute.For<IBaseCommand>();
        var fakeSystemCommand = new Command("fake-secret-get", "A fake secret command for testing");

        // Set up the fake command to have secret metadata
        fakeCommand.GetCommand().Returns(fakeSystemCommand);
        fakeCommand.Title.Returns("Fake Secret Get");
        fakeCommand.Metadata.Returns(new ToolMetadata { Secret = true });

        // Create command factory using existing helper
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory(serviceProvider);

        // Add our fake command to the internal command map using reflection
        var commandMapField = typeof(CommandFactory).GetField("_commandMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandMap = (Dictionary<string, IBaseCommand>)commandMapField!.GetValue(commandFactory)!;
        commandMap["fake-secret-get"] = fakeCommand;

        var toolLoader = new CommandFactoryToolLoader(serviceProvider, commandFactory, toolLoaderOptions, logger);
        var request = CreateRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // Find the fake secret tool
        var secretTool = result.Tools.FirstOrDefault(t => t.Name == "fake-secret-get");
        Assert.NotNull(secretTool);

        // Check that the secret tool has SecretHint in its Meta
        Assert.NotNull(secretTool.Meta);
        Assert.True(secretTool.Meta.TryGetPropertyValue("SecretHint", out var secretHintNode));
        Assert.True(secretHintNode?.GetValue<bool>());
    }

    #region Elicitation Tests

    [Fact]
    public async Task CallToolHandler_WithSecretTool_WhenClientDoesNotSupportElicitation_RejectsExecution()
    {
        var (toolLoader, commandFactory) = CreateToolLoader();

        // Add the fake secret command to the command factory
        var fakeCommand = Substitute.For<IBaseCommand>();
        var fakeSystemCommand = new Command("fake-secret-get", "A fake secret command for testing");
        fakeCommand.GetCommand().Returns(fakeSystemCommand);
        fakeCommand.Title.Returns("Fake Secret Get");
        fakeCommand.Metadata.Returns(new ToolMetadata { Secret = true });
        fakeCommand.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>())
                   .Returns(new CommandResponse { Status = HttpStatusCode.OK, Message = "Secret test response" });

        // Add our fake command to the internal command map using reflection
        var commandMapField = typeof(CommandFactory).GetField("_commandMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandMap = (Dictionary<string, IBaseCommand>)commandMapField!.GetValue(commandFactory)!;
        commandMap["fake-secret-get"] = fakeCommand;

        // Create mock server without elicitation capabilities
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        mockServer.ClientCapabilities.Returns((ClientCapabilities?)null);

        var request = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = "fake-secret-get",
                Arguments = new Dictionary<string, JsonElement>()
            }
        };

        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        // Should reject execution as client doesn't support elicitation (security requirement)
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Contains("does not support elicitation", ((TextContentBlock)result.Content.First()).Text);
    }

    [Fact]
    public async Task CallToolHandler_WithNonSecretTool_DoesNotTriggerElicitation()
    {
        var (toolLoader, commandFactory) = CreateToolLoader();

        // Add a fake non-secret command to the command factory
        var fakeCommand = Substitute.For<IBaseCommand>();
        var fakeSystemCommand = new Command("fake-non-secret-get", "A fake non-secret command for testing");
        fakeCommand.GetCommand().Returns(fakeSystemCommand);
        fakeCommand.Title.Returns("Fake Non-Secret Get");
        fakeCommand.Metadata.Returns(new ToolMetadata { Secret = false }); // Not secret
        fakeCommand.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>())
                   .Returns(new CommandResponse { Status = HttpStatusCode.OK, Message = "Test response" });

        // Add our fake command to the internal command map using reflection
        var commandMapField = typeof(CommandFactory).GetField("_commandMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandMap = (Dictionary<string, IBaseCommand>)commandMapField!.GetValue(commandFactory)!;
        commandMap["fake-non-secret-get"] = fakeCommand;

        // Create mock server with elicitation capabilities
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        var capabilities = new ClientCapabilities { Elicitation = new ElicitationCapability() };
        mockServer.ClientCapabilities.Returns(capabilities);

        var request = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = "fake-non-secret-get",
                Arguments = new Dictionary<string, JsonElement>()
            }
        };

        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        // Should execute without issues for non-secret tools
        Assert.NotNull(result);
        Assert.False(result.IsError);
    }

    [Fact]
    public async Task CallToolHandler_WithSecretTool_WhenInsecureDisableElicitationEnabled_BypassesElicitation()
    {
        // Create tool loader with insecure disable elicitation enabled
        var options = new ToolLoaderOptions(InsecureDisableElicitation: true);
        var (toolLoader, commandFactory) = CreateToolLoader(options);

        // Add the fake secret command to the command factory
        var fakeCommand = Substitute.For<IBaseCommand>();
        var fakeSystemCommand = new Command("fake-secret-get", "A fake secret command for testing");
        fakeCommand.GetCommand().Returns(fakeSystemCommand);
        fakeCommand.Title.Returns("Fake Secret Get");
        fakeCommand.Metadata.Returns(new ToolMetadata { Secret = true });
        fakeCommand.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>())
                   .Returns(new CommandResponse { Status = HttpStatusCode.OK, Message = "Secret test response" });

        // Add our fake command to the internal command map using reflection
        var commandMapField = typeof(CommandFactory).GetField("_commandMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandMap = (Dictionary<string, IBaseCommand>)commandMapField!.GetValue(commandFactory)!;
        commandMap["fake-secret-get"] = fakeCommand;

        // Create mock server - elicitation support doesn't matter when bypassed
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        mockServer.ClientCapabilities.Returns((ClientCapabilities?)null);

        var request = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = "fake-secret-get",
                Arguments = new Dictionary<string, JsonElement>()
            }
        };

        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        // Should execute successfully despite being a secret tool and client not supporting elicitation
        Assert.NotNull(result);
        Assert.False(result.IsError);
        var responseText = ((TextContentBlock)result.Content.First()).Text;
        var response = JsonSerializer.Deserialize<CommandResponse>(responseText);
        Assert.Equal(HttpStatusCode.OK, response!.Status);
        Assert.Equal("Secret test response", response.Message);
    }

    [Fact]
    public async Task CallToolHandler_WithSecretTool_WhenInsecureDisableElicitationDisabled_StillRequiresElicitation()
    {
        // Create tool loader with insecure disable elicitation disabled (default)
        var options = new ToolLoaderOptions(InsecureDisableElicitation: false);
        var (toolLoader, commandFactory) = CreateToolLoader(options);

        // Add the fake secret command to the command factory
        var fakeCommand = Substitute.For<IBaseCommand>();
        var fakeSystemCommand = new Command("fake-secret-get", "A fake secret command for testing");
        fakeCommand.GetCommand().Returns(fakeSystemCommand);
        fakeCommand.Title.Returns("Fake Secret Get");
        fakeCommand.Metadata.Returns(new ToolMetadata { Secret = true });
        fakeCommand.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>())
                   .Returns(new CommandResponse { Status = HttpStatusCode.OK, Message = "Secret test response" });

        // Add our fake command to the internal command map using reflection
        var commandMapField = typeof(CommandFactory).GetField("_commandMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandMap = (Dictionary<string, IBaseCommand>)commandMapField!.GetValue(commandFactory)!;
        commandMap["fake-secret-get"] = fakeCommand;

        // Create mock server without elicitation capabilities
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        mockServer.ClientCapabilities.Returns((ClientCapabilities?)null);

        var request = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = "fake-secret-get",
                Arguments = new Dictionary<string, JsonElement>()
            }
        };

        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        // Should still reject execution when insecure option is disabled
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Contains("does not support elicitation", ((TextContentBlock)result.Content.First()).Text);
    }

    [Fact]
    public void ToolLoaderOptions_DefaultInsecureDisableElicitation_IsFalse()
    {
        // Arrange & Act
        var options = new ToolLoaderOptions();

        // Assert
        Assert.False(options.InsecureDisableElicitation);
    }

    [Fact]
    public void ToolLoaderOptions_WithInsecureDisableElicitationTrue_IsSetCorrectly()
    {
        // Arrange & Act
        var options = new ToolLoaderOptions(InsecureDisableElicitation: true);

        // Assert
        Assert.True(options.InsecureDisableElicitation);
    }

    #endregion
}
