// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using CopilotCliTester.Models;

namespace CopilotCliTester;

/// <summary>
/// E2E Test Runner for Azure MCP tools using Copilot SDK.
/// Runs prompts from e2eTestPrompts.md and verifies correct tools are invoked.
/// </summary>
class Program
{
    private static readonly TimeSpan PerAttemptTimeout = TimeSpan.FromMinutes(5);

    static async Task<int> Main(string[] args)
    {
        // Simple command line parsing
        var command = args.Length > 0 ? args[0].ToLowerInvariant() : "run";

        return command switch
        {
            "run" => await RunE2ETestsFromArgs(args.Skip(1).ToArray()),
            "--help" or "-h" => ShowHelp(),
            _ => await RunE2ETestsFromArgs(args) // Default to run
        };
    }

    static int ShowHelp()
    {
        Console.WriteLine("""
            Azure MCP E2E Test Runner (Copilot SDK)

            Usage:
              CopilotCliTester run [options]      Run E2E tests

            Options:
              --namespace <name>  Filter by namespace (partial match)
              --tool <name>       Filter by tool name (exact match)
              --max <n>           Maximum number of prompts to test (0 = all)
              --retries <n>       Maximum retry attempts per prompt (default: 3)
              --one-per-tool      Test only one prompt per tool
              --output <dir>      Output directory for reports
              --model <name>      Model to use (default: claude-sonnet-4.5)
            """);
        return 0;
    }

