// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client.Helpers;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

public sealed class CommandFactoryToolLoaderConsolidatedTests
{
    private static CommandFactoryToolLoader CreateLoader(ServerRuntimeConfiguration? configuration = null)
    {
        var providerLogger = Substitute.For<ILogger<ResourceConsolidatedToolDefinitionProvider>>();
        var definitionProvider = new ResourceConsolidatedToolDefinitionProvider(
            providerLogger,
            typeof(Mcp.Server.Program).Assembly,
            "consolidated-tools.json");

        return new CommandFactoryToolLoader(
            CommandFactoryHelpers.CreateCommandFactory(),
            Microsoft.Extensions.Options.Options.Create(configuration ?? new ServerRuntimeConfiguration
            {
                Mode = ModeTypes.ConsolidatedProxy
            }),
            Substitute.For<ILogger<CommandFactoryToolLoader>>(),
            definitionProvider);
    }

    [Fact]
    public async Task ListToolsHandler_ExposesConsolidatedCommandGroups()
    {
        var loader = CreateLoader();

        var result = await loader.ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(),
            TestContext.Current.CancellationToken);

        Assert.NotEmpty(result.Tools);
        Assert.All(result.Tools, tool =>
        {
            Assert.Contains("hierarchical", tool.Description, StringComparison.OrdinalIgnoreCase);
            Assert.NotEmpty(loader.GetChildToolList(tool.Name));
        });
    }

    [Fact]
    public async Task ListToolsHandler_NamespaceFilter_FiltersMappedCommands()
    {
        var loader = CreateLoader(new ServerRuntimeConfiguration
        {
            Mode = ModeTypes.ConsolidatedProxy,
            Namespace = ["storage"]
        });

        var result = await loader.ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(),
            TestContext.Current.CancellationToken);

        Assert.NotEmpty(result.Tools);
        Assert.All(
            result.Tools.SelectMany(tool => loader.GetChildToolList(tool.Name)),
            child => Assert.Contains("storage", child.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ListToolsHandler_ReadOnlyMode_ExposesOnlyReadOnlyChildren()
    {
        var loader = CreateLoader(new ServerRuntimeConfiguration
        {
            Mode = ModeTypes.ConsolidatedProxy,
            ReadOnly = true
        });

        var result = await loader.ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(),
            TestContext.Current.CancellationToken);

        Assert.NotEmpty(result.Tools);
        Assert.All(
            result.Tools.SelectMany(tool => loader.GetChildToolList(tool.Name)),
            child => Assert.True(child.Annotations?.ReadOnlyHint));
    }

    [Fact]
    public async Task ListToolsHandler_WithoutDefinitionProvider_Throws()
    {
        var loader = new CommandFactoryToolLoader(
            CommandFactoryHelpers.CreateCommandFactory(),
            Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration { Mode = ModeTypes.ConsolidatedProxy }),
            Substitute.For<ILogger<CommandFactoryToolLoader>>());

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public void AreMetadataEqual_ComparesAllHints()
    {
        var metadata = new ToolMetadata
        {
            Destructive = true,
            Idempotent = false,
            OpenWorld = true,
            ReadOnly = false,
            Secret = true,
            LocalRequired = true
        };

        Assert.True(CommandFactoryToolLoader.AreMetadataEqual(metadata, metadata));
        Assert.False(CommandFactoryToolLoader.AreMetadataEqual(metadata, new ToolMetadata()));
        Assert.False(CommandFactoryToolLoader.AreMetadataEqual(metadata, null));
        Assert.True(CommandFactoryToolLoader.AreMetadataEqual(null, null));
    }
}
