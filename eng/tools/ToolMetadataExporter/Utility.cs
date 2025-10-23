// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using ToolSelection.Models;

namespace ToolMetadataExporter;

internal class Utility
{
    internal static async Task<ListToolsResult?> LoadToolsDynamicallyAsync(string workDirectory, bool isCiMode = false)
    {
        try
        {
            var output = await ExecuteAzmcpAsync("tools list", isCiMode);
            // Filter out non-JSON lines (like launch settings messages)
            var lines = output.Split('\n');
            var jsonStartIndex = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim().StartsWith("{"))
                {
                    jsonStartIndex = i;

                    break;
                }
            }

            if (jsonStartIndex == -1)
            {
                if (isCiMode)
                {
                    return null; // Graceful fallback in CI
                }

                throw new InvalidOperationException("No JSON output found from azmcp command.");
            }

            var jsonOutput = string.Join('\n', lines.Skip(jsonStartIndex));

            // Parse the JSON output
            var result = JsonSerializer.Deserialize(jsonOutput, SourceGenerationContext.Default.ListToolsResult);

            // Save the dynamically loaded tools to tools.json for future use
            if (result != null)
            {
                await SaveToolsToJsonAsync(result, Path.Combine(workDirectory, "tools.json"));

                Console.WriteLine($"💾 Saved {result.Tools?.Count} tools to tools.json");
            }

            return result;
        }
        catch (Exception)
        {
            if (isCiMode)
            {
                return null; // Graceful fallback in CI
            }

            throw;
        }
    }

    internal static async Task<string> GetVersionAsync()
    {
        return await ExecuteAzmcpAsync("version");
    }

    internal static async Task<string> ExecuteAzmcpAsync(string arguments, bool isCiMode = false)
    {
        // Locate azmcp artifact across common build outputs (servers/core, Debug/Release)
        var exeDir = AppContext.BaseDirectory;
        var repoRoot = FindRepoRoot(exeDir);
        var searchRoots = new List<string>
            {
                Path.Combine(repoRoot, "servers", "Azure.Mcp.Server", "src", "bin", "Debug"),
                Path.Combine(repoRoot, "servers", "Azure.Mcp.Server", "src", "bin", "Release")
            };

        var candidateNames = new[] { "azmcp.exe", "azmcp", "azmcp.dll" };
        FileInfo? cliArtifact = null;

        foreach (var root in searchRoots.Where(Directory.Exists))
        {
            foreach (var name in candidateNames)
            {
                var found = new DirectoryInfo(root)
                    .EnumerateFiles(name, SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (found != null)
                {
                    cliArtifact = found;
                    break;
                }
            }

            if (cliArtifact != null)
            {
                break;
            }
        }

        if (cliArtifact == null)
        {
            if (isCiMode)
            {
                return string.Empty; // Graceful fallback in CI
            }

            throw new FileNotFoundException("Could not locate azmcp CLI artifact in Debug/Release outputs under servers.");
        }

        var isDll = string.Equals(cliArtifact.Extension, ".dll", StringComparison.OrdinalIgnoreCase);
        var fileName = isDll ? "dotnet" : cliArtifact.FullName;
        var argumentsToUse = isDll ? $"{cliArtifact.FullName} " : "tools list";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = argumentsToUse,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            if (isCiMode)
            {
                return string.Empty; // Graceful fallback in CI
            }

            throw new InvalidOperationException($"Failed to execute operation '{arguments}' from azmcp: {error}");
        }

        return output;
    }

    private static async Task<ListToolsResult?> LoadToolsFromJsonAsync(string filePath, bool isCiMode = false)
    {
        if (!File.Exists(filePath))
        {
            if (isCiMode)
            {
                return null; // Let caller handle this gracefully
            }

            throw new FileNotFoundException($"Tools file not found: {filePath}");
        }

        var json = await File.ReadAllTextAsync(filePath);

        // Process the JSON
        if (json.StartsWith('\'') && json.EndsWith('\''))
        {
            json = json[1..^1]; // Remove first and last characters (quotes)
            json = json.Replace("\\'", "'"); // Convert \' --> '
            json = json.Replace("\\\\\"", "'"); // Convert \\" --> '
        }

        var result = JsonSerializer.Deserialize(json, SourceGenerationContext.Default.ListToolsResult);

        return result;
    }


    private static async Task SaveToolsToJsonAsync(ListToolsResult toolsResult, string filePath)
    {
        try
        {
            // Normalize only tool and option descriptions instead of escaping the entire JSON document
            if (toolsResult.Tools != null)
            {
                foreach (var tool in toolsResult.Tools)
                {
                    if (!string.IsNullOrEmpty(tool.Description))
                    {
                        tool.Description = EscapeCharacters(tool.Description);
                    }

                    if (tool.Options != null)
                    {
                        foreach (var opt in tool.Options)
                        {
                            if (!string.IsNullOrEmpty(opt.Description))
                            {
                                opt.Description = EscapeCharacters(opt.Description);
                            }
                        }
                    }
                }
            }

            var writerOptions = new JsonWriterOptions
            {
                Indented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            using var stream = new MemoryStream();
            using (var jsonWriter = new Utf8JsonWriter(stream, writerOptions))
            {
                JsonSerializer.Serialize(jsonWriter, toolsResult, SourceGenerationContext.Default.ListToolsResult);
            }

            await File.WriteAllBytesAsync(filePath, stream.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Warning: Failed to save tools to {filePath}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static string EscapeCharacters(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Normalize only the fancy “curly” quotes to straight ASCII. Identity replacements were removed.
        return text.Replace(UnicodeChars.LeftSingleQuote, "'")
               .Replace(UnicodeChars.RightSingleQuote, "'")
               .Replace(UnicodeChars.LeftDoubleQuote, "\"")
               .Replace(UnicodeChars.RightDoubleQuote, "\"");
    }

    // Traverse up from a starting directory to find the repo root (containing AzureMcp.sln or .git)
    internal static string FindRepoRoot(string startDir)
    {
        var dir = new DirectoryInfo(startDir);

        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "AzureMcp.sln")) ||
                Directory.Exists(Path.Combine(dir.FullName, ".git")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not find repo root (AzureMcp.sln or .git).");
    }

    internal static class UnicodeChars
    {
        public const string LeftSingleQuote = "\u2018";
        public const string RightSingleQuote = "\u2019";
        public const string LeftDoubleQuote = "\u201C";
        public const string RightDoubleQuote = "\u201D";
    }
}
