// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ML.Tokenizers;
using Microsoft.Mcp.Core.Commands;
using System.Text.Json;

namespace Azure.Mcp.Server.Perf;

/// <summary>
/// Instantiates the full server in-process and counts the number of registered commands
/// plus the exact token budget for the tools/list MCP response using the GPT-4o tokenizer.
///
/// Modes:
///   --count-tools          Builds DI container, counts commands, reports exact+approx tokens
///                          for the name+description list.
///   --count-tokens <file>  Counts exact tokens in a raw tools/list JSON response file
///                          (e.g. produced by Test-StartupPerformance.ps1).
/// </summary>
internal static class ToolCountMeasurement
{
    // GPT-4o uses o200k_base. GPT-4/GPT-3.5-turbo use cl100k_base.
    // Pass the model name and the library picks the correct encoding.
    private static readonly TiktokenTokenizer _gpt4oTokenizer =
        TiktokenTokenizer.CreateForModel("gpt-4o");

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

        var summaryJson = JsonSerializer.Serialize(commandSummaries,
            new JsonSerializerOptions { WriteIndented = false });

        var exactTokens = _gpt4oTokenizer.CountTokens(summaryJson);
        var approxTokens = summaryJson.Length / 4;

        var result = new
        {
            timestamp = DateTimeOffset.UtcNow,
            commandCount = allCommands.Count,
            nameAndDescriptionJsonBytes = summaryJson.Length,
            nameAndDescriptionTokens = new
            {
                exact_gpt4o_o200k_base = exactTokens,
                approx_bytes_div_4 = approxTokens,
                error_pct = Math.Round(Math.Abs(approxTokens - exactTokens) / (double)exactTokens * 100, 1)
            },
            notes = "exact_gpt4o uses o200k_base encoding via Microsoft.ML.Tokenizers. " +
                    "Run Test-StartupPerformance.ps1 for the full tools/list schema token count."
        };

        // WriteIndented keeps --count-tools human-readable; --count-tokens uses compact JSON for easy PowerShell parsing
        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
    }

    /// <summary>
    /// Counts exact tokens in a startup-e2e.json file produced by Test-StartupPerformance.ps1.
    /// The tools/list response line is stored in the JSON under scenarios.tools_list_payload.
    /// For exact full-schema token counting, pass the raw tools/list JSON-RPC response line.
    /// </summary>
    internal static Task RunCountTokensAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"File not found: {filePath}");
            Environment.Exit(1);
        }

        var text = File.ReadAllText(filePath);
        var exactTokens = _gpt4oTokenizer.CountTokens(text);
        var approxTokens = text.Length / 4;

        var result = new
        {
            file = filePath,
            bytes = System.Text.Encoding.UTF8.GetByteCount(text),
            tokens = new
            {
                exact_gpt4o_o200k_base = exactTokens,
                approx_bytes_div_4 = approxTokens,
                error_pct = Math.Round(Math.Abs(approxTokens - exactTokens) / (double)exactTokens * 100, 1)
            }
        };

        // Compact (single-line) JSON so PowerShell can parse it with ConvertFrom-Json without rejoining lines
        Console.WriteLine(JsonSerializer.Serialize(result));
        return Task.CompletedTask;
    }
}
