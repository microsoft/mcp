// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

public sealed class CommandFactoryToolLoaderNamespaceTests
{
    private static (CommandFactoryToolLoader Loader, ICommandFactory Factory) CreateLoader(
        ServerRuntimeConfiguration? configuration = null)
    {
        var factory = CommandFactoryHelpers.CreateCommandFactory();
        var options = Microsoft.Extensions.Options.Options.Create(configuration ?? new ServerRuntimeConfiguration
        {
            Mode = ModeTypes.NamespaceProxy
        });
        var loader = new CommandFactoryToolLoader(
            factory,
            options,
            Substitute.For<ILogger<CommandFactoryToolLoader>>());

        return (loader, factory);
    }

    [Fact]
    public async Task ListToolsHandler_NamespaceMode_ExposesCommandGroupsAndUtilityCommands()
    {
        var (loader, factory) = CreateLoader();

        var result = await loader.ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(),
            TestContext.Current.CancellationToken);

        var namespaceTools = result.Tools
            .Where(tool => tool.Description?.Contains("hierarchical", StringComparison.OrdinalIgnoreCase) == true)
            .ToList();
        Assert.True(namespaceTools.Count > 50);
        Assert.All(namespaceTools, tool =>
        {
            var properties = tool.InputSchema.AssertProperty("properties");
            properties.AssertProperty("intent");
            properties.AssertProperty("command");
            properties.AssertProperty("parameters");
            properties.AssertProperty("learn");
        });

        var utilityCommandNames = CommandFactory.GetVisibleCommands(factory.GroupCommands(CommandFactoryToolLoader.UtilityNamespaces))
            .Select(command => command.Key);
        Assert.All(utilityCommandNames, commandName => Assert.Contains(result.Tools, tool => tool.Name == commandName));
    }

    [Fact]
    public async Task ListToolsHandler_NamespaceFilter_AppliesOnlyToCommandGroups()
    {
        var (loader, _) = CreateLoader(new ServerRuntimeConfiguration
        {
            Mode = ModeTypes.NamespaceProxy,
            Namespace = ["storage", "keyvault"]
        });

        var result = await loader.ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(),
            TestContext.Current.CancellationToken);

        var namespaceNames = result.Tools
            .Where(tool => tool.Description?.Contains("hierarchical", StringComparison.OrdinalIgnoreCase) == true)
            .Select(tool => tool.Name)
            .ToList();
        Assert.Equal(2, namespaceNames.Count);
        Assert.Contains("storage", namespaceNames);
        Assert.Contains("keyvault", namespaceNames);
    }

    [Fact]
    public async Task ListToolsHandler_SingleMode_ExposesOnlyCommandGroups()
    {
        var (loader, _) = CreateLoader(new ServerRuntimeConfiguration { Mode = ModeTypes.SingleToolProxy });

        var result = await loader.ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(),
            TestContext.Current.CancellationToken);

        Assert.NotEmpty(result.Tools);
        Assert.All(result.Tools, tool => Assert.Contains("hierarchical", tool.Description, StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Tools, tool => CommandFactoryToolLoader.UtilityNamespaces.Contains(tool.Name));
    }

    [Fact]
    public async Task ListToolsHandler_ReadOnlyMode_FiltersGroupsAndChildCommands()
    {
        var (loader, _) = CreateLoader(new ServerRuntimeConfiguration
        {
            Mode = ModeTypes.NamespaceProxy,
            ReadOnly = true
        });

        var result = await loader.ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(),
            TestContext.Current.CancellationToken);

        foreach (var namespaceTool in result.Tools.Where(tool => loader.ContainsCommandGroup(tool.Name)))
        {
            Assert.All(loader.GetChildToolList(namespaceTool.Name), child => Assert.True(child.Annotations?.ReadOnlyHint));
        }
    }

    [Fact]
    public async Task ListToolsHandler_HttpMode_FiltersLocalRequiredGroupsAndChildCommands()
    {
        var (loader, _) = CreateLoader(new ServerRuntimeConfiguration
        {
            Mode = ModeTypes.NamespaceProxy,
            Transport = TransportTypes.Http
        });

        var result = await loader.ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(),
            TestContext.Current.CancellationToken);

        foreach (var namespaceTool in result.Tools.Where(tool => loader.ContainsCommandGroup(tool.Name)))
        {
            Assert.All(
                loader.GetChildToolList(namespaceTool.Name),
                child => Assert.False(Microsoft.Mcp.Core.Helpers.McpHelper.HasHint(child, Microsoft.Mcp.Core.Helpers.McpHelper.LocalRequiredHintMetaKey)));
        }
    }

    [Fact]
    public async Task CallToolHandler_Learn_ReturnsAvailableChildCommands()
    {
        var (loader, _) = CreateLoader(new ServerRuntimeConfiguration
        {
            Mode = ModeTypes.NamespaceProxy,
            Namespace = ["storage"]
        });
        var request = McpTestUtilities.CreateToolCallRequest("storage", new Dictionary<string, object?>
        {
            ["intent"] = "List storage resources",
            ["learn"] = true
        });

        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        Assert.Contains("available commands", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CallToolHandler_UnknownNamespace_ReturnsNotFound()
    {
        var (loader, _) = CreateLoader();
        var request = McpTestUtilities.CreateToolCallRequest("not-a-namespace", new Dictionary<string, object?>
        {
            ["intent"] = "Do something",
            ["learn"] = true
        });

        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("not-a-namespace", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
    }
}
