// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Tests.Client.Helpers;
using NSubstitute;
using Xunit;
using Xunit.Sdk;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class FabricCoreCatalogTests()
{
    private static readonly (string Name, Type CommandType, bool ReadOnly)[] ExpectedTools =
    [
        ("core_assign-workspace-to-capacity", typeof(WorkspaceAssignToCapacityCommand), false),
        ("core_create-item", typeof(ItemCreateCommand), false),
        ("core_create-workspace", typeof(WorkspaceCreateCommand), false),
        ("core_delete-item", typeof(ItemDeleteCommand), false),
        ("core_delete-workspace", typeof(WorkspaceDeleteCommand), false),
        ("core_get-capacity", typeof(CapacityGetCommand), true),
        ("core_get-item", typeof(ItemGetCommand), true),
        ("core_get-workspace", typeof(WorkspaceGetCommand), true),
        ("core_list-capacities", typeof(CapacityListCommand), true),
        ("core_list-items", typeof(ItemListCommand), true),
        ("core_list-workspaces", typeof(WorkspaceListCommand), true),
        ("core_search-catalog", typeof(CatalogSearchCommand), true),
        ("core_update-item", typeof(ItemUpdateCommand), false),
        ("core_update-workspace", typeof(WorkspaceUpdateCommand), false)
    ];

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, StructuredOutputMode.Compact)]
    [InlineData(false, StructuredOutputMode.Duplicated)]
    [InlineData(true, null)]
    [InlineData(true, StructuredOutputMode.Compact)]
    [InlineData(true, StructuredOutputMode.Duplicated)]
    public static async Task RegisteredCatalog_MatchesManifestAndPreservesSchemas(bool readOnly, StructuredOutputMode? mode)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Discovery must not send HTTP."));
        var credential = Substitute.For<TokenCredential>();
        var services = new ServiceCollection();
        var setup = new FabricCoreSetup();
        setup.ConfigureServices(services);
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        services.AddSingleton(credential);
        services.AddLogging();
        services.AddSingleton<IAreaSetup>(setup);
        services.AddSingleton(Substitute.For<ITelemetryService>());
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            RootCommandGroupName = "fabmcp",
            Name = "Fabric.Mcp.Server",
            ShortName = "fabric",
            DisplayName = "Microsoft Fabric MCP Server",
            Version = "1.0.0",
            Description = "Offline Core catalog tests",
            IsTelemetryEnabled = false
        }));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            Transport = TransportTypes.StdIo,
            StructuredOutputMode = mode,
            ReadOnly = readOnly
        }));
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<CommandFactoryToolLoader>();
        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<ICommandFactory>();
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IFabricCoreService));
        AssertCatalog(factory.AllCommands.Select(pair => (pair.Key, (bool?)pair.Value.Metadata.ReadOnly)), readOnly: false);
        foreach (var expected in ExpectedTools)
        {
            var descriptor = Assert.Single(services, descriptor => descriptor.ServiceType == expected.CommandType);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
            var command = factory.AllCommands[expected.Name];
            Assert.IsType(expected.CommandType, command);
            Assert.Same(provider.GetRequiredService(expected.CommandType), command);
        }

        var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);

        AssertCatalog(catalog.Tools.Select(tool => (tool.Name, tool.Annotations?.ReadOnlyHint)), readOnly);
        foreach (var tool in catalog.Tools)
        {
            var command = factory.AllCommands[tool.Name];
            Assert.Equal(command.Metadata.Destructive, tool.Annotations?.DestructiveHint);
            Assert.Equal(command.Metadata.Idempotent, tool.Annotations?.IdempotentHint);
            Assert.Equal(command.Metadata.OpenWorld, tool.Annotations?.OpenWorldHint);
            Assert.Equal(mode.HasValue && command.ResultTypeInfo is not null, tool.OutputSchema.HasValue);
            Assert.Equal(command.GetCommand().Options.Where(option => option.Name != "--learn")
                .Select(option => option.Name.TrimStart('-')).Order(StringComparer.Ordinal),
                tool.InputSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order(StringComparer.Ordinal));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(false, "missing")]
    [InlineData(false, "unexpected")]
    [InlineData(false, "duplicate")]
    [InlineData(false, "misclassified")]
    [InlineData(true, "missing")]
    [InlineData(true, "unexpected")]
    [InlineData(true, "duplicate")]
    [InlineData(true, "misclassified")]
    public static void CatalogAssertion_RejectsMembershipAndClassificationRegressions(bool readOnly, string change)
    {
        var actual = ExpectedTools.Where(tool => !readOnly || tool.ReadOnly)
            .Select(tool => (tool.Name, ReadOnly: (bool?)tool.ReadOnly)).ToList();
        switch (change)
        {
            case "missing":
                actual.RemoveAt(0);
                break;
            case "unexpected":
                actual.Add(("core_unexpected-tool", true));
                break;
            case "duplicate":
                actual.Add(actual[0]);
                break;
            case "misclassified":
                actual[0] = (actual[0].Name, !actual[0].ReadOnly);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change));
        }

        Assert.ThrowsAny<XunitException>(() => AssertCatalog(actual, readOnly));
    }

    private static void AssertCatalog(IEnumerable<(string Name, bool? ReadOnly)> actual, bool readOnly)
    {
        var expected = ExpectedTools.Where(tool => !readOnly || tool.ReadOnly)
            .Select(tool => (tool.Name, ReadOnly: (bool?)tool.ReadOnly));
        Assert.Equal(expected.OrderBy(tool => tool.Name, StringComparer.Ordinal),
            actual.OrderBy(tool => tool.Name, StringComparer.Ordinal));
    }
}
