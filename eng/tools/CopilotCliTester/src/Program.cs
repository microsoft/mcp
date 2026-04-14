// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using CopilotCliTester.Models;

namespace CopilotCliTester;

/// <summary>
/// E2E Test Runner for Azure MCP tools using Copilot SDK. Runs prompts from e2eTestPrompts.md and verifies correct tools are invoked.
/// </summary>
static class Program
{
    private static readonly TimeSpan PerAttemptTimeout = TimeSpan.FromMinutes(5);

    private static readonly Lock _consoleLock = new();
    private static readonly Lock _reportLock = new();
    private static readonly string ServerProjectRelativePath =
        Path.Combine("servers", "Azure.Mcp.Server", "src", "Azure.Mcp.Server.csproj");
    private static readonly string runId = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
    static async Task<int> Main(string[] args)
    {
        var command = args.Length > 0 ? args[0].ToLowerInvariant() : "run";

        return command switch
        {
            "run" => await RunE2ETestsFromArgs(args.Skip(1).ToArray()),
            "--help" or "-h" => ShowHelp(),
            _ => await RunE2ETestsFromArgs(args)
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
              --output <dir>      Output directory for reports (default: results)
              --model <name>      Model to use (default: claude-opus-4.6)
              --parallel <n>      Number of prompts to test concurrently (default: 4)
              --threshold <n>       Pass threshold percentage (default: 95)
              --prompts-file <path>  Custom prompts file (markdown format)
              --list-namespaces     List all namespaces in prompts file
              --fail-on-empty-result   Consider tests that return no tool calls as failures (default: false)
            """);
        return 0;
    }

    static async Task<int> RunE2ETestsFromArgs(string[] args)
    {
        string? namespaceFilter = null, tool = null, outputDir = CopilotTestConstants.OutputDirectory, model = CopilotTestConstants.ModelName, promptsFile = null;
        int max = CopilotTestConstants.MaxPrompts, retries = CopilotTestConstants.MaxRetryAttempts, parallel = CopilotTestConstants.Parallel;
        bool onePerTool = false, listNamespaces = false, failOnEmptyResult = false;
        double threshold = CopilotTestConstants.PassThreshold;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--list-namespaces":
                    listNamespaces = true;
                    break;
                case "--namespace" when i + 1 < args.Length:
                    namespaceFilter = args[++i];
                    break;
                case "--tool" when i + 1 < args.Length:
                    tool = args[++i];
                    break;
                case "--max" when i + 1 < args.Length:
                    if (!int.TryParse(args[++i], out var parsedMax))
                    {
                        Console.WriteLine("Invalid value for --max. Using default: " + CopilotTestConstants.MaxPrompts);
                    }
                    else
                    {
                        max = parsedMax;
                    }
                    break;
                case "--retries" when i + 1 < args.Length:
                    if (!int.TryParse(args[++i], out var parsedRetries))
                    {
                        Console.WriteLine("Invalid value for --retries. Using default: " + CopilotTestConstants.MaxRetryAttempts);
                    }
                    else
                    {
                        retries = parsedRetries;
                    }
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
                case "--parallel" when i + 1 < args.Length:
                    if (!int.TryParse(args[++i], out var parsedParallel))
                    {
                        Console.WriteLine("Invalid value for --parallel. Using default: " + CopilotTestConstants.Parallel);
                    }
                    else
                    {
                        parallel = parsedParallel;
                    }
                    break;
                case "--threshold" when i+1 <args.Length:
                    if (!double.TryParse(args[++i], out var parsedThreshold))
                    {
                        Console.WriteLine("Invalid value for --threshold. Using default: " + CopilotTestConstants.PassThreshold);
                    }
                    else
                    {
                        threshold = parsedThreshold;
                    }
                    break;
                case "--prompts-file" when i + 1 < args.Length: 
                    promptsFile = args[++i];
                    break;
                case "--fail-on-empty-result":
                    failOnEmptyResult = true;
                    break;
            }
        }

        if (parallel < 1) {
            Console.WriteLine("Warning: --parallel must be >= 1. Using 1.");
            parallel = 1;
        } else if (parallel > CopilotTestConstants.MaxParallelAllowed) {
            Console.WriteLine($"Warning: --parallel must be <= {CopilotTestConstants.MaxParallelAllowed}. Using {CopilotTestConstants.MaxParallelAllowed}.");
            parallel = CopilotTestConstants.MaxParallelAllowed;
        }

        if (threshold < 0) {
            Console.WriteLine("Warning: --threshold must be >= 0. Using default: " + CopilotTestConstants.PassThreshold);
            threshold = CopilotTestConstants.PassThreshold;
        } else if (threshold > 100) {
            Console.WriteLine("Warning: --threshold must be <= 100. Using default: " + CopilotTestConstants.PassThreshold);
            threshold = CopilotTestConstants.PassThreshold;
        }

        if (listNamespaces)
        {
            var (_, defaultPromptsPath) = LoadFiles();
            var promptsPath = promptsFile ?? defaultPromptsPath;
            if (promptsPath is null || !File.Exists(promptsPath))
            {
                Console.Error.WriteLine($"ERROR: Prompts file not found: {promptsPath ?? "e2eTestPrompts.md"}");
                return 1;
            }
            var namespaces = PromptParser.ParseNamespaces(promptsPath);
            Console.WriteLine($"Available namespaces ({namespaces.Count}):");
            foreach (var ns in namespaces)
            {
                Console.WriteLine($"  - {ns}");
            }
            return 0;
        }

        return await RunE2ETests(namespaceFilter, tool, max, retries, onePerTool, outputDir, model, parallel, threshold, failOnEmptyResult, promptsFile);
    }

    static void CleanStaleWorkspaces()
    {
        try
        {
            var staleDirectoryDeleteThreshold = DateTimeOffset.UtcNow.AddMinutes(-20);
            var allDirectories = Directory.GetDirectories(Path.GetTempPath(), $"mcp-test-*")
                .Where(dir => !dir.Contains(runId) && Directory.GetCreationTimeUtc(dir) < staleDirectoryDeleteThreshold);
            var count = 0;
            foreach (var directory in allDirectories)
            {
                try
                {
                    Directory.Delete(directory, recursive: true);
                    count++;
                }
                catch
                {
                    // Best-effort cleanup
                }
            }
            if (count > 0)
            {
                Console.WriteLine($"Cleaned up {count} stale temp directories");
            }
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    static async Task<int> RunE2ETests(string? namespaceFilter, string? tool, int max, int retries, bool onePerTool, string outputDir, string model, int parallel, double threshold, bool failOnEmptyResult, string? promptsFile = null)
    {
        CleanStaleWorkspaces();

        Console.WriteLine("--------------------------------------------");
        Console.WriteLine("Azure MCP E2E Test Runner (Copilot SDK)");
        Console.WriteLine("--------------------------------------------");
        Console.WriteLine();

        var (testContextPath, defaultPromptsPath) = LoadFiles();

        // Load test context
        var testContext = testContextPath is not null ? File.ReadAllText(testContextPath).Trim() : "";
        if (!string.IsNullOrEmpty(testContext))
        {
            Console.WriteLine("SUCCESS: Loaded test context");
        }
        
        var promptsPath = promptsFile ?? defaultPromptsPath;
        if (promptsPath is null || !File.Exists(promptsPath))
        {
            Console.Error.WriteLine($"ERROR: Prompts file not found: {promptsPath ?? "e2eTestPrompts.md"}");
            return 1;
        }
        Console.WriteLine($"SUCCESS: Loading prompts from: {promptsPath}");
        var allPrompts = PromptParser.ParseFile(promptsPath);
        Console.WriteLine($"  Found {allPrompts.Count} total prompts");

        // Apply different filters
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
            if (failOnEmptyResult)
            {
                Console.WriteLine("No prompts matched the filter criteria. Considered as failure due to --fail-on-empty-result.");
                return 1;
            }
            else
            {
                Console.WriteLine("No prompts matched the filter criteria. Run with --list-namespaces to see available namespaces, or adjust your filters.");
                return 0;
            }
        }

        var namespaceCount = allPrompts.Select(p => p.Namespace).Distinct().Count();

        Console.WriteLine();
        Console.WriteLine($"Testing {allPrompts.Count} prompts across {namespaceCount} namespaces");
        Console.WriteLine($"Retries: {retries}, Model: {model}, Pass Threshold: {threshold}%");
        Console.WriteLine("--------------------------------------------------------------------------------");
        Console.WriteLine();

        var totalStopwatch = Stopwatch.StartNew();

        Directory.CreateDirectory(outputDir);
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        var title = !string.IsNullOrWhiteSpace(namespaceFilter) ? $"-{namespaceFilter}" : "";
        var reportFile = Path.Combine(outputDir, $"e2e-report{title}-{timestamp}.md");
        InitializeMarkdownReport(reportFile);
        Console.WriteLine($"Report: {reportFile}");
        Console.WriteLine($"Parallel workers: {parallel}");
        Console.WriteLine();

        var debug = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEBUG"));

        // Resolve the server executable once before launching parallel workers
        // to avoid N concurrent dotnet build races on cold start.
        var serverExecutablePath = await ResolveServerExecutable();

        using var semaphore = new SemaphoreSlim(parallel);

        var tasks = allPrompts.Select(async prompt =>
        {
            await semaphore.WaitAsync();
            try
            {
                // Create a dedicated client per task. When the client is disposed, it kills its CLI process tree, ensuring child azmcp.exe processes are cleaned up.
                var (client, workspacePath) = AgentRunner.CreateSharedClient(runId, debug, outputDir);
                await using var runner = new AgentRunner(client, serverExecutablePath, outputDir, workspacePath);
                var result = await ProcessPromptAsync(runner, prompt, prompt.Namespace, testContext, model, retries);
                AppendResultToMarkdown(reportFile, result);
                return result;
            }
            catch (Exception ex)
            {
                WriteLineLock($"ERROR: Task for tool '{prompt.Tool}' failed to initialize: {ex.Message}");
                var errorResult = new TestResult
                {
                    Tool = prompt.Tool,
                    Prompt = prompt.Prompt,
                    Duration = 0,
                    Status = TestStatus.ERROR,
                    Error = $"Task infrastructure error: {ex.Message}"
                };
                AppendResultToMarkdown(reportFile, errorResult);
                return errorResult;
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();
        
        var taskResults = await Task.WhenAll(tasks);
        var results = taskResults.OrderBy(r => r.Tool).ThenBy(r => r.Prompt).ToList();

        totalStopwatch.Stop();

        var passed = results.Count(r => r.Status == TestStatus.PASS);
        var failed = results.Count(r => r.Status == TestStatus.FAIL);
        var skipped = results.Count(r => r.Status == TestStatus.ERROR);
        var passRate = results.Count > 0 ? (double)passed / results.Count * 100 : 0;

        Console.WriteLine(new string('═', 64));
        Console.WriteLine("SUMMARY");
        Console.WriteLine(new string('─', 64));
        Console.WriteLine($"  Total:     {results.Count}");
        Console.WriteLine($"  Passed:    {passed}");
        Console.WriteLine($"  Failed:    {failed}");
        Console.WriteLine($"  Skipped:   {skipped}");
        Console.WriteLine($"  Pass Rate: {passRate:F1}%");
        Console.WriteLine($"  Duration:  {totalStopwatch.Elapsed.TotalSeconds:F1}s");
        Console.WriteLine(new string('═', 64));

        var resultsFile = Path.Combine(outputDir, $"e2e-results{title}-{timestamp}.json");
        var resultsJson = JsonSerializer.Serialize(results.ToArray(), JsonContext.Default.TestResultArray);
        File.WriteAllText(resultsFile, resultsJson);
        Console.WriteLine($"\n Final results: {resultsFile}");

        AppendMarkdownSummary(reportFile, results, totalStopwatch.Elapsed);
        Console.WriteLine($" Report finalized: {reportFile}");

        return passRate >= threshold ? 0 : 1;
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

        var toolTag = $"[{prompt.Tool}]";
        WriteLineLock($"  {toolTag} {prompt.Prompt}");

        for (var attempt = 1; attempt <= retries+1; attempt++)
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
                WriteLineLock($"  {toolTag} WARNING: Attempt {attempt} failed: {e.Message}");
                if (attempt <= retries)
                {
                    continue;
                }
                // Final attempt failed
                WriteLineLock($"  {toolTag} X ERROR: {e.Message}");
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
                WriteLineLock($"  {toolTag} ✓ PASS{retryIndicator} [{stopwatch.Elapsed.TotalSeconds:F1}s]");
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

            if (attempt <= retries)
            {
                WriteLineLock($"  {toolTag} RETRY (attempt {attempt + 1})...");
            }
        }

        // All retries exhausted without invoking the expected tool
        var allToolsCalled = allAttemptTools.SelectMany(t => t).Distinct().ToArray();
        WriteLineLock($"  {toolTag} ✗ FAIL (tools: {string.Join(", ", allToolsCalled)})");
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
        writer.WriteLine($"**Date:** {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}");
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
        lock (_reportLock)
        {
            try
            {
                var status = result.Status switch 
                {
                    TestStatus.PASS => "✓",
                    TestStatus.FAIL => "X",
                    TestStatus.ERROR => "--",
                    _ => "--"
                };
                var promptShort = result.Prompt.Length > 40 ? result.Prompt[..40] + "..." : result.Prompt;
                var line = $"| {status} | `{result.Tool}` | {promptShort} | {result.Duration:F1}s | {result.Attempts} |";
                File.AppendAllText(filePath, line + Environment.NewLine);
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"Warning: Failed to write to report: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Appends summary and failed tests sections to the live markdown report.
    /// </summary>
    static void AppendMarkdownSummary(string filePath, List<TestResult> results, TimeSpan duration)
    {
        var passed = results.Count(r => r.Status == TestStatus.PASS);
        var failed = results.Count(r => r.Status == TestStatus.FAIL);
        var skipped = results.Count(r => r.Status == TestStatus.ERROR);
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
        writer.WriteLine($"| Skipped | {skipped} |");
        writer.WriteLine($"| Pass Rate | {passRate:F1}% |");
        writer.WriteLine($"| Duration | {duration.TotalSeconds:F1}s |");
        writer.WriteLine();

        if (failed > 0)
        {
            writer.WriteLine("## Failed Tests");
            writer.WriteLine();
            writer.WriteLine("| Tool | Prompt | Tools Called |");
            writer.WriteLine("|------|--------|--------------|");
            foreach (var result in results.Where(r => r.Status == TestStatus.FAIL))
            {
                var toolsCalled = result.ToolsCalled is not null ? string.Join(", ", result.ToolsCalled) : "";
                var promptShort = result.Prompt.Length > 50 ? result.Prompt[..50] + "..." : result.Prompt;
                writer.WriteLine($"| `{result.Tool}` | {promptShort} | {toolsCalled} |");
            }
        }

        if (skipped > 0)
        {
            writer.WriteLine("## Skipped Tests");
            writer.WriteLine();
            writer.WriteLine("| Tool | Prompt | Error |");
            writer.WriteLine("|------|--------|-------|");
            foreach (var result in results.Where(r => r.Status == TestStatus.ERROR))
            {
                var promptShort = result.Prompt.Length > 50 ? result.Prompt[..50] + "..." : result.Prompt;
                var error = result.Error ?? "";
                writer.WriteLine($"| `{result.Tool}` | {promptShort} | {error} |");
            }
        }
    }

    static void WriteLineLock(string message)
    {
        lock(_consoleLock)
        {
            Console.WriteLine(message);
        }
    }

    static (string? TestContextPath, string? PromptsPath) LoadFiles()
    {
        string? root = null;
        try 
        {
            root = AgentRunnerUtils.FindRepoRoot(Directory.GetCurrentDirectory());
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"Warning: {ex.Message}");
            return (null, null);
        }
        var context = Path.Combine(root, "eng", "tools", "CopilotCliTester", "src", "test-context.md");
        var prompts = Path.Combine(root, "servers", "Azure.Mcp.Server", "docs", "e2eTestPrompts.md");

        return (
            TestContextPath: File.Exists(context) ? context : null,
            PromptsPath: File.Exists(prompts) ? prompts : null
        );
    }

    /// <summary>
    /// Resolves the azmcp server executable. Walks up from the current assembly location to find the repo root, then checks for an existing
    /// build artifact. If none is found, builds the server project.
    /// </summary>
    private static async Task<string> ResolveServerExecutable()
    {
        var repoRoot = AgentRunnerUtils.FindRepoRoot(AppContext.BaseDirectory);
        var serverProject = Path.Combine(repoRoot, ServerProjectRelativePath);

        if (!File.Exists(serverProject))
        {
            throw new InvalidOperationException(
                $"Azure MCP Server project not found at '{serverProject}'. " +
                "Make sure you're running from within the repo.");
        }

        // Check for an existing Debug build output
        var file = FindBuildExecutable(repoRoot);

        if (File.Exists(file))
        {
            Debug.WriteLine($"Using existing server build: {file}");
            return file;
        }

        // If no pre-built artifact found, build the server project
        Console.WriteLine("No pre-built azmcp found. Building Azure.Mcp.Server...");
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            ArgumentList = { "build", serverProject },
            RedirectStandardOutput = false,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start 'dotnet build'.");

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(15));
        
        var stderrTask = process.StandardError.ReadToEndAsync();

        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree:  true);
            }
            throw new TimeoutException(
                $"'dotnet build' timed out after 15 minutes. The build process has been terminated.");
        }

        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"'dotnet build' failed (exit code {process.ExitCode}).\n{stderr}");
        }

        // After build, the Debug output should exist
        var builtPath = FindBuildExecutable(repoRoot);

        if (!File.Exists(builtPath))
        {
            throw new InvalidOperationException(
                $"Build succeeded but server executable not found at '{builtPath}'.");
        }

        Console.WriteLine($"Build complete: {builtPath}");
        return builtPath;
    }

    private static readonly string[] configurations = ["Debug", "Release"];

    private static string? FindBuildExecutable(string repoRoot)
    {
        var exeName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";
        var binDirectory = Path.Combine(repoRoot, "servers", "Azure.Mcp.Server", "src", "bin");

        // Check well-known output paths (bin/{Debug,Release}/*/) and pick the most recent
        var knownPath = configurations
            .Select(config => Path.Combine(binDirectory, config))
            .Where(Directory.Exists)
            .SelectMany(configDir => Directory.GetFiles(configDir, exeName, SearchOption.AllDirectories))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

        if (knownPath is not null)
        {
            Console.WriteLine($"  Found server executable: {knownPath}");
            return knownPath;
        }

        // Fallback: search bin/ subdirectories outside the known configurations.
        if (!Directory.Exists(binDirectory))
        {
            return null;
        }

        var knownConfigDirs = configurations
            .Select(config => Path.Combine(binDirectory, config))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var fallback = Directory.GetDirectories(binDirectory)
            .Where(dir => !knownConfigDirs.Contains(dir))
            .SelectMany(dir => Directory.GetFiles(dir, exeName, SearchOption.AllDirectories))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

        if (fallback is not null)
        {
            Console.WriteLine($"  WARNING: Known config builds not found. Using fallback: {fallback}");
        }

        return fallback;
    }
}
