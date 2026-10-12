// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Server.Tests.Infrastructure;

public sealed class ConsolidatedToolMetadataTests()
{
    [Fact]
    public async Task ConsolidatedTools_MappedCommands_HaveMatchingMetadata()
    {
        ServiceCollection serviceCollection = new();
        Program.ConfigureServices(serviceCollection);
        await using var services = serviceCollection.BuildServiceProvider();

        var commandFactory = services.GetRequiredService<ICommandFactory>();
        var definitionProvider = services.GetRequiredService<IConsolidatedToolDefinitionProvider>();
        var mismatches = new List<string>();

        foreach (var consolidatedTool in definitionProvider.GetToolDefinitions())
        {
            foreach (var commandName in consolidatedTool.MappedToolList)
            {
                if (!commandFactory.AllCommands.TryGetValue(commandName, out var command))
                {
                    // Definitions can include commands that are not registered in this server build.
                    continue;
                }

                if (!MetadataMatches(command.Metadata, consolidatedTool.ToolMetadata))
                {
                    mismatches.Add(
                        $"Command '{commandName}' mapped to consolidated tool '{consolidatedTool.Name}' has metadata " +
                        $"{FormatMetadata(command.Metadata)}; expected {FormatMetadata(consolidatedTool.ToolMetadata)}.");
                }
            }
        }

        Assert.True(
            mismatches.Count == 0,
            $"Found {mismatches.Count} invalid consolidated tool mapping(s):{Environment.NewLine}" +
            string.Join(Environment.NewLine, mismatches));
    }

    private static bool MetadataMatches(ToolMetadata actual, ToolMetadata expected) =>
        actual.Destructive == expected.Destructive &&
        actual.Idempotent == expected.Idempotent &&
        actual.OpenWorld == expected.OpenWorld &&
        actual.ReadOnly == expected.ReadOnly &&
        actual.Secret == expected.Secret &&
        actual.LocalRequired == expected.LocalRequired;

    private static string FormatMetadata(ToolMetadata metadata) =>
        $"[Destructive={metadata.Destructive}, Idempotent={metadata.Idempotent}, " +
        $"OpenWorld={metadata.OpenWorld}, ReadOnly={metadata.ReadOnly}, " +
        $"Secret={metadata.Secret}, LocalRequired={metadata.LocalRequired}]";
}
