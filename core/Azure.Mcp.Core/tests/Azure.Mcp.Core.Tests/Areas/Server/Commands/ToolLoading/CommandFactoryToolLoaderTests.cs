// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.Runtime;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

public class CommandFactoryToolLoaderTests
{
    private static (CommandFactoryToolLoader toolLoader, ICommandFactory commandFactory) CreateToolLoader(ServerRuntimeConfiguration? configuration = null)
    {
        var serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider();
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory(serviceProvider);
        var runtimeConfiguration = Microsoft.Extensions.Options.Options.Create(configuration ?? new ServerRuntimeConfiguration());

        var toolLoader = new CommandFactoryToolLoader(commandFactory, runtimeConfiguration, Substitute.For<ILogger<CommandFactoryToolLoader>>());
        return (toolLoader, commandFactory);
    }

    private static IMcpRuntime CreateRuntime(IToolLoader toolLoader, Activity activity)
    {
        var telemetry = Substitute.For<ITelemetryService>();
        telemetry.StartActivity(Arg.Any<string>(), Arg.Any<Implementation?>(), Arg.Any<RequestParams?>()).Returns(activity);

        var runtime = new McpRuntime(toolLoader, telemetry);

        return runtime;
    }

    private static IBaseCommand CreateFakeCommand(string toolName, ToolMetadata metadata)
    {
        var toolId = Guid.NewGuid().ToString();

        var fakeCommand = Substitute.For<IBaseCommand>();
        var fakeSystemCommand = new Command(toolName, "Fake tool for testing");
        fakeCommand.GetCommand().Returns(fakeSystemCommand);
        fakeCommand.Title.Returns("Fake tool for testing");
        fakeCommand.Id.Returns(toolId);
        fakeCommand.Metadata.Returns(metadata);

        return fakeCommand;
    }

    private static void InjectCommandFactoryTool(ICommandFactory commandFactory, IBaseCommand fakeCommand)
    {
        var commandMapField = typeof(CommandFactory).GetField("_commandMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandMap = (Dictionary<string, IBaseCommand>)commandMapField!.GetValue(commandFactory)!;
        commandMap[fakeCommand.GetCommand().Name] = fakeCommand;
    }

    [Fact]
    public async Task ListToolsHandler_ReturnsToolsWithExpectedProperties()
    {
        var (toolLoader, commandFactory) = CreateToolLoader();
        var request = McpTestUtilities.CreateToolListRequest();

        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

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
        var configuration = new ServerRuntimeConfiguration { ReadOnly = true };
        var (toolLoader, _) = CreateToolLoader(configuration);
        var request = McpTestUtilities.CreateToolListRequest();

        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

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
    public async Task ListToolsHandler_WithIsHttpOption_DoesNotReturnLocalRequiredTools()
    {
        var configuration = new ServerRuntimeConfiguration { Transport = TransportTypes.Http };
        var (toolLoader, _) = CreateToolLoader(configuration);
        var request = McpTestUtilities.CreateToolListRequest();

        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Verify basic structure
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // When HTTP mode is enabled, only tools with LocalRequiredHint = false should be returned
        // This may result in fewer tools or potentially no tools if all are marked as local required
        foreach (var tool in result.Tools)
        {
            Assert.False(McpHelper.HasHint(tool, McpHelper.LocalRequiredHintMetaKey),
                $"Tool '{tool.Name}' should have LocalRequiredHint = false when HTTP mode is enabled");
        }
    }

    [Fact]
    public async Task ListToolsHandler_WithToolFilter_ReturnsOnlySpecifiedTool()
    {
        // Arrange
        var (_, commandFactory) = CreateToolLoader();
        var availableCommands = CommandFactory.GetVisibleCommands(commandFactory.AllCommands).ToList();

        // Skip test if no commands are available
        if (!availableCommands.Any())
        {
            return;
        }

        var specificToolName = availableCommands.First().Key;
        var configuration = new ServerRuntimeConfiguration { Tool = [specificToolName] };
        var (toolLoader, _) = CreateToolLoader(configuration);
        var request = McpTestUtilities.CreateToolListRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);
        Assert.Single(result.Tools);
        Assert.Equal(specificToolName, result.Tools[0].Name);
    }

    [Fact]
    public async Task ListToolsHandler_WithNonExistentToolFilter_ReturnsEmptyList()
    {
        // Arrange
        var nonExistentTool = "non-existent-tool-name";
        var configuration = new ServerRuntimeConfiguration { Tool = [nonExistentTool] };
        var (toolLoader, _) = CreateToolLoader(configuration);
        var request = McpTestUtilities.CreateToolListRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);
        Assert.Empty(result.Tools);
    }

