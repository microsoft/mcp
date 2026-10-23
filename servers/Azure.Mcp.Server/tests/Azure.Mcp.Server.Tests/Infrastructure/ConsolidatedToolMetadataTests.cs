// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Server.Tests.Infrastructure;

public sealed class ConsolidatedToolMetadataTests()
{
    [Theory]
    [InlineData("resiliency_goal_assignment_create", "create_azure_resilience_management_resources", true, true)]
    [InlineData("resiliency_goal_assignment_update", "create_azure_resilience_management_resources", true, true)]
    [InlineData("resiliency_goal_assignment_delete", "create_azure_resilience_management_resources", true, true)]
    [InlineData("resiliency_goal_assignment_update-resources", "create_azure_resilience_management_resources", true, true)]
    [InlineData("resiliency_goal_assignment_recommend-capacity", "assess_azure_resilience_goal_resources", false, false)]
    [InlineData("resiliency_goal_assignment_refresh-resources", "assess_azure_resilience_goal_resources", false, false)]
    public async Task GoalAssignmentMutations_AreRegisteredAndMappedExactlyOnce(
        string commandName, string consolidatedName, bool destructive, bool idempotent)
    {
        ServiceCollection serviceCollection = new();
        Program.ConfigureServices(serviceCollection);
        await using var services = serviceCollection.BuildServiceProvider();

        var commandFactory = services.GetRequiredService<ICommandFactory>();
        var definitionProvider = services.GetRequiredService<IConsolidatedToolDefinitionProvider>();
        var command = commandFactory.FindCommandByName(commandName);
        Assert.NotNull(command);

        var definition = Assert.Single(definitionProvider.GetToolDefinitions(),
            tool => tool.MappedToolList.Contains(commandName));
        Assert.Single(definition.MappedToolList, name => name == commandName);
        Assert.Equal(consolidatedName, definition.Name);
        Assert.Equal(destructive, command.Metadata.Destructive);
        Assert.Equal(idempotent, command.Metadata.Idempotent);
        Assert.False(command.Metadata.ReadOnly);
        Assert.True(MetadataMatches(command.Metadata, definition.ToolMetadata));
    }

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

                if (!MetadataMatches(command.Command.Metadata, consolidatedTool.ToolMetadata))
                {
                    mismatches.Add(
                        $"Command '{commandName}' mapped to consolidated tool '{consolidatedTool.Name}' has metadata " +
                        $"{FormatMetadata(command.Command.Metadata)}; expected {FormatMetadata(consolidatedTool.ToolMetadata)}.");
                }
            }
        }

        Assert.True(
            mismatches.Count == 0,
            $"Found {mismatches.Count} invalid consolidated tool mapping(s):{Environment.NewLine}" +
            string.Join(Environment.NewLine, mismatches));
    }

    [Fact]
    public async Task ResiliencyAgentConsolidatedTools_AreLocalAndNotReadOnly()
    {
        ServiceCollection serviceCollection = new();
        Program.ConfigureServices(serviceCollection);
        await using var services = serviceCollection.BuildServiceProvider();

        var definitionProvider = services.GetRequiredService<IConsolidatedToolDefinitionProvider>();
        var definitions = definitionProvider.GetToolDefinitions();

        foreach (string name in new[] { "prepare_azure_resiliency_file", "use_azure_resiliency_agent" })
        {
            var definition = Assert.Single(definitions, candidate => candidate.Name == name);
            Assert.True(definition.ToolMetadata.LocalRequired);
            Assert.False(definition.ToolMetadata.ReadOnly);
        }
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
