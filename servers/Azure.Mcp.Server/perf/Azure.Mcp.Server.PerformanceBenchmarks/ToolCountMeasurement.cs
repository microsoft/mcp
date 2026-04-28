// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Server.Perf;

/// <summary>
/// Instantiates the full server in-process and counts the number of registered commands
/// plus the exact token budget using the GPT-4o tokenizer.
///
/// Mode:
///   --count-tools   Builds DI container, counts commands, reports name+description
///                   byte and token cost. For full-schema token cost (including
///                   inputSchema per tool) use --mcp-startup instead.
/// </summary>
internal static class ToolCountMeasurement
{
    internal static async Task RunCountToolsAsync()
    {
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.None));

        await using var sp = services.BuildServiceProvider();
        await Program.InitializeServicesAsync(sp);

        var commandFactory = sp.GetRequiredService<ICommandFactory>();
        var allCommands = commandFactory.AllCommands;

        var commandSummaries = allCommands
            .Select(kvp => new { name = kvp.Key, description = kvp.Value.Description })
            .OrderBy(c => c.name)
            .ToList();

        var summaryJson = JsonSerializer.Serialize(commandSummaries);
        var (bytes, exactTokens, approxTokens) = PerfTokenizer.Measure(summaryJson);

        var result = new
        {
            timestamp = DateTimeOffset.UtcNow,
            commandCount = allCommands.Count,
            nameAndDescriptionJsonBytes = bytes,
            nameAndDescriptionTokens = new
            {
                exact_gpt4o_o200k_base = exactTokens,
                approx_bytes_div_4 = approxTokens,
                error_pct = exactTokens == 0
                    ? (double?)null
                    : Math.Round(Math.Abs(approxTokens - exactTokens) / (double)exactTokens * 100, 1)
            },
            notes = "Counts name+description only. Use --mcp-startup for full inputSchema token cost."
        };

        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
    }
}
