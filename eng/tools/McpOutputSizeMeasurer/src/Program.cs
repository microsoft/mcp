// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using McpToolEvaluator.Core;

namespace McpOutputSizeMeasurer;

/// <summary>
/// Standalone console application that measures the server's exposed MCP tool surfaces.
/// Starts the azmcp executable over stdio in both
/// consolidated and namespace modes and measures negotiated server metadata, tools/list
/// discovery, and learn-mode payload sizes (including inner commands) for every tool,
/// writing a JSON report that Measure-McpOutputSizes.ps1 summarizes.
/// </summary>
internal static class Program
{
    private static readonly string[] DefaultModes = ["consolidated", "namespace"];

    private static async Task<int> Main(string[] args)
    {
        if (args.Contains("--help") || args.Contains("-h"))
        {
            ShowHelp();
            return 0;
        }

        string? executablePath = null;
        string? reportPath = null;
        var modes = new List<string>();
        var verbose = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--executable":
                case "--exe":
                    executablePath = RequireValue(args, ref i, "--executable");
                    break;
                case "--report":
                    reportPath = RequireValue(args, ref i, "--report");
                    break;
                case "--mode":
                    modes.Add(RequireValue(args, ref i, "--mode"));
                    break;
                case "--verbose":
                    verbose = true;
                    break;
                default:
                    Console.Error.WriteLine($"Unknown argument: {args[i]}");
                    return -1;
            }
        }

        if (modes.Count == 0)
        {
            modes.AddRange(DefaultModes);
        }

        var repoRoot = Utilities.FindRepoRoot(AppContext.BaseDirectory);

        if (string.IsNullOrEmpty(executablePath))
        {
            var executableName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";
            executablePath = Path.Combine(
                repoRoot, "servers", "Azure.Mcp.Server", "src", "bin", "Debug", "net10.0", executableName);
        }

        if (!File.Exists(executablePath))
        {
            Console.Error.WriteLine(
                $"Executable not found at {executablePath}. Build the Azure.Mcp.Server project first, or pass --executable <path>.");
            return -1;
        }

        reportPath ??= Path.Combine(repoRoot, "TestResults", "mcp-output-size.json");
        reportPath = Path.GetFullPath(reportPath);
        var reportDirectory = Path.GetDirectoryName(reportPath)!;
        Directory.CreateDirectory(reportDirectory);

        var logger = verbose ? (Action<string>)Console.Error.WriteLine : null;
        var measurer = new McpOutputSizeMeasurer(logger);

        object report;
        try
        {
            report = await measurer.MeasureAsync(executablePath, modes, reportDirectory, reportPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Measurement failed: {ex.Message}");
            return -1;
        }

        var reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(reportPath, reportJson, Encoding.UTF8);

        Console.WriteLine(reportJson);
        Console.WriteLine($"Measurement report saved to {reportPath}");

        return 0;
    }

    private static string RequireValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {optionName}.");
        }

        index++;
        return args[index];
    }

    private static void ShowHelp()
    {
        Console.WriteLine("""
            MCP Output Size Measurer

            Measures the server's exposed MCP tool surfaces: negotiated server metadata,
            tools/list discovery, decoded learn-mode content and command JSON, and serialized
            learn response payloads (including inner commands) for every tool.

            Usage:
              McpOutputSizeMeasurer [options]

            Options:
              --executable <path>  Path to the azmcp(.exe) executable to measure.
                                   Defaults to servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp(.exe).
              --report <path>      Path to write the JSON measurement report.
                                   Defaults to TestResults/mcp-output-size.json.
              --mode <name>        Server mode to measure (repeatable). Defaults to
                                   "consolidated" and "namespace".
              --verbose            Log MCP server stderr output.
              --help, -h           Show this help.
            """);
    }
}