    static async Task<int> RunE2ETestsFromArgs(string[] args)
    {
        string? namespaceFilter = null, tool = null, outputDir = "reports", model = "claude-sonnet-4.5";
        int max = 0, retries = 3;
        bool onePerTool = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--namespace" when i + 1 < args.Length:
                    namespaceFilter = args[++i];
                    break;
                case "--tool" when i + 1 < args.Length:
                    tool = args[++i];
                    break;
                case "--max" when i + 1 < args.Length:
                    int.TryParse(args[++i], out max);
                    break;
                case "--retries" when i + 1 < args.Length:
                    int.TryParse(args[++i], out retries);
                    break;
                case "--one-per-tool":
                    onePerTool = true;
                    break;
                case "--output" when i + 1 < args.Length:
                    outputDir = args[++i];
                    break;
                case "--model" when i + 1 < args.Length:
                    model = args[++i];
                    break;
            }
        }

        return await RunE2ETests(namespaceFilter, tool, max, retries, onePerTool, outputDir, model);
    }

    static async Task<int> RunE2ETests(string? namespaceFilter, string? tool, int max, int retries, bool onePerTool, string outputDir, string model)
    {
        Console.WriteLine("--------------------------------------------");
        Console.WriteLine("Azure MCP E2E Test Runner (Copilot SDK)");
        Console.WriteLine("--------------------------------------------");
        Console.WriteLine();

        var (testContextPath, promptsPath) = LoadFiles();

        // Load test context
        var testContext = testContextPath is not null ? File.ReadAllText(testContextPath).Trim() : "";
        if (!string.IsNullOrEmpty(testContext))
        {
            Console.WriteLine("SUCCESS: Loaded test context");
        }

        // Load prompts
        if (promptsPath is null)
        {
            Console.Error.WriteLine("ERROR: e2eTestPrompts.md not found");
            return 1;
        }
        Console.WriteLine($"SUCCESS: Loading prompts from: {promptsPath}");
        var allPrompts = PromptParser.ParseFile(promptsPath);
        Console.WriteLine($"  Found {allPrompts.Count} total prompts");

        // Apply filters
        if (!string.IsNullOrWhiteSpace(namespaceFilter))
        {
            allPrompts = allPrompts
                .Where(p => p.Namespace.Contains(namespaceFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Console.WriteLine($"  → Filtered to namespace \"{namespaceFilter}\": {allPrompts.Count} prompts");
        }

        if (!string.IsNullOrWhiteSpace(tool))
        {
            allPrompts = allPrompts
                .Where(p => string.Equals(p.Tool, tool, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Console.WriteLine($"  → Filtered to tool \"{tool}\": {allPrompts.Count} prompts");
        }

        if (onePerTool)
        {
            allPrompts = allPrompts
                .GroupBy(p => p.Tool, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();
            Console.WriteLine($"  → One per tool: {allPrompts.Count} prompts");
        }

        if (max > 0 && allPrompts.Count > max)
        {
            allPrompts = allPrompts.Take(max).ToList();
            Console.WriteLine($"  → Limited to {max} prompts");
        }

        if (allPrompts.Count == 0)
        {
            Console.WriteLine("No prompts matched the filter criteria.");
            return 0;
        }

        // Group by namespace
        var groupedByNamespace = allPrompts
            .GroupBy(p => p.Namespace)
            .Select(g => (Namespace: g.Key, Prompts: g.ToList()))
            .OrderBy(g => g.Namespace)
            .ToList();

        Console.WriteLine();
        Console.WriteLine($"Testing {allPrompts.Count} prompts across {groupedByNamespace.Count} namespaces");
        Console.WriteLine($"Retries: {retries}, Model: {model}");
        Console.WriteLine("--------------------------------------------------------------------------------");
        Console.WriteLine();

        var results = new List<TestResult>();
        var totalStopwatch = Stopwatch.StartNew();

        Directory.CreateDirectory(outputDir);
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var title = !string.IsNullOrWhiteSpace(namespaceFilter) ? $"-{namespaceFilter}" : "";
        var reportFile = Path.Combine(outputDir, $"e2e-report{title}-{timestamp}.md");
        InitializeMarkdownReport(reportFile);
        Console.WriteLine($"Live report: {reportFile}");
        Console.WriteLine();

        await using var runner = new AgentRunner();

        foreach (var (namespaceName, prompts) in groupedByNamespace)
        {
            Console.WriteLine($"------ {namespaceName} ({prompts.Count} prompts) -----");

            foreach (var prompt in prompts)
            {
                var result = await ProcessPromptAsync(runner, prompt, namespaceName, testContext, model, retries);
                results.Add(result);
                AppendResultToMarkdown(reportFile, result);
            }

            Console.WriteLine();
        }

        totalStopwatch.Stop();

        // Generate summary
        var passed = results.Count(r => r.Status == TestStatus.PASS);
        var failed = results.Count(r => r.Status != TestStatus.PASS);
        var passRate = results.Count > 0 ? (double)passed / results.Count * 100 : 0;

        Console.WriteLine(new string('═', 64));
        Console.WriteLine("SUMMARY");
        Console.WriteLine(new string('─', 64));
        Console.WriteLine($"  Total:     {results.Count}");
        Console.WriteLine($"  Passed:    {passed}");
        Console.WriteLine($"  Failed:    {failed}");
        Console.WriteLine($"  Pass Rate: {passRate:F1}%");
        Console.WriteLine($"  Duration:  {totalStopwatch.Elapsed.TotalSeconds:F1}s");
        Console.WriteLine(new string('═', 64));

        // Write final JSON results
        var resultsFile = Path.Combine(outputDir, $"e2e-results{title}-{timestamp}.json");
        var resultsJson = JsonSerializer.Serialize(results.ToArray(), JsonContext.Default.TestResultArray);
        File.WriteAllText(resultsFile, resultsJson);
        Console.WriteLine($"\n✓ Final results: {resultsFile}");

        AppendMarkdownSummary(reportFile, results, totalStopwatch.Elapsed);
        Console.WriteLine($"✓ Report finalized: {reportFile}");

        return failed > 0 ? 1 : 0;
    }

    /// <summary>
    /// Processes a single prompt with retry logic and returns the test result.
    /// </summary>
    static async Task<TestResult> ProcessPromptAsync(
        AgentRunner runner,
        TestPrompt prompt,
        string namespaceName,
        string testContext,
        string model,
        int retries)
    {
        var stopwatch = Stopwatch.StartNew();
        var allAttemptTools = new List<List<string>>();
        var attempts = 0;

        Console.Write($"  [{prompt.Tool}] - {prompt.Prompt} ");

        for (var attempt = 1; attempt <= retries; attempt++)
        {
            attempts = attempt;
            using var cts = new CancellationTokenSource(PerAttemptTimeout);

            AgentMetadata metadata;
            try
            {
                metadata = await runner.RunAsync(new AgentRunConfig
                {
                    Prompt = prompt.Prompt,
                    ToolName = prompt.Tool,
                    Namespace = namespaceName,
                    SystemPrompt = string.IsNullOrWhiteSpace(testContext)
                        ? null
                        : new SystemPromptConfig { Mode = SystemPromptMode.Append, Content = testContext },
                    ShouldEarlyTerminate = md => AgentRunnerUtils.WasToolInvoked(md, prompt.Tool),
                    Model = model,
                }, cts.Token);
            }
            catch (Exception e)
            {
                Console.Write($"WARNING: Attempt {attempt} failed with error: {e.Message}");
                if (attempt < retries)
                {
                    continue;
                }
                // Final attempt failed
                Console.WriteLine($" X ERROR: {e.Message}");
                return new TestResult
                {
                    Tool = prompt.Tool,
                    Prompt = prompt.Prompt,
                    Duration = stopwatch.Elapsed.TotalSeconds,
                    Attempts = attempt,
                    Status = TestStatus.ERROR,
                    Error = e.Message
                };
            }

            var attemptTools = AgentRunnerUtils.GetInvokedToolNames(metadata);
            allAttemptTools.Add(attemptTools);

            if (AgentRunnerUtils.WasToolInvoked(metadata, prompt.Tool))
            {
                var toolsCalled = allAttemptTools.SelectMany(t => t).Distinct().ToArray();
                var retryIndicator = attempts > 1 ? $" (attempt {attempts})" : "";
                Console.WriteLine($" ✓ PASS{retryIndicator} [{stopwatch.Elapsed.TotalSeconds:F1}s]");
                return new TestResult
                {
                    Tool = prompt.Tool,
                    Prompt = prompt.Prompt,
                    Duration = stopwatch.Elapsed.TotalSeconds,
                    ToolsCalled = toolsCalled,
                    Attempts = attempts,
                    Status = TestStatus.PASS
                };
            }

            if (attempt < retries)
            {
                Console.Write($"RETRYING (attempt {attempt + 1})... ");
            }
        }

        // All retries exhausted without invoking the expected tool
        var allToolsCalled = allAttemptTools.SelectMany(t => t).Distinct().ToArray();
        Console.WriteLine($" X FAIL (tools: {string.Join(", ", allToolsCalled)})");
        return new TestResult
        {
            Tool = prompt.Tool,
            Prompt = prompt.Prompt,
            Duration = stopwatch.Elapsed.TotalSeconds,
            ToolsCalled = allToolsCalled,
            Attempts = attempts,
            Status = TestStatus.FAIL,
            Error = $"Expected tool not invoked. Called: [{string.Join(", ", allToolsCalled)}]"
        };
    }

    /// <summary>
    /// Initializes the live markdown report with header and table columns.
    /// </summary>
    static void InitializeMarkdownReport(string filePath)
    {
        using var writer = new StreamWriter(filePath, append: false);
        writer.WriteLine("# Azure MCP E2E Test Report");
        writer.WriteLine();
        writer.WriteLine($"**Date:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine();
        writer.WriteLine("## Results");
        writer.WriteLine();
        writer.WriteLine("| Status | Tool | Prompt | Duration | Attempts |");
        writer.WriteLine("|--------|------|--------|----------|----------|");
    }

    /// <summary>
    /// Appends a single test result row to the live markdown report.
    /// </summary>
    static void AppendResultToMarkdown(string filePath, TestResult result)
    {
        try
        {
            var status = result.Status == TestStatus.PASS ? "✓" : "✗";
            var promptShort = result.Prompt.Length > 40 ? result.Prompt[..40] + "..." : result.Prompt;
            var line = $"| {status} | `{result.Tool}` | {promptShort} | {result.Duration:F1}s | {result.Attempts} |";
            File.AppendAllText(filePath, line + Environment.NewLine);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"Warning: Failed to write to report: {ex.Message}");
        }
    }

    /// <summary>
    /// Appends summary and failed tests sections to the live markdown report.
    /// </summary>
    static void AppendMarkdownSummary(string filePath, List<TestResult> results, TimeSpan duration)
    {
        var passed = results.Count(r => r.Status == TestStatus.PASS);
        var failed = results.Count(r => r.Status != TestStatus.PASS);
        var passRate = results.Count > 0 ? (double)passed / results.Count * 100 : 0;

        using var writer = new StreamWriter(filePath, append: true);
        writer.WriteLine();
        writer.WriteLine("## Summary");
        writer.WriteLine();
        writer.WriteLine($"| Metric | Value |");
        writer.WriteLine($"|--------|-------|");
        writer.WriteLine($"| Total | {results.Count} |");
        writer.WriteLine($"| Passed | {passed} |");
        writer.WriteLine($"| Failed | {failed} |");
        writer.WriteLine($"| Pass Rate | {passRate:F1}% |");
        writer.WriteLine($"| Duration | {duration.TotalSeconds:F1}s |");
        writer.WriteLine();

        if (failed > 0)
        {
            writer.WriteLine("## Failed Tests");
            writer.WriteLine();
            writer.WriteLine("| Tool | Prompt | Tools Called |");
            writer.WriteLine("|------|--------|--------------|");
            foreach (var result in results.Where(r => r.Status != TestStatus.PASS))
            {
                var toolsCalled = result.ToolsCalled is not null ? string.Join(", ", result.ToolsCalled) : "";
                var promptShort = result.Prompt.Length > 50 ? result.Prompt[..50] + "..." : result.Prompt;
                writer.WriteLine($"| `{result.Tool}` | {promptShort} | {toolsCalled} |");
            }
        }
    }


    static (string? TestContextPath, string? PromptsPath) LoadFiles()
    {
        var root = FindRepoRoot();
        var context = Path.Combine(root, "eng", "tools", "CopilotCliTester", "src", "test-context.md");
        var prompts = Path.Combine(root, "servers", "Azure.Mcp.Server", "docs", "e2eTestPrompts.md");

        return (
            TestContextPath: File.Exists(context) ? context : null,
            PromptsPath: File.Exists(prompts) ? prompts : null
        );
    }

    static string FindRepoRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "Microsoft.Mcp.slnx")) ||
                File.Exists(Path.Combine(dir, "mcp.sln")))
            {
                return dir;
            }
            dir = Path.GetDirectoryName(dir);
        }
        return Directory.GetCurrentDirectory();
    }
}
