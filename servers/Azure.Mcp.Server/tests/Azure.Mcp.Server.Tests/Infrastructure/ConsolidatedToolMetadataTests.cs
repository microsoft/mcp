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
    public async Task AdvisorBatchUpdate_UsesTheExistingDestructiveConsolidatedTool()
    {
        ServiceCollection serviceCollection = new();
        Program.ConfigureServices(serviceCollection);
        await using var services = serviceCollection.BuildServiceProvider();

        var command = services.GetRequiredService<ICommandFactory>().FindCommandByName("advisor_recommendation_update");
        Assert.NotNull(command);
        var definition = Assert.Single(
            services.GetRequiredService<IConsolidatedToolDefinitionProvider>().GetToolDefinitions(),
            tool => tool.MappedToolList.Contains("advisor_recommendation_update"));

        Assert.Equal("update_azure_advisor_recommendation_state", definition.Name);
        Assert.Single(definition.MappedToolList);
        Assert.True(command.Metadata.Destructive);
        Assert.False(command.Metadata.ReadOnly);
        Assert.True(MetadataMatches(command.Metadata, definition.ToolMetadata));
        Assert.Contains(command.GetCommand().Options, option => option.Name == "--recommendation-ids");
    }

    [Theory]
    [InlineData("advisor_recommendation_list")]
    [InlineData("advisor_recommendation_summary")]
    [InlineData("advisor_recommendation_prevalidate")]
    public async Task AdvisorReads_ShareOneExistingConsolidatedTool(string commandName)
    {
        ServiceCollection serviceCollection = new();
        Program.ConfigureServices(serviceCollection);
        await using var services = serviceCollection.BuildServiceProvider();

        var command = services.GetRequiredService<ICommandFactory>().FindCommandByName(commandName);
        Assert.NotNull(command);
        var definition = Assert.Single(
            services.GetRequiredService<IConsolidatedToolDefinitionProvider>().GetToolDefinitions(),
            tool => tool.MappedToolList.Contains(commandName));

        Assert.Equal("list_azure_advisor_recommendations", definition.Name);
        Assert.Single(definition.MappedToolList, name => name == commandName);
        Assert.True(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.Destructive);
        Assert.True(MetadataMatches(command.Metadata, definition.ToolMetadata));
    }

    [Theory]
    [InlineData("resiliency_goal_assignment_recommend-capacity", "recommend_azure_resilience_goal_capacity")]
    [InlineData("resiliency_goal_assignment_refresh-resources", "update_azure_resilience_goal_resources")]
    [InlineData("resiliency_goal_assignment_update-resources", "update_azure_resilience_goal_resources")]
    public async Task GoalAssignmentActions_AreRegisteredAndMappedExactlyOnce(string commandName, string consolidatedName)
    {
        ServiceCollection serviceCollection = new();
        Program.ConfigureServices(serviceCollection);
        await using var services = serviceCollection.BuildServiceProvider();
        var commands = services.GetRequiredService<ICommandFactory>();
        Assert.True(commands.AllCommands.ContainsKey(commandName));
        var definitions = services.GetRequiredService<IConsolidatedToolDefinitionProvider>();
        var definition = Assert.Single(definitions.GetToolDefinitions(), tool => tool.MappedToolList.Contains(commandName));
        Assert.Equal(consolidatedName, definition.Name);
        Assert.True(MetadataMatches(commands.AllCommands[commandName].Command.Metadata, definition.ToolMetadata));
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