    [Fact]
    public async Task ListToolsHandler_WithToolFilterCaseInsensitive_ReturnsSpecifiedTool()
    {
        // Arrange
        var (_, commandFactory) = CreateToolLoader();
        var availableCommands = CommandFactory.GetVisibleCommands(commandFactory.AllCommands).ToList();

        // Skip test if no commands are available
        if (!availableCommands.Any())
        {
            return;
        }

        var specificToolName = availableCommands.First().Key;
        var configuration = new ServerRuntimeConfiguration { Tool = [specificToolName.ToUpperInvariant()] }; // Test case insensitive
        var (toolLoader, _) = CreateToolLoader(configuration);
        var request = McpTestUtilities.CreateToolListRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);
        Assert.Single(result.Tools);
        Assert.Equal(specificToolName, result.Tools[0].Name);
    }

    [Fact]
    public async Task ListToolsHandler_WithServiceFilter_ReturnsOnlyFilteredTools()
    {
        // Try to filter by a specific service/group - using a common Azure service name
        var configuration = new ServerRuntimeConfiguration
        {
            Namespace = ["storage"]  // Assuming there's a storage service group
        };
        var (toolLoader, _) = CreateToolLoader(configuration);
        var request = McpTestUtilities.CreateToolListRequest();

        try
        {
            var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

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
        var configuration = new ServerRuntimeConfiguration
        {
            Namespace = ["storage", "appconfig", "search"]  // Real Azure service groups from the codebase
        };
        var (toolLoader, commandFactory) = CreateToolLoader(configuration);
        var request = McpTestUtilities.CreateToolListRequest();

        try
        {
            var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

            // Verify basic structure
            Assert.NotNull(result);
            Assert.NotNull(result.Tools);

            // Get all commands from the specified groups for comparison
            var expectedCommands = new List<string>();
            var existingServices = new List<string>();

            var serviceCommands = commandFactory.GroupCommands(configuration.Namespace);
            expectedCommands.AddRange(serviceCommands.Keys);
            existingServices.AddRange(configuration.Namespace);

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
                var (allToolsLoader, _) = CreateToolLoader(new ServerRuntimeConfiguration()); // No filter = all tools
                var allToolsResult = await allToolsLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

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

        var request = McpTestUtilities.CreateToolCallRequest(firstCommand.Key);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);

        // Validate telemetry
        Assert.Equal(result.IsError == true ? ActivityStatusCode.Error : ActivityStatusCode.Ok, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, true);
        activity.AssertTagEquals(TagName.ToolName, firstCommand.Key);
        activity.AssertTagEquals(TagName.ToolId, firstCommand.Value.Id);
        activity.AssertTagEquals(TagName.ToolArea, commandFactory.GetServiceArea(firstCommand.Key)!);
        activity.AssertTagEquals(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(firstCommand.Value));
        activity.AssertTagDoesNotExist(TagName.ToolParameters);
        activity.AssertTagEquals(TagName.ToolSource, "internal");
    }

    [Fact]
    public async Task CallToolHandler_WithNullParams_ReturnsError()
    {
        var (toolLoader, _) = CreateToolLoader();

        var request = McpTestUtilities.CreateToolCallRequest((CallToolRequestParams)null!, Substitute.For<McpServer>());

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);

        var textContent = result.Content.First() as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Contains("Cannot call tools with null parameters", textContent.Text);

        // Validate telemetry
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        activity.AssertTagEquals(TagName.ExceptionType, "InvalidParameters");
        activity.AssertTagDoesNotExist(TagName.IsServerCommandInvoked);
        activity.AssertTagDoesNotExist(TagName.ToolArea);
        activity.AssertTagDoesNotExist(TagName.ToolId);
        activity.AssertTagDoesNotExist(TagName.ToolParameters);
    }

    [Fact]
    public async Task CallToolHandler_WithUnknownTool_ReturnsError()
    {
        var toolName = "non-existent-tool";
        var (toolLoader, _) = CreateToolLoader();

        var request = McpTestUtilities.CreateToolCallRequest(toolName);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);

        var textContent = result.Content.First() as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Contains("Could not find command: non-existent-tool", textContent.Text);

        // Validate telemetry
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, false);
        activity.AssertTagEquals(TagName.ToolName, toolName);
        activity.AssertTagDoesNotExist(TagName.ToolArea);
        activity.AssertTagDoesNotExist(TagName.ToolId);
    }

    [Fact]
    public async Task GetsToolsWithRawMcpInputOption()
    {
        var configuration = new ServerRuntimeConfiguration
        {
            Namespace = ["deploy"]  // Assuming there's a deploy service group
        };
        var (toolLoader, _) = CreateToolLoader(configuration);
        var request = McpTestUtilities.CreateToolListRequest();
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

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
        var targetCommand = availableCommands.FirstOrDefault(cmd => cmd.Key.Contains("subscription") && cmd.Key.Contains("list"));

        var callToolRequest = McpTestUtilities.CreateToolCallRequest(targetCommand.Key);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        // Act - Call CallToolHandler BEFORE ListToolsHandler
        var callResult = await mcpRuntime.CallToolHandler(callToolRequest, TestContext.Current.CancellationToken);

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

        // Validate tool call telemetry
        Assert.Equal(callResult.IsError == true ? ActivityStatusCode.Error : ActivityStatusCode.Ok, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, true);
        activity.AssertTagEquals(TagName.ToolName, targetCommand.Key);
        activity.AssertTagEquals(TagName.ToolId, targetCommand.Value.Id);
        activity.AssertTagEquals(TagName.ToolArea, commandFactory.GetServiceArea(targetCommand.Key)!);
        activity.AssertTagEquals(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(targetCommand.Value));
        activity.AssertTagDoesNotExist(TagName.ToolParameters);
        activity.AssertTagEquals(TagName.ToolSource, "internal");

        // Now call ListToolsHandler to verify it still works after CallToolHandler
        var listToolsRequest = McpTestUtilities.CreateToolListRequest();
        var listResult = await toolLoader.ListToolsHandler(listToolsRequest, TestContext.Current.CancellationToken);

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
        var request = McpTestUtilities.CreateToolListRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Find the appconfig_kv_set tool and print all tool names
        var appConfigSetTool = result.Tools.FirstOrDefault(t => t.Name == "appconfig_kv_set");

        // Assert
        Assert.NotNull(appConfigSetTool);
        Assert.Equal(JsonValueKind.Object, appConfigSetTool.InputSchema.ValueKind);

        // Check that the tags parameter exists and has correct structure
        var properties = appConfigSetTool.InputSchema.GetProperty("properties");
        var tagsProperty = properties.AssertProperty("tags");

        // Verify tags parameter has array type
        var typeProperty = tagsProperty.AssertProperty("type");
        Assert.Equal("array", typeProperty.GetString());

        // Verify tags parameter has items property
        var itemsProperty = tagsProperty.AssertProperty("items");
        Assert.Equal(JsonValueKind.Object, itemsProperty.ValueKind);

        // Verify items has string type
        var itemTypeProperty = itemsProperty.AssertProperty("type");
        Assert.Equal("string", itemTypeProperty.GetString());
    }

    [Fact]
    public async Task ListToolsHandler_EveryTool_ProducesValidInputSchema()
    {
        // Arrange
        var (toolLoader, commandFactory) = CreateToolLoader();
        var request = McpTestUtilities.CreateToolListRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(result.Tools);

        var visibleCommands = CommandFactory.GetVisibleCommands(commandFactory.AllCommands)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        foreach (var tool in result.Tools)
        {
            var schema = tool.InputSchema;
            Assert.Equal(JsonValueKind.Object, schema.ValueKind);

            // Raw MCP passthrough commands supply a hand-authored schema verbatim, so they
            // are not expected to follow the generated strict-object shape. Skip them.
            if (UsesRawMcpToolInput(visibleCommands[tool.Name]))
            {
                continue;
            }

            var typeProperty = schema.AssertProperty("type");
            Assert.Equal("object", typeProperty.GetString());

            var propertiesProperty = schema.AssertProperty("properties");
            Assert.Equal(JsonValueKind.Object, propertiesProperty.ValueKind);

            // OpenAI strict-mode compatibility: additionalProperties must be false.
            var additionalProperties = schema.GetProperty("additionalProperties");
            Assert.Equal(JsonValueKind.False, additionalProperties.ValueKind);

            // Every 'required' entry must reference a declared property.
            if (schema.TryGetProperty("required", out var requiredProperty))
            {
                Assert.Equal(JsonValueKind.Array, requiredProperty.ValueKind);

                foreach (var required in requiredProperty.EnumerateArray())
                {
                    var name = required.GetString();
                    Assert.False(string.IsNullOrEmpty(name),
                        $"'{tool.Name}' has an empty entry in 'required'.");
                    Assert.True(propertiesProperty.TryGetProperty(name, out _),
                        $"'{tool.Name}' requires '{name}' which is not a declared property.");
                }
            }
        }
    }

    [Fact]
    public async Task ListToolsHandler_EnumOption_IsExportedAsStringType()
    {
        // Arrange
        // Build a fake command that declares a single enum-backed option using the same production
        // machinery real commands use (OptionBinder.RegisterOptions -> OptionDescriptor + OptionTypeHandler).
        // This exercises how an enum flows through OptionSchemaGenerator without coupling the test to a
        // shipping tool whose options could change over time.
        var toolName = "fake-enum-get";
        var serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<CommandFactoryToolLoader>();
        var configuration = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration());

        var fakeCommand = CreateFakeCommand(toolName, new());
        OptionBinder.RegisterOptions<EnumSchemaTestOptions>(fakeCommand.GetCommand());

        var commandFactory = CommandFactoryHelpers.CreateCommandFactory(serviceProvider);
        InjectCommandFactoryTool(commandFactory, fakeCommand);

        var toolLoader = new CommandFactoryToolLoader(commandFactory, configuration, logger);
        var request = McpTestUtilities.CreateToolListRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        // Enum options are modeled as Option<string> by OptionTypeHandler (a JsonStringEnumConverter is
        // registered so values round-trip as member names, never integers). The schema generator only sees
        // Option.ValueType (string), so an enum surfaces as JSON "type": "string" with its allowed values
        // described in prose - it does not emit a JSON "enum" keyword. This spot-check locks that contract:
        // an enum-backed option must be exported as a string type, guarding against a regression to integer
        // (ordinal) serialization.
        var tool = result.Tools.FirstOrDefault(t => t.Name == "fake-enum-get");
        Assert.NotNull(tool);

        var schema = tool.InputSchema;
        var properties = schema.AssertProperty("properties");

        var sampleLevel = properties.AssertProperty("sample-level");
        Assert.Equal(JsonValueKind.Object, sampleLevel.ValueKind);

        var typeProperty = sampleLevel.AssertProperty("type");

        // The enum must map to the JSON string type and never to a numeric (ordinal) type. Tolerate a
        // scalar ("string") or a union array (e.g. ["string", "null"]) representation of nullability.
        static bool IsStringType(JsonElement type) =>
            type.ValueKind == JsonValueKind.String && type.GetString() == "string";
        static bool IsNullType(JsonElement type) =>
            type.ValueKind == JsonValueKind.String && type.GetString() == "null";

        if (typeProperty.ValueKind == JsonValueKind.Array)
        {
            var entries = typeProperty.EnumerateArray().ToArray();

            // Assert.All invokes the predicate on every element, so a stray numeric (or any other
            // unexpected) entry fails the test. Whitelisting "string"/"null" is stricter than
            // blacklisting numeric, since it also rejects anything else the union should not contain.
            Assert.All(entries, entry => Assert.True(IsStringType(entry) || IsNullType(entry),
                $"'sample-level' type union should contain only 'string'/'null' but had '{entry}'."));

            // The union must also actually include the string type (Assert.Contains is an existence check).
            Assert.Contains(entries, IsStringType);
        }
        else
        {
            Assert.True(IsStringType(typeProperty),
                $"'sample-level' enum option should be exported as a string type but was '{typeProperty}'.");
        }
    }

    [Fact]
    public async Task ListToolsHandler_CommandWithResultTypeInfo_EmitsObjectOutputSchema()
    {
        // Arrange
        // A fake command that advertises a source-generated result type. GetTool should surface that type as
        // the tool's outputSchema.
        var serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<CommandFactoryToolLoader>();
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            StructuredOutputMode = StructuredOutputMode.Compact
        });

        var fakeCommand = Substitute.For<IBaseCommand>();
        fakeCommand.GetCommand().Returns(new Command("fake-output-get", "A fake command that advertises a result type."));
        fakeCommand.Title.Returns("Fake Output Get");
        fakeCommand.Metadata.Returns(new ToolMetadata());
        fakeCommand.ResultTypeInfo.Returns(OutputSchemaTestJsonContext.Default.OutputSchemaSampleResult);

        var commandFactory = CommandFactoryHelpers.CreateCommandFactory(serviceProvider);
        var commandMapField = typeof(CommandFactory).GetField("_commandMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandMap = (Dictionary<string, IBaseCommand>)commandMapField!.GetValue(commandFactory)!;
        commandMap["fake-output-get"] = fakeCommand;

        var toolLoader = new CommandFactoryToolLoader(commandFactory, toolLoaderOptions, logger);
        var request = McpTestUtilities.CreateToolListRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        // A migrated command (where ResultTypeInfo != null) must surface an MCP outputSchema whose root is an
        // object exposing the result record's own properties.
        var tool = result.Tools.FirstOrDefault(t => t.Name == "fake-output-get");
        Assert.NotNull(tool);
        Assert.NotNull(tool.OutputSchema);

        var outputSchema = tool.OutputSchema!.Value;
        Assert.Equal(JsonValueKind.Object, outputSchema.ValueKind);

        Assert.True(outputSchema.TryGetProperty("type", out var typeProperty), "outputSchema is missing 'type'.");
        Assert.Equal("object", typeProperty.GetString());

        Assert.True(outputSchema.TryGetProperty("properties", out var properties), "outputSchema is missing 'properties'.");
        Assert.True(properties.TryGetProperty("name", out _), "outputSchema should expose the result record's 'name' property.");

        var inputProperties = tool.InputSchema.GetProperty("properties");
        Assert.Empty(inputProperties.EnumerateObject());
    }

    [Fact]
    public async Task ListToolsHandler_CommandWithoutResultTypeInfo_OmitsOutputSchema()
    {
        // Arrange
        // A fake command that does not advertise a result type (ResultTypeInfo == null, the default value).
        // GetTool should leave the tool's outputSchema unset so the command gracefully advertises
        // no structured output.
        var serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<CommandFactoryToolLoader>();
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration());

        var fakeCommand = Substitute.For<IBaseCommand>();
        fakeCommand.GetCommand().Returns(new Command("fake-noschema-get", "A fake command with no result type."));
        fakeCommand.Title.Returns("Fake No Schema Get");
        fakeCommand.Metadata.Returns(new ToolMetadata());

        var commandFactory = CommandFactoryHelpers.CreateCommandFactory(serviceProvider);
        var commandMapField = typeof(CommandFactory).GetField("_commandMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandMap = (Dictionary<string, IBaseCommand>)commandMapField!.GetValue(commandFactory)!;
        commandMap["fake-noschema-get"] = fakeCommand;

        var toolLoader = new CommandFactoryToolLoader(commandFactory, toolLoaderOptions, logger);
        var request = McpTestUtilities.CreateToolListRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        var tool = result.Tools.FirstOrDefault(t => t.Name == "fake-noschema-get");
        Assert.NotNull(tool);
        Assert.Null(tool.OutputSchema);
    }

    [Theory]
    [InlineData(StructuredOutputMode.Duplicated)]
    [InlineData(StructuredOutputMode.Compact)]
    public async Task ListToolsHandler_StructuredOutputMode_EmitsSchemaWithoutChangingInputSchema(
        StructuredOutputMode mode)
    {
        var serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<CommandFactoryToolLoader>();
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            StructuredOutputMode = mode
        });
        var fakeCommand = Substitute.For<IBaseCommand>();
        fakeCommand.GetCommand().Returns(new Command("fake-output-get"));
        fakeCommand.Title.Returns("Fake Output Get");
        fakeCommand.Metadata.Returns(new ToolMetadata());
        fakeCommand.ResultTypeInfo.Returns(OutputSchemaTestJsonContext.Default.OutputSchemaSampleResult);

        var commandFactory = CommandFactoryHelpers.CreateCommandFactory(serviceProvider);
        var commandMapField = typeof(CommandFactory).GetField(
            "_commandMap",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandMap = (Dictionary<string, IBaseCommand>)commandMapField!.GetValue(commandFactory)!;
        commandMap["fake-output-get"] = fakeCommand;

        var toolLoader = new CommandFactoryToolLoader(
            commandFactory,
            toolLoaderOptions,
            logger);

        var result = await toolLoader.ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(),
            TestContext.Current.CancellationToken);

        var tool = Assert.Single(result.Tools, tool => tool.Name == "fake-output-get");
        Assert.True(tool.OutputSchema.HasValue);
        Assert.Empty(tool.InputSchema.GetProperty("properties").EnumerateObject());
    }

    [Fact]
    public async Task ListToolsHandler_DisabledStructuredOutput_OmitsOutputSchema()
    {
        var serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<CommandFactoryToolLoader>();
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration());

        var fakeCommand = Substitute.For<IBaseCommand>();
        fakeCommand.GetCommand().Returns(new Command("fake-output-get", "A fake command that advertises a result type."));
        fakeCommand.Title.Returns("Fake Output Get");
        fakeCommand.Metadata.Returns(new ToolMetadata());
        fakeCommand.ResultTypeInfo.Returns(OutputSchemaTestJsonContext.Default.OutputSchemaSampleResult);

        var commandFactory = CommandFactoryHelpers.CreateCommandFactory(serviceProvider);
        var commandMapField = typeof(CommandFactory).GetField("_commandMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandMap = (Dictionary<string, IBaseCommand>)commandMapField!.GetValue(commandFactory)!;
        commandMap["fake-output-get"] = fakeCommand;

        var toolLoader = new CommandFactoryToolLoader(commandFactory, toolLoaderOptions, logger);
        var request = McpTestUtilities.CreateToolListRequest();

        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        var tool = result.Tools.FirstOrDefault(t => t.Name == "fake-output-get");
        Assert.NotNull(tool);
        Assert.Null(tool.OutputSchema);
    }

    [Fact]
    public void TryBuildStructuredContent_ObjectResult_IsUnwrapped()
    {
        // An object-root command result already satisfies MCP's "structuredContent must be an object" rule,
        // so it included as-is: its own properties are surfaced directly rather than nested under a
        // wrapper. This stays aligned with CreateOutputSchema, which leaves object roots unwrapped.
        var result = ResponseResult.Create(
            new OutputSchemaSampleResult("alpha", 3),
            OutputSchemaTestJsonContext.Default.OutputSchemaSampleResult);

        var structuredContent = StructuredOutputHelper.TryBuildStructuredContent(result);

        Assert.NotNull(structuredContent);
        var value = structuredContent!.Value;
        Assert.Equal(JsonValueKind.Object, value.ValueKind);
        Assert.Equal("alpha", value.GetProperty("name").GetString());
        Assert.Equal(3, value.GetProperty("count").GetInt32());
        Assert.False(value.TryGetProperty("value", out _), "Object results must not be wrapped under a 'value' property.");
    }

    [Fact]
    public void TryBuildStructuredContent_ArrayResult_IsWrappedUnderValue()
    {
        // A non-object payload (array) cannot be the structuredContent root, so it is wrapped under a single
        // 'value' property, matching the wrapping CreateOutputSchema applies to the advertised schema so the
        // payload validates against it.
        var result = ResponseResult.Create(
            new[] { "one", "two" },
            OutputSchemaTestJsonContext.Default.StringArray);

        var structuredContent = StructuredOutputHelper.TryBuildStructuredContent(result);

        Assert.NotNull(structuredContent);
        var value = structuredContent!.Value;
        Assert.Equal(JsonValueKind.Object, value.ValueKind);
        Assert.True(value.TryGetProperty("value", out var wrapped), "Array results must be wrapped under a 'value' property.");
        Assert.Equal(JsonValueKind.Array, wrapped.ValueKind);
        Assert.Equal(2, wrapped.GetArrayLength());
    }

    [Fact]
    public void TryBuildStructuredContent_ScalarResult_IsWrappedUnderValue()
    {
        // A scalar payload likewise cannot be the root object, so it is wrapped under 'value' to match the
        // advertised schema.
        var result = ResponseResult.Create(
            42,
            OutputSchemaTestJsonContext.Default.Int32);

        var structuredContent = StructuredOutputHelper.TryBuildStructuredContent(result);

        Assert.NotNull(structuredContent);
        var value = structuredContent!.Value;
        Assert.Equal(JsonValueKind.Object, value.ValueKind);
        Assert.True(value.TryGetProperty("value", out var wrapped), "Scalar results must be wrapped under a 'value' property.");
        Assert.Equal(42, wrapped.GetInt32());
    }

    [Fact]
    public void TryBuildStructuredContent_NoResult_ReturnsNull()
    {
        var structuredContent = StructuredOutputHelper.TryBuildStructuredContent(null);

        Assert.Null(structuredContent);
    }

    [Fact]
    public void TryBuildStructuredContent_NullResult_ReturnsNull()
    {
        var result = ResponseResult.Create<string?>(null, OutputSchemaTestJsonContext.Default.String);

        var structuredContent = StructuredOutputHelper.TryBuildStructuredContent(result);

        Assert.Null(structuredContent);
    }

    [Fact]
    public async Task CallToolHandler_CompactMode_EmitsStructuredContent()
    {
        // Arrange
        // A non-secret command that advertises a result type and returns a payload should receive the
        // payload in structuredContent and compact text when the operator opts into compact mode.
        var (toolLoader, commandFactory) = CreateToolLoader(new ServerRuntimeConfiguration
        {
            StructuredOutputMode = StructuredOutputMode.Compact
        });

        var fakeCommand = Substitute.For<IBaseCommand>();
        fakeCommand.GetCommand().Returns(new Command("fake-structured-get", "A fake command that returns a structured payload."));
        fakeCommand.Title.Returns("Fake Structured Get");
        fakeCommand.Metadata.Returns(new ToolMetadata { Destructive = false });
        fakeCommand.ResultTypeInfo.Returns(OutputSchemaTestJsonContext.Default.OutputSchemaSampleResult);
        fakeCommand.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
                   .Returns(new CommandResponse
                   {
                       Status = HttpStatusCode.OK,
                       Results = ResponseResult.Create(new OutputSchemaSampleResult("alpha", 3), OutputSchemaTestJsonContext.Default.OutputSchemaSampleResult)
                   });

        var commandMapField = typeof(CommandFactory).GetField("_commandMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandMap = (Dictionary<string, IBaseCommand>)commandMapField!.GetValue(commandFactory)!;
        commandMap["fake-structured-get"] = fakeCommand;

        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        var request = McpTestUtilities.CreateToolCallRequest("fake-structured-get", mockServer);

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(StructuredOutputHelper.CompactContentMessage, GetTextContent(result));
        Assert.NotNull(result.StructuredContent);
        var structured = result.StructuredContent!.Value;
        Assert.Equal("alpha", structured.GetProperty("name").GetString());
        Assert.Equal(3, structured.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task CallToolHandler_DisabledStructuredOutput_OmitsStructuredContent()
    {
        var response = CreateSuccessfulStructuredResponse();
        var (toolLoader, _) = CreateStructuredToolLoader(response, mode: null);

        var result = await toolLoader.CallToolHandler(
            CreateCallRequest(),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Null(result.StructuredContent);
        Assert.Equal(JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse), GetTextContent(result));
    }

    [Fact]
    public async Task CallToolHandler_DuplicatedMode_UsesFullContentAndStructuredContent()
    {
        var response = CreateSuccessfulStructuredResponse();
        var (toolLoader, _) = CreateStructuredToolLoader(response, mode: StructuredOutputMode.Duplicated);

        var result = await toolLoader.CallToolHandler(
            CreateCallRequest(),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse),
            GetTextContent(result));
        Assert.NotNull(result.StructuredContent);
    }

    [Fact]
    public async Task CallToolHandler_CommandError_UsesFullContentWithoutStructuredContent()
    {
        var response = new CommandResponse
        {
            Status = HttpStatusCode.InternalServerError,
            Message = "Command failed."
        };
        var (toolLoader, _) = CreateStructuredToolLoader(response);

        var result = await toolLoader.CallToolHandler(
            CreateCallRequest(),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        Assert.Equal(JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse), GetTextContent(result));
    }

    [Fact]
    public async Task CallToolHandler_SuccessWithoutResults_UsesFullContentWithoutStructuredContent()
    {
        var response = new CommandResponse
        {
            Status = HttpStatusCode.OK,
            Message = "Success"
        };
        var (toolLoader, _) = CreateStructuredToolLoader(response);

        var result = await toolLoader.CallToolHandler(
            CreateCallRequest(),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Null(result.StructuredContent);
        Assert.Equal(JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse), GetTextContent(result));
    }

    private static string GetTextContent(CallToolResult result)
    {
        var content = Assert.Single(result.Content);
        return Assert.IsType<TextContentBlock>(content).Text;
    }

    private static CommandResponse CreateSuccessfulStructuredResponse()
    {
        return new CommandResponse
        {
            Status = HttpStatusCode.OK,
            Results = ResponseResult.Create(
                new OutputSchemaSampleResult("alpha", 3),
                OutputSchemaTestJsonContext.Default.OutputSchemaSampleResult)
        };
    }

    private static (CommandFactoryToolLoader ToolLoader, IBaseCommand Command) CreateStructuredToolLoader(
        CommandResponse response,
        Command? systemCommand = null,
        string commandName = "fake-structured-get",
        Action<ParseResult>? onExecute = null,
        StructuredOutputMode? mode = StructuredOutputMode.Compact)
    {
        var (toolLoader, commandFactory) = CreateToolLoader(new ServerRuntimeConfiguration
        {
            StructuredOutputMode = mode
        });
        var fakeCommand = Substitute.For<IBaseCommand>();
        fakeCommand.GetCommand().Returns(systemCommand ?? new Command(commandName));
        fakeCommand.Title.Returns("Fake Structured Get");
        fakeCommand.Metadata.Returns(new ToolMetadata { Destructive = false });
        fakeCommand.ResultTypeInfo.Returns(OutputSchemaTestJsonContext.Default.OutputSchemaSampleResult);
        fakeCommand.ExecuteAsync(
                Arg.Any<CommandContext>(),
                Arg.Any<ParseResult>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                onExecute?.Invoke(callInfo.ArgAt<ParseResult>(1));
                return Task.FromResult(response);
            });

        var commandMapField = typeof(CommandFactory).GetField(
            "_commandMap",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var commandMap = (Dictionary<string, IBaseCommand>)commandMapField!.GetValue(commandFactory)!;
        commandMap[commandName] = fakeCommand;

        return (toolLoader, fakeCommand);
    }

    private static ModelContextProtocol.Server.RequestContext<CallToolRequestParams> CreateCallRequest(
        Dictionary<string, JsonElement>? arguments = null,
        string commandName = "fake-structured-get")
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        return McpTestUtilities.CreateToolCallRequest(
            new CallToolRequestParams
            {
                Name = commandName,
                Arguments = arguments ?? new Dictionary<string, JsonElement>()
            },
            mockServer);
    }

    // A self-contained enum + options POCO used only by ListToolsHandler_EnumOption_IsExportedAsStringType.
    // Declaring them here keeps the enum-to-schema contract test independent of any shipping tool.
    private enum SchemaSampleLevel
    {
        Critical,
        Error,
        Informational,
        Verbose,
        Warning
    }

    private sealed class EnumSchemaTestOptions
    {
        [Option(Name = "sample-level", Description = "A sample enum option for schema testing.")]
        public SchemaSampleLevel? SampleLevel { get; set; }
    }

    private static bool UsesRawMcpToolInput(IBaseCommand command) =>
        command.GetCommand().Options.Any(BaseToolLoader.IsRawMcpToolInputOption);

    [Fact]
    public async Task ListToolsHandler_ToolsWithSecretMetadata_HaveSecretHintInMeta()
    {
        // Arrange - create a simple fake command with secret metadata
        var toolName = "fake-secret-get";
        var serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<CommandFactoryToolLoader>();
        var configuration = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration());

        // Create a fake command factory that includes a command with secret metadata
        var fakeCommand = CreateFakeCommand(toolName, new() { Secret = true });

        // Create command factory using existing helper
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory(serviceProvider);
        InjectCommandFactoryTool(commandFactory, fakeCommand);

        var toolLoader = new CommandFactoryToolLoader(commandFactory, configuration, logger);
        var request = McpTestUtilities.CreateToolListRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // Find the fake secret tool
        var secretTool = result.Tools.FirstOrDefault(t => t.Name == toolName);
        Assert.NotNull(secretTool);

        // Check that the secret tool has SecretHint in its Meta
        Assert.NotNull(secretTool.Meta);
        Assert.True(McpHelper.HasHint(secretTool, McpHelper.SecretHintMetaKey));
    }

    #region Elicitation Tests

    [Fact]
    public async Task CallToolHandler_WithSecretTool_WhenClientDoesNotSupportElicitation_RejectsExecution()
    {
        var toolName = "fake-secret-get";
        var (toolLoader, commandFactory) = CreateToolLoader();

        // Add the fake secret command to the command factory
        var fakeCommand = CreateFakeCommand(toolName, new() { Secret = true });
        fakeCommand.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(new CommandResponse { Status = HttpStatusCode.OK, Message = "Secret test response" });

        InjectCommandFactoryTool(commandFactory, fakeCommand);

        // Create mock server without elicitation capabilities
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns((ClientCapabilities?)null);

        var request = McpTestUtilities.CreateToolCallRequest(toolName, mockServer);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Should reject execution as client doesn't support elicitation (security requirement)
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Contains("does not support elicitation", ((TextContentBlock)result.Content.First()).Text);

        // Validate telemetry
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, false);
        activity.AssertTagEquals(TagName.ToolName, toolName);
        activity.AssertTagEquals(TagName.ToolId, fakeCommand.Id);
        activity.AssertTagDoesNotExist(TagName.ToolArea);
    }

    [Fact]
    public async Task CallToolHandler_WithNonSecretTool_DoesNotTriggerElicitation()
    {
        var toolName = "fake-non-secret-get";
        var (toolLoader, commandFactory) = CreateToolLoader();

        // Add a fake non-secret command to the command factory
        var fakeCommand = CreateFakeCommand(toolName, new() { Secret = false, Destructive = false });
        fakeCommand.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(new CommandResponse { Status = HttpStatusCode.OK, Message = "Test response" });

        // Add our fake command to the internal command map using reflection
        InjectCommandFactoryTool(commandFactory, fakeCommand);

        // Create mock server with elicitation capabilities
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability() });

        var request = McpTestUtilities.CreateToolCallRequest(toolName, mockServer);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Should execute without issues for non-secret tools
        Assert.NotNull(result);
        Assert.False(result.IsError);

        // Validate telemetry
        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, true);
        activity.AssertTagEquals(TagName.ToolName, toolName);
        activity.AssertTagEquals(TagName.ToolId, fakeCommand.Id);
        activity.AssertTagDoesNotExist(TagName.ToolArea);
        activity.AssertTagEquals(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(fakeCommand));
        activity.AssertTagDoesNotExist(TagName.ToolParameters);
        activity.AssertTagEquals(TagName.ToolSource, "internal");
    }

    [Fact]
    public async Task CallToolHandler_WithSecretTool_WhenDangerouslyDisableElicitationEnabled_BypassesElicitation()
    {
        var toolName = "fake-secret-get";

        // Create tool loader with dangerously disable elicitation enabled
        var configuration = new ServerRuntimeConfiguration { DangerouslyDisableElicitation = true };
        var (toolLoader, commandFactory) = CreateToolLoader(configuration);

        // Add the fake secret command to the command factory
        var fakeCommand = CreateFakeCommand(toolName, new() { Secret = true });
        fakeCommand.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(new CommandResponse { Status = HttpStatusCode.OK, Message = "Secret test response" });

        // Add our fake command to the internal command map using reflection
        InjectCommandFactoryTool(commandFactory, fakeCommand);

        // Create mock server - elicitation support doesn't matter when bypassed
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns((ClientCapabilities?)null);

        var request = McpTestUtilities.CreateToolCallRequest(toolName, mockServer);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Should execute successfully despite being a secret tool and client not supporting elicitation
        Assert.NotNull(result);
        Assert.False(result.IsError);
        var responseText = ((TextContentBlock)result.Content.First()).Text;
        var response = JsonSerializer.Deserialize<CommandResponse>(responseText);
        Assert.Equal(HttpStatusCode.OK, response!.Status);
        Assert.Equal("Secret test response", response.Message);

        // Validate telemetry
        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, true);
        activity.AssertTagEquals(TagName.ToolName, toolName);
        activity.AssertTagEquals(TagName.ToolId, fakeCommand.Id);
        activity.AssertTagDoesNotExist(TagName.ToolArea);
        activity.AssertTagEquals(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(fakeCommand));
        activity.AssertTagDoesNotExist(TagName.ToolParameters);
        activity.AssertTagEquals(TagName.ToolSource, "internal");
    }

    [Fact]
    public async Task CallToolHandler_WithSecretTool_WhenDangerouslyDisableElicitationDisabled_StillRequiresElicitation()
    {
        var toolName = "fake-secret-get";

        // Create tool loader with dangerously disable elicitation disabled (default)
        var configuration = new ServerRuntimeConfiguration { DangerouslyDisableElicitation = false };
        var (toolLoader, commandFactory) = CreateToolLoader(configuration);

        // Add the fake secret command to the command factory
        var fakeCommand = CreateFakeCommand(toolName, new() { Secret = true });
        fakeCommand.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(new CommandResponse { Status = HttpStatusCode.OK, Message = "Secret test response" });

        // Add our fake command to the internal command map using reflection
        InjectCommandFactoryTool(commandFactory, fakeCommand);

        // Create mock server without elicitation capabilities
        var mockServer = Substitute.For<McpServer>();
        mockServer.ClientCapabilities.Returns((ClientCapabilities?)null);

        var request = McpTestUtilities.CreateToolCallRequest(toolName, mockServer);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Should still reject execution when insecure option is disabled
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.Contains("does not support elicitation", ((TextContentBlock)result.Content.First()).Text);

        // Validate telemetry
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, false);
        activity.AssertTagEquals(TagName.ToolName, toolName);
        activity.AssertTagEquals(TagName.ToolId, fakeCommand.Id);
        activity.AssertTagDoesNotExist(TagName.ToolArea);
        activity.AssertTagEquals(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(fakeCommand));
        activity.AssertTagDoesNotExist(TagName.ToolParameters);
        activity.AssertTagEquals(TagName.ToolSource, "internal");
    }

    [Fact]
    public async Task CallToolHandler_WithToolFilter_AllowsSpecifiedTool()
    {
        // Arrange
        var (_, commandFactory) = CreateToolLoader();
        var availableCommands = CommandFactory.GetVisibleCommands(commandFactory.AllCommands).ToList();

        // Skip test if no commands are available
        if (!availableCommands.Any())
        {
            return;
        }

        var (toolName, tool) = availableCommands.First();
        var configuration = new ServerRuntimeConfiguration { Tool = [toolName] };
        var (toolLoader, _) = CreateToolLoader(configuration);

        var request = McpTestUtilities.CreateToolCallRequest(toolName);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        // Act
        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should not reject due to tool filtering
        Assert.NotNull(result);
        // Note: The result might still be an error for other reasons (like missing parameters),
        // but it should not be rejected specifically due to tool filtering
        if (result.IsError == true)
        {
            var errorText = ((TextContentBlock)result.Content.First()).Text;
            Assert.DoesNotContain("is not available", errorText);
            Assert.DoesNotContain("only expose the tool", errorText);
        }

        // Validate telemetry
        Assert.Equal(result.IsError == true ? ActivityStatusCode.Error : ActivityStatusCode.Ok, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, true);
        activity.AssertTagEquals(TagName.ToolName, toolName);
        activity.AssertTagEquals(TagName.ToolId, tool.Id);
        activity.AssertTagEquals(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(tool));
        activity.AssertTagDoesNotExist(TagName.ToolParameters);
        activity.AssertTagEquals(TagName.ToolSource, "internal");
    }

    [Fact]
    public async Task CallToolHandler_WithToolFilter_RejectsNonSpecifiedTool()
    {
        // Arrange
        var (_, commandFactory) = CreateToolLoader();
        var availableCommands = CommandFactory.GetVisibleCommands(commandFactory.AllCommands).ToList();

        // Skip test if fewer than 2 commands are available
        if (availableCommands.Count < 2)
        {
            return;
        }

        var (toolName, _) = availableCommands.First();
        var (otherToolName, _) = availableCommands.Skip(1).First();
        var configuration = new ServerRuntimeConfiguration { Tool = [toolName] };
        var (toolLoader, _) = CreateToolLoader(configuration);

        // Request a different tool than the filtered one
        var request = McpTestUtilities.CreateToolCallRequest(otherToolName);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        // Act
        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsError);
        var errorText = ((TextContentBlock)result.Content.First()).Text;
        Assert.Contains("is not available", errorText);
        Assert.Contains("only expose the tool", errorText);
        Assert.Contains(toolName, errorText);

        // Validate telemetry
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, false);
        activity.AssertTagEquals(TagName.ToolName, otherToolName);
        activity.AssertTagDoesNotExist(TagName.ToolArea);
        activity.AssertTagDoesNotExist(TagName.ToolId);
    }

    [Fact]
    public async Task CallToolHandler_WithToolFilterCaseInsensitive_AllowsSpecifiedTool()
    {
        // Arrange
        var (_, commandFactory) = CreateToolLoader();
        var availableCommands = CommandFactory.GetVisibleCommands(commandFactory.AllCommands).ToList();

        // Skip test if no commands are available
        if (!availableCommands.Any())
        {
            return;
        }

        var (toolName, tool) = availableCommands.First();
        var configuration = new ServerRuntimeConfiguration { Tool = [toolName.ToUpperInvariant()] }; // Set filter to uppercase
        var (toolLoader, _) = CreateToolLoader(configuration);

        // Request with original case
        var request = McpTestUtilities.CreateToolCallRequest(toolName);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        // Act
        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should not reject due to tool filtering (case insensitive match)
        Assert.NotNull(result);
        if (result.IsError == true)
        {
            var errorText = ((TextContentBlock)result.Content.First()).Text;
            Assert.DoesNotContain("is not available", errorText);
            Assert.DoesNotContain("only expose the tool", errorText);
        }

        // Validate telemetry
        Assert.Equal(result.IsError == true ? ActivityStatusCode.Error : ActivityStatusCode.Ok, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, true);
        activity.AssertTagEquals(TagName.ToolName, toolName);
        activity.AssertTagEquals(TagName.ToolId, tool.Id);
        activity.AssertTagEquals(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(tool));
        activity.AssertTagDoesNotExist(TagName.ToolParameters);
        activity.AssertTagEquals(TagName.ToolSource, "internal");
    }

    [Fact]
    public async Task ListToolsHandler_WithMultipleToolFilter_ReturnsSpecifiedTools()
    {
        // Arrange
        var (toolLoader, commandFactory) = CreateToolLoader();
        var allCommands = CommandFactory.GetVisibleCommands(commandFactory.AllCommands);

        // Skip test if we don't have at least 2 commands
        if (allCommands.Count() < 2)
        {
            return;
        }

        var toolNames = allCommands.Take(2).Select(kvp => kvp.Key).ToArray();
        var configuration = new ServerRuntimeConfiguration { Tool = toolNames };
        var (filteredToolLoader, _) = CreateToolLoader(configuration);
        var request = McpTestUtilities.CreateToolListRequest();

        // Act
        var result = await filteredToolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);
        Assert.Equal(2, result.Tools.Count);
        Assert.Contains(result.Tools, t => t.Name == toolNames[0]);
        Assert.Contains(result.Tools, t => t.Name == toolNames[1]);
    }

    #endregion

    #region Execution-Time Mode Enforcement Tests

    [Fact]
    public async Task CallToolHandler_WithReadOnlyMode_RejectsNonReadOnlyTool()
    {
        var toolName = "fake-write-tool";
        // Arrange - create a tool loader with read-only mode enabled
        var configuration = new ServerRuntimeConfiguration { ReadOnly = true };
        var (toolLoader, commandFactory) = CreateToolLoader(configuration);

        // Add a fake non-read-only command
        var fakeCommand = CreateFakeCommand(toolName, new() { ReadOnly = false });

        InjectCommandFactoryTool(commandFactory, fakeCommand);

        var request = McpTestUtilities.CreateToolCallRequest(toolName);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        // Act
        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should reject the tool call due to read-only mode
        Assert.NotNull(result);
        Assert.True(result.IsError);
        var errorText = ((TextContentBlock)result.Content.First()).Text;
        Assert.Contains("read-only mode", errorText);
        Assert.Contains("fake-write-tool", errorText);

        // Validate telemetry
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, false);
        activity.AssertTagEquals(TagName.ToolName, toolName);
        activity.AssertTagEquals(TagName.ToolId, fakeCommand.Id);
        activity.AssertTagEquals(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(fakeCommand));
        activity.AssertTagDoesNotExist(TagName.ToolParameters);
        activity.AssertTagEquals(TagName.ToolSource, "internal");
    }

    [Fact]
    public async Task CallToolHandler_WithReadOnlyMode_AllowsReadOnlyTool()
    {
        var toolName = "fake-readonly-tool";

        // Arrange - create a tool loader with read-only mode enabled
        var configuration = new ServerRuntimeConfiguration { ReadOnly = true };
        var (toolLoader, commandFactory) = CreateToolLoader(configuration);

        // Add a fake read-only command
        var fakeCommand = CreateFakeCommand(toolName, new() { ReadOnly = true, Destructive = false });
        fakeCommand.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(new CommandResponse { Status = HttpStatusCode.OK, Message = "Read-only test response" });

        InjectCommandFactoryTool(commandFactory, fakeCommand);

        var request = McpTestUtilities.CreateToolCallRequest(toolName);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        // Act
        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should allow execution of read-only tool
        Assert.NotNull(result);
        Assert.False(result.IsError);

        // Validate telemetry
        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, true);
        activity.AssertTagEquals(TagName.ToolName, toolName);
        activity.AssertTagEquals(TagName.ToolId, fakeCommand.Id);
        activity.AssertTagEquals(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(fakeCommand));
        activity.AssertTagDoesNotExist(TagName.ToolParameters);
        activity.AssertTagEquals(TagName.ToolSource, "internal");
    }

    [Fact]
    public async Task CallToolHandler_WithHttpMode_RejectsLocalRequiredTool()
    {
        // Arrange - create a tool loader with HTTP mode enabled
        var toolName = "fake-local-tool";
        var configuration = new ServerRuntimeConfiguration { Transport = TransportTypes.Http };
        var (toolLoader, commandFactory) = CreateToolLoader(configuration);

        // Add a fake local-required command
        var fakeCommand = CreateFakeCommand(toolName, new() { LocalRequired = true });

        InjectCommandFactoryTool(commandFactory, fakeCommand);

        var request = McpTestUtilities.CreateToolCallRequest(toolName);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        // Act
        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should reject the tool call due to HTTP mode
        Assert.NotNull(result);
        Assert.True(result.IsError);
        var errorText = ((TextContentBlock)result.Content.First()).Text;
        Assert.Contains("HTTP mode", errorText);
        Assert.Contains("fake-local-tool", errorText);

        // Validate telemetry
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, false);
        activity.AssertTagEquals(TagName.ToolName, toolName);
        activity.AssertTagEquals(TagName.ToolId, fakeCommand.Id);
        activity.AssertTagEquals(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(fakeCommand));
        activity.AssertTagDoesNotExist(TagName.ToolParameters);
        activity.AssertTagEquals(TagName.ToolSource, "internal");
    }

    [Fact]
    public async Task CallToolHandler_WithoutReadOnlyMode_AllowsNonReadOnlyTool()
    {
        // Arrange - create a tool loader WITHOUT read-only mode
        var toolName = "fake-write-tool-2";
        var configuration = new ServerRuntimeConfiguration { ReadOnly = false };
        var (toolLoader, commandFactory) = CreateToolLoader(configuration);

        // Add a fake non-read-only command
        var fakeCommand = CreateFakeCommand(toolName, new() { ReadOnly = false, Destructive = false });
        fakeCommand.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(new CommandResponse { Status = HttpStatusCode.OK, Message = "Write test response" });

        InjectCommandFactoryTool(commandFactory, fakeCommand);

        var request = McpTestUtilities.CreateToolCallRequest(toolName);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        // Act
        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should allow execution when read-only mode is not enabled
        Assert.NotNull(result);
        Assert.False(result.IsError);

        // Validate telemetry
        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, true);
        activity.AssertTagEquals(TagName.ToolName, toolName);
        activity.AssertTagEquals(TagName.ToolId, fakeCommand.Id);
        activity.AssertTagEquals(TagName.ToolAnnotations, McpHelper.CreateToolAnnotationTelemetry(fakeCommand));
        activity.AssertTagDoesNotExist(TagName.ToolParameters);
        activity.AssertTagEquals(TagName.ToolSource, "internal");
    }

    [Fact]
    public async Task CallToolHandler_UnknownParameters_RejectsToolCall()
    {
        var toolName = "fake-read-tool";
        // Arrange - create a tool loader with read-only mode enabled
        var (toolLoader, commandFactory) = CreateToolLoader();

        // Add a fake non-read-only command
        var fakeCommand = CreateFakeCommand(toolName, new() { ReadOnly = true, Destructive = false });

        InjectCommandFactoryTool(commandFactory, fakeCommand);

        var request = McpTestUtilities.CreateToolCallRequest(toolName);
        request.Params.Arguments?.Add("unknown-param", JsonDocument.Parse("\"some-value\"").RootElement);

        using var activity = new Activity("test-activity");
        activity.Start();

        var mcpRuntime = CreateRuntime(toolLoader, activity);

        // Act
        var result = await mcpRuntime.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should reject the tool call due to unknown parameter
        Assert.NotNull(result);
        Assert.True(result.IsError);
        var errorText = ((TextContentBlock)result.Content.First()).Text;
        Assert.Contains("unknown-param", errorText);

        // Validate telemetry
        activity.AssertTagEquals(TagName.IsServerCommandInvoked, false);
        activity.AssertTagEquals(TagName.ToolName, toolName);
        activity.AssertTagEquals(TagName.ToolId, fakeCommand.Id);
        activity.AssertTagDoesNotExist(TagName.ToolArea);
        activity.AssertTagEquals(TagName.ToolParameters, toolParameters =>
        {
            var parameterList = JsonSerializer.Deserialize(toolParameters.ToString()!, ModelsJsonContext.Default.ListString);
            Assert.NotNull(parameterList);
            Assert.Single(parameterList);
            Assert.Contains("unknown-param", parameterList);
        });
    }

    #endregion
}
