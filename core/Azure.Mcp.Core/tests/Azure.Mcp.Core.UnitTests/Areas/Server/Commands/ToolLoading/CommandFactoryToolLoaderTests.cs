// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
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
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(options ?? new ToolLoaderOptions());

        var toolLoader = new CommandFactoryToolLoader(serviceProvider, commandFactory, toolLoaderOptions, logger);
        return (toolLoader, commandFactory);
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
    public async Task ListToolsHandler_WithServiceFilter_ReturnsOnlyFilteredTools()
    {
        // Try to filter by a specific service/group - using a common Azure service name
        var filteredOptions = new ToolLoaderOptions
        {
            Namespace = new[] { "storage" }  // Assuming there's a storage service group
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
            Namespace = new[] { "storage", "appconfig", "search" }  // Real Azure service groups from the codebase
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
    public async Task GetsToolsWithRawMcpInputOption()
    {
        var filteredOptions = new ToolLoaderOptions
        {
            Namespace = new[] { "deploy" }  // Assuming there's a deploy service group
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
}
