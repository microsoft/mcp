// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using ToolSelection.Models;
using ToolSelection.Services;
using ToolSelection.VectorDb;

namespace ToolSelection;

class Program
{
    private static readonly HttpClient HttpClient = new();

    private const string SpaceReplacement = "_";
    private const string TestToolIdPrefix = $"test{SpaceReplacement}tool{SpaceReplacement}";
    private static readonly IReadOnlyDictionary<string, string> ValidServers =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Azure"] = "azmcp",
            ["Fabric"] = "fabmcp"
        };

    static async Task Main(string[] args)
    {
        var stopwatchTotal = Stopwatch.StartNew();

        // Check if we're in CI mode (skip if credentials are missing)
        var isCiMode = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BUILD_BUILDID")) ||
                       !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
                       args.Contains("--ci");

        // Show help if requested
        if (args.Contains("--help") || args.Contains("-h"))
        {
            ShowHelp();

            return;
        }

        int maxResultsPerTest = 5; // Default maximum number of results to show per test
        string? customToolsFile = null; // Optional custom tools file
        string? customPromptsFile = null; // Optional custom prompts file
        string? customOutputFileName = null; // Optional custom output file name
        string? areaFilter = null; // Optional area filter for prompts
        string? serverName = null; // Optional server name. Defaults to "Azure".
        string? serverExePath = null; // Optional server executable path. Supercedes server name if provided.

        // Single tool test mode options
        bool testSingleToolMode = false; // Optional mode to test a single tool description
        string? testToolDescription = null; // Optional tool description for single tool testing
        var testPrompts = new List<string>(); // Optional prompts for single tool testing

        // Parse command-line arguments
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--top" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out var parsed) && parsed > 0)
                {
                    maxResultsPerTest = parsed;
                }
                else
                {
                    Console.WriteLine("⚠️  Ignoring --top value (must be a positive integer). Using default: 5.");
                }
            }
            else if (args[i] == "--tools-file" && i + 1 < args.Length)
            {
                customToolsFile = args[i + 1];

                if (!File.Exists(customToolsFile))
                {
                    throw new FileNotFoundException($"The specified tools file does not exist: {customToolsFile}");
                }
            }
            else if (args[i] == "--prompts-file" && i + 1 < args.Length)
            {
                customPromptsFile = args[i + 1];

                if (!customPromptsFile.EndsWith(".md", StringComparison.OrdinalIgnoreCase) && !customPromptsFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Custom prompts file must be either a .md or .json file.");
                }

                if (!File.Exists(customPromptsFile))
                {
                    throw new FileNotFoundException($"The specified prompts file does not exist: {customPromptsFile}");
                }
            }
            else if (args[i] == "--output-file-name" && i + 1 < args.Length)
            {
                customOutputFileName = args[i + 1];

                if (string.IsNullOrWhiteSpace(customOutputFileName))
                {
                    throw new ArgumentException("The --output-file-name argument cannot be empty.");
                }
            }
            else if (args[i] == "--area" && i + 1 < args.Length)
            {
                areaFilter = args[i + 1];

                if (string.IsNullOrWhiteSpace(areaFilter))
                {
                    throw new ArgumentException("The --area argument cannot be empty.");
                }
            }
            else if (args[i] == "--test-single-tool")
            {
                testSingleToolMode = args.Contains("--test-single-tool");
            }
            else if (args[i] == "--tool-description" && i + 1 < args.Length)
            {
                testToolDescription = args[i + 1];

                if (string.IsNullOrWhiteSpace(testToolDescription))
                {
                    throw new ArgumentException("The --tool-description argument cannot be empty.");
                }
            }
            else if (args[i] == "--prompt" && i + 1 < args.Length)
            {
                testPrompts.Add(args[i + 1]);

                if (string.IsNullOrWhiteSpace(args[i + 1]))
                {
                    throw new ArgumentException("A --prompt argument cannot be empty.");
                }
            }
            else if (args[i] == "--server" && i + 1 < args.Length)
            {
                serverName = args[i + 1];

                if (!ValidServers.ContainsKey(serverName))
                {
                    throw new ArgumentException($"Invalid server name: {serverName}. Allowed values are {string.Join(", ", ValidServers.Select(kvp => $"'{kvp.Key}'"))} (case-insensitive)."); 
                }
            } else if (args[i] == "--server-exe" && i + 1 < args.Length)
            {
                serverExePath = args[i + 1];

                if (!File.Exists(serverExePath))
                {
                    throw new FileNotFoundException($"The specified server executable path does not exist: {serverExePath}");
                }
            }
        }

        try
        {
            if (testSingleToolMode)
            {
                WarnArgumentsAreIgnored("--test-single-tool", new Dictionary<string, string?>
                {
                    {"--area", areaFilter},
                    {"--server", serverName},
                    {"--server-exe", serverExePath},
                    {"--prompts-file", customPromptsFile},
                    {"--output-file-name", customOutputFileName}
                });

                if (string.IsNullOrEmpty(testToolDescription))
                {
                    throw new ArgumentException("--test-single-tool mode requires exactly one --tool-description argument.");
                }

                if (testPrompts.Count == 0)
                {
                    throw new ArgumentException("--test-single-tool mode requires at least one --prompt argument.");
                }
            }
            else if (!string.IsNullOrEmpty(testToolDescription) || testPrompts.Count > 0)
            {
                throw new ArgumentException("--tool-description and --prompt arguments are only valid in --test-single-tool mode.");
            }

            serverName ??= "Azure";
            string exeDir = AppContext.BaseDirectory;
            string repoRoot = FindRepoRoot(exeDir);
            string toolDir = FindToolDir(repoRoot, exeDir);

            // Load environment variables from .env file if it exists
            await LoadDotEnvFile(toolDir);

            // Get configuration values
            var endpoint = Environment.GetEnvironmentVariable("AOAI_ENDPOINT");

            if (string.IsNullOrEmpty(endpoint))
            {
                string errorMessage;

                if (isCiMode)
                {
                    errorMessage = "AOAI_ENDPOINT not configured";
                }
                else
                {
                    errorMessage = "AOAI_ENDPOINT environment variable not set. Please set it to an Azure OpenAI endpoint URL.";
                }

                throw new ArgumentException(errorMessage);
            }

            var apiKey = Environment.GetEnvironmentVariable("TEXT_EMBEDDING_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                string errorMessage;

                if (isCiMode)
                {
                    errorMessage = "TEXT_EMBEDDING_API_KEY not available";
                }
                else
                {
                    errorMessage = "TEXT_EMBEDDING_API_KEY environment variable not set. Please set it to an Azure OpenAI API key.";
                }

                throw new ArgumentException(errorMessage);
            }

            var embeddingService = new EmbeddingService(HttpClient, endpoint, apiKey!);

            // Load tools - use custom JSON file if specified, otherwise try dynamic loading with fallback
            ListToolsResult? listToolsResult = null;

            string? customToolsFileResolved = !string.IsNullOrEmpty(customToolsFile) && !Path.IsPathRooted(customToolsFile)
                ? Path.Combine(toolDir, customToolsFile)
                : customToolsFile;

            if (!string.IsNullOrEmpty(customToolsFileResolved))
            {
                listToolsResult = await LoadToolsFromJsonAsync(customToolsFileResolved);

                if (listToolsResult == null)
                {
                    Console.WriteLine($"⚠️  Failed to load tools from {customToolsFileResolved}, falling back to dynamic loading");
                    listToolsResult = await LoadToolsDynamicallyAsync(toolDir, serverName, serverExePath);
                }
            }
            else
            {
                listToolsResult = await LoadToolsDynamicallyAsync(toolDir, serverName, serverExePath) ?? await LoadToolsFromJsonAsync(Path.Combine(toolDir, "tools.json"));
            }

            var tools = listToolsResult?.Tools ?? listToolsResult?.ConsolidatedTools;

            if (tools == null || tools.Count == 0)
            {
                throw new InvalidDataException("No tools found for processing.");
            }

            // Create vector database
            var db = new VectorDB(new CosineSimilarity());
            var stopwatch = Stopwatch.StartNew();

            await PopulateDatabaseAsync(db, tools, embeddingService);

            stopwatch.Stop();

            var toolCount = db.Count;
            var executionTime = stopwatch.Elapsed;

            // Check if output should use text format
            var isTextOutput = IsTextOutput();
            var outputFileName = "results";

            if (!string.IsNullOrWhiteSpace(customOutputFileName))
            {
                outputFileName = Path.GetFileNameWithoutExtension(customOutputFileName);
            }

            outputFileName += isTextOutput ? ".txt" : ".md";

            // Determine output file path
            var outputFilePath = Path.Combine(toolDir, outputFileName);

            // Add console output
            Console.WriteLine("🔍 Running tool selection analysis...");
            Console.WriteLine($"✅ Loaded {toolCount} tools in {executionTime.TotalSeconds:F2}s");

            if (!string.IsNullOrEmpty(customToolsFile))
            {
                Console.WriteLine($"📄 Using custom tools file: {customToolsFile}");
            }
            else
            {
                Console.WriteLine($"🔄 Loading tools dynamically from {serverName}.Mcp.Server executable");
            }

            if (!string.IsNullOrEmpty(customPromptsFile))
            {
                Console.WriteLine($"📝 Using custom prompts file: {customPromptsFile}");
            }
            else
            {
                Console.WriteLine($"📝 Using default prompts file (e2eTestPrompts.md) for {serverName}.Mcp.Server");
            }

            if (testSingleToolMode)
            {
                await TestSingleToolAsync(testToolDescription, testPrompts, embeddingService, db, maxResultsPerTest);

                return;
            }
            else if (!string.IsNullOrEmpty(areaFilter))
            {
                var areaCount = areaFilter.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
                var areaLabel = areaCount > 1 ? "areas" : "area";

                Console.WriteLine($"🎯 Filtering prompts to {areaLabel}: {areaFilter}");
            }

            // Create or overwrite the output file
            using var writer = new StreamWriter(outputFilePath, false);

            if (!isTextOutput)
            {
                await writer.WriteLineAsync("# Tool Selection Analysis Setup");
                await writer.WriteLineAsync();
                await writer.WriteLineAsync($"**Setup completed:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}  ");
                await writer.WriteLineAsync($"**Tool count:** {toolCount}  ");
                await writer.WriteLineAsync($"**Database setup time:** {executionTime.TotalSeconds:F7}s  ");
                await writer.WriteLineAsync();
                await writer.WriteLineAsync("---");
                await writer.WriteLineAsync();
            }

            // Load prompts from custom file, markdown file, or JSON file as fallback
            Dictionary<string, List<string>>? toolNameAndPrompts = null;

            string? customPromptsFileResolved = !string.IsNullOrEmpty(customPromptsFile) && !Path.IsPathRooted(customPromptsFile)
                ? Path.Combine(toolDir, customPromptsFile)
                : customPromptsFile;

            if (!string.IsNullOrEmpty(customPromptsFileResolved))
            {
                // User specified a custom prompts file
                if (customPromptsFileResolved.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                {
                    toolNameAndPrompts = await LoadPromptsFromMarkdownAsync(customPromptsFileResolved, areaFilter);
                }
                else if (customPromptsFileResolved.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    toolNameAndPrompts = await LoadPromptsFromJsonAsync(customPromptsFileResolved, areaFilter);
                }
            }
            else
            {
                // Use default fallback logic
                Console.WriteLine($"⚠️  No custom prompts file specified; using fallback prompts file for {serverName}.Mcp.Server");

                var defaultPromptsPath = Path.Combine(repoRoot, "servers", $"{serverName}.Mcp.Server", "docs", "e2eTestPrompts.md");
                var promptsJsonPath = Path.Combine(toolDir, "prompts.json");

                if (File.Exists(defaultPromptsPath))
                {
                    // Load from markdown and save a normalized JSON copy for future runs
                    toolNameAndPrompts = await LoadPromptsFromMarkdownAsync(defaultPromptsPath, areaFilter);

                    if (toolNameAndPrompts != null)
                    {
                        await SavePromptsToJsonAsync(toolNameAndPrompts, promptsJsonPath);

                        Console.WriteLine($"💾 Saved prompts to prompts.json");
                    }
                    else
                    {
                        // If parsing returned no prompts, try to fall back to a previously saved prompts.json
                        if (File.Exists(promptsJsonPath))
                        {
                            Console.WriteLine($"⚠️  No prompts parsed from {defaultPromptsPath}; falling back to prompts.json at {promptsJsonPath}");
                            toolNameAndPrompts = await LoadPromptsFromJsonAsync(promptsJsonPath, areaFilter);
                        }
                    }
                }
                else
                {
                    // Default markdown not present — try prompts.json before failing
                    if (File.Exists(promptsJsonPath))
                    {
                        Console.WriteLine($"⚠️  Default prompts markdown not found at {defaultPromptsPath}; falling back to prompts.json at {promptsJsonPath}");
                        toolNameAndPrompts = await LoadPromptsFromJsonAsync(promptsJsonPath, areaFilter);
                    }
                }

                if (toolNameAndPrompts == null)
                {
                    throw new InvalidDataException("No prompts found for processing.");
                }
            }

            await PerformAnalysis(toolNameAndPrompts!, embeddingService, db, executionTime, writer, maxResultsPerTest, isCiMode);

            stopwatchTotal.Stop();

            // Print summary to console for immediate feedback
            Console.WriteLine($"🎯 Tool selection analysis completed");
            Console.WriteLine($"📊 Results written to: {Path.GetFullPath(outputFilePath)}");
            Console.WriteLine($"⏱️  Total execution time: {stopwatchTotal.Elapsed.TotalSeconds:F7}s");
        }
        catch (Exception ex)
        {
            if (isCiMode)
            {
                Console.WriteLine($"⚠️  Error occurred while running in CI: {ex.Message}");
                Console.WriteLine("⏭️  Skipping tool selection analysis");
                Environment.Exit(0);
            }

            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }

    private static bool IsTextOutput()
    {
        var args = Environment.GetCommandLineArgs();

        return args.Contains("--text-results", StringComparer.OrdinalIgnoreCase);
    }

    private static void WarnArgumentsAreIgnored(string prioritizedArgument, Dictionary<string, string?> ignoredArguments)
    {
        var argumentsWithValues = ignoredArguments
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
            .Select(kvp => kvp.Key)
            .ToList();

        if (argumentsWithValues.Count > 0)
        {
            Console.WriteLine($"⚠️  Ignoring argument{(argumentsWithValues.Count > 1 ? "s" : "")} {string.Join(", ", argumentsWithValues)} when using {prioritizedArgument}");
        }
    }

    private static async Task LoadDotEnvFile(string toolDir)
    {
        var envFilePath = Path.Combine(toolDir, ".env");

        if (!File.Exists(envFilePath))
        {
            Console.WriteLine("No .env file found or error loading it");
            return;
        }

        var lines = await File.ReadAllLinesAsync(envFilePath);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            var parts = line.Split('=', 2);

            if (parts.Length == 2)
            {
                Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
            }
        }
    }

    // Traverse up from a starting directory to find the repo root
    private static string FindRepoRoot(string startDir)
    {
        var dir = new DirectoryInfo(startDir);

        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        throw new FileNotFoundException("Could not find repo root.");
    }

    // Resolve the ToolDescriptionEvaluator directory robustly from repo root, with fallbacks from exeDir
    private static string FindToolDir(string repoRoot, string exeDir)
    {
        var candidate = Path.Combine(repoRoot, "eng", "tools", "ToolDescriptionEvaluator");
        if (Directory.Exists(candidate))
        {
            return candidate;
        }

        // Fallback: traverse up from the executable directory looking for the project file
        var dir = new DirectoryInfo(exeDir);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "ToolDescriptionEvaluator.csproj")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        // Last resort: previous relative approach (bin/... -> project folder)
        return Path.GetFullPath(Path.Combine(exeDir, "..", "..", ".."));
    }

    private static async Task<ListToolsResult?> LoadToolsDynamicallyAsync(string toolDir, string server, string? serverExePath)
    {
        // Locate mcp server artifact across common build outputs (servers/core, Debug/Release)
        var exeDir = AppContext.BaseDirectory;
        var repoRoot = FindRepoRoot(exeDir);
        var searchRoots = new List<string>
        {
            Path.Combine(repoRoot, "servers", server + ".Mcp.Server", "src", "bin", "Debug"),
            Path.Combine(repoRoot, "servers", server + ".Mcp.Server", "src", "bin", "Release")
        };

        FileInfo? cliArtifact = null;
        if (!string.IsNullOrEmpty(serverExePath))
        {
            cliArtifact = new FileInfo(serverExePath);
        }
        else
        {
            string serverExe = ValidServers[server];
            string[] candidateNames = new[] { ".exe", "", ".dll" };

            foreach (var root in searchRoots.Where(Directory.Exists))
            {
                foreach (var name in candidateNames)
                {
                    FileInfo? found = new DirectoryInfo(root)
                        .EnumerateFiles(serverExe + name, SearchOption.AllDirectories)
                        .FirstOrDefault();
                    if (found != null)
                    {
                        cliArtifact = found;
                        break;
                    }
                }

                if (cliArtifact != null)
                {
                    Console.WriteLine($"Found {serverExe} CLI artifact: {cliArtifact.FullName}");
                    break;
                }
            }

            if (cliArtifact == null)
            {
                throw new FileNotFoundException($"Could not locate {serverExe} CLI artifact in Debug/Release outputs under servers.");
            }
        }

        var isDll = string.Equals(cliArtifact.Extension, ".dll", StringComparison.OrdinalIgnoreCase);
        var fileName = isDll ? "dotnet" : cliArtifact.FullName;
        var arguments = isDll ? $"{cliArtifact.FullName} tools list" : "tools list";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
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
            throw new IOException($"Failed to get tools from {fileName}: {error}");
        }

        // Parse the JSON output
        var result = JsonSerializer.Deserialize(output, SourceGenerationContext.Default.ListToolsResult);

        // Save the dynamically loaded tools to tools.json for future use
        if (result != null)
        {
            await SaveToolsToJsonAsync(result, Path.Combine(toolDir, "tools.json"));

            Console.WriteLine($"💾 Saved {result.Tools?.Count} tools to tools.json");
        }

        return result;
    }

    private static async Task<ListToolsResult?> LoadToolsFromJsonAsync(string filePath)
    {
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
            Console.WriteLine($"⚠️  Failed to save tools to {filePath}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private static async Task<Dictionary<string, List<string>>?> LoadPromptsFromMarkdownAsync(string filePath, string? areaFilter = null)
    {
        var content = await File.ReadAllTextAsync(filePath);
        var prompts = new Dictionary<string, List<string>>();

        // Parse markdown tables to extract tool names and prompts
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip headers, separators, and non-table content
            if (trimmedLine.StartsWith("| Tool Name") ||
                trimmedLine.StartsWith("|:-------") ||
                trimmedLine.StartsWith("#") ||
                string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }

            // Parse table rows. For example: | tool_name | Test prompt |
            if (trimmedLine.StartsWith("|") && trimmedLine.Contains("|"))
            {
                var parts = trimmedLine.Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var toolName = parts[0].Trim();
                    var prompt = parts[1].Trim();

                    // Skip empty entries
                    if (string.IsNullOrWhiteSpace(toolName) || string.IsNullOrWhiteSpace(prompt))
                        continue;

                    // Filter by tool name prefix(es) (e.g., keyvault, storage)
                    if (!string.IsNullOrEmpty(areaFilter))
                    {
                        // Support multiple areas separated by commas
                        var areas = areaFilter.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                .Select(a => a.Trim())
                                                .Where(a => !string.IsNullOrEmpty(a))
                                                .ToList();

                        // Check if tool name starts with any of the area filters
                        if (!areas.Any(prefix => toolName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                        {
                            // Skip this tool as it doesn't match any prefix
                            continue;
                        }
                    }

                    if (!prompts.ContainsKey(toolName))
                    {
                        prompts[toolName] = new List<string>();
                    }

                    prompts[toolName].Add(prompt.Replace("\\<", "<"));
                }
            }
        }

        // If area filter was specified but no prompts found, provide feedback
        if (!string.IsNullOrEmpty(areaFilter) && prompts.Count == 0)
        {
            Console.WriteLine($"⚠️  No prompts found for prefix '{areaFilter}'. Use service names like 'keyvault', 'storage', 'functionapp', etc.");
        }

        return prompts.Count > 0 ? prompts : null;
    }

    private static async Task<Dictionary<string, List<string>>?> LoadPromptsFromJsonAsync(string filePath, string? areaFilter = null)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var prompts = JsonSerializer.Deserialize(json, SourceGenerationContext.Default.DictionaryStringListString);

        if (!string.IsNullOrEmpty(areaFilter))
        {
            // Filter prompts by area
            prompts = prompts?.Where(kvp => kvp.Key.StartsWith(areaFilter, StringComparison.OrdinalIgnoreCase))
                                   .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        return prompts;
    }

    private static async Task SavePromptsToJsonAsync(Dictionary<string, List<string>> prompts, string filePath)
    {
        try
        {
            // Escape only the prompt VALUES (not the keys or overall JSON structure)
            foreach (var kvp in prompts.ToList())
            {
                var list = kvp.Value;
                for (int i = 0; i < list.Count; i++)
                {
                    if (!string.IsNullOrEmpty(list[i]))
                    {
                        list[i] = EscapeCharacters(list[i]);
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
                JsonSerializer.Serialize(jsonWriter, prompts, SourceGenerationContext.Default.DictionaryStringListString);
            }

            await File.WriteAllBytesAsync(filePath, stream.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Failed to save prompts to {filePath}: {ex.Message}");
            throw;
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

    private static async Task PopulateDatabaseAsync(VectorDB db, List<Tool> tools, EmbeddingService embeddingService)
    {
        const int threshold = 2;

        if (tools.Count > threshold)
        {
            // Split work into two halves and process them concurrently.
            int half = tools.Count / 2;
            var left = tools.Take(half).ToList();
            var right = tools.Skip(half).ToList();

            await Task.WhenAll(
                PopulateDatabaseAsync(db, left, embeddingService),
                PopulateDatabaseAsync(db, right, embeddingService));

            return;
        }

        foreach (var tool in tools)
        {
            var input = tool.Description ?? "";

            // Handle test tools specially
            string toolName;

            if (tool.Name.StartsWith(TestToolIdPrefix))
            {
                toolName = tool.Name;
            }
            else
            {
                // Convert command to tool name format (spaces to underscores)
                toolName = tool.Command?.Replace(" ", SpaceReplacement) ?? tool.Name;
            }

            var vector = await embeddingService.CreateEmbeddingsAsync(input);

            db.Upsert(new Entry(toolName, tool, vector));
        }
    }

    private static async Task PerformAnalysis(Dictionary<string, List<string>> toolNameWithPrompts, EmbeddingService embeddingService, VectorDB db, TimeSpan databaseSetupTime, StreamWriter writer, int maxResultsPerTest = 5, bool isCiMode = false)
    {
        var stopwatch = Stopwatch.StartNew();
        int promptCount = 0;
        var isTextOutput = IsTextOutput(); // Check if output should use text format

        if (isTextOutput)
        {
            await writer.WriteLineAsync($"Loaded {db.Count} tools in {databaseSetupTime.TotalSeconds:F7}s");
            await writer.WriteLineAsync();
        }
        else
        {
            await writer.WriteLineAsync("# Tool Selection Analysis Results");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync($"**Analysis Date:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}  ");
            await writer.WriteLineAsync($"**Tool count:** {db.Count}  ");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("## Table of Contents");
            await writer.WriteLineAsync();

            // Generate TOC
            int toolIndex = 1;

            foreach (var (toolName, prompts) in toolNameWithPrompts)
            {
                foreach (var _ in prompts)
                {
                    await writer.WriteLineAsync($"- [Test {toolIndex}: {toolName}](#test-{toolIndex})");
                    toolIndex++;
                }
            }

            await writer.WriteLineAsync();
            await writer.WriteLineAsync("---");
            await writer.WriteLineAsync();
        }

        int testNumber = 1;

        foreach (var (toolName, prompts) in toolNameWithPrompts)
        {
            foreach (var prompt in prompts)
            {
                promptCount++;

                if (isTextOutput)
                {
                    await writer.WriteLineAsync($"\nPrompt: {prompt}");
                    await writer.WriteLineAsync($"Expected tool: {toolName}");
                }
                else
                {
                    await writer.WriteLineAsync($"## Test {testNumber}");
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync($"**Expected Tool:** `{toolName}`  ");
                    await writer.WriteLineAsync($"**Prompt:** {prompt}  ");
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync("### Results");
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync("| Rank | Score | Tool | Status |");
                    await writer.WriteLineAsync("|------|-------|------|--------|");
                }

                var vector = await embeddingService.CreateEmbeddingsAsync(prompt);
                // Query a little more than requested so confidence metrics (which currently assume TopK=10) remain stable.
                // If user requests more than 10, expand TopK accordingly so we have enough rows.
                var topK = Math.Max(10, maxResultsPerTest);
                var queryResults = db.Query(vector, new QueryOptions(TopK: topK));

                for (int i = 0; i < Math.Min(maxResultsPerTest, queryResults.Count); i++)
                {
                    var qr = queryResults[i];

                    if (isTextOutput)
                    {
                        var note = qr.Entry.Id == toolName ? "*** EXPECTED ***" : "";
                        await writer.WriteLineAsync($"   {qr.Score:F6}   {qr.Entry.Id,-50}     {note}");
                    }
                    else
                    {
                        var status = qr.Entry.Id == toolName ? "✅ **EXPECTED**" : "❌";
                        await writer.WriteLineAsync($"| {i + 1} | {qr.Score:F6} | `{qr.Entry.Id}` | {status} |");
                    }
                }

                if (!isTextOutput)
                {
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync("---");
                    await writer.WriteLineAsync();
                }

                testNumber++;
            }
        }

        stopwatch.Stop();

        if (isTextOutput)
        {
            // Calculate success rate metrics for regular format too
            var metrics = await CalculateSuccessRateAsync(db, toolNameWithPrompts, embeddingService);

            await writer.WriteLineAsync($"\n\nTotal Prompts Tested={promptCount}, Analysis Execution Time={stopwatch.Elapsed.TotalSeconds:F7}s");
            await writer.WriteLineAsync($"Top choice success rate={metrics.TopChoicePercentage:F1}% ({metrics.TopChoiceCount}/{promptCount} tests passed)");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("Confidence Level Distribution:");
            await writer.WriteLineAsync($"  Very High Confidence (≥0.8): {metrics.VeryHighConfidencePercentage:F1}% ({metrics.VeryHighConfidenceCount}/{promptCount} tests)");
            await writer.WriteLineAsync($"  High Confidence (≥0.7): {metrics.HighConfidencePercentage:F1}% ({metrics.HighConfidenceCount}/{promptCount} tests)");
            await writer.WriteLineAsync($"  Good Confidence (≥0.6): {metrics.GoodConfidencePercentage:F1}% ({metrics.GoodConfidenceCount}/{promptCount} tests)");
            await writer.WriteLineAsync($"  Fair Confidence (≥0.5): {metrics.FairConfidencePercentage:F1}% ({metrics.FairConfidenceCount}/{promptCount} tests)");
            await writer.WriteLineAsync($"  Acceptable Confidence (≥0.4): {metrics.AcceptableConfidencePercentage:F1}% ({metrics.AcceptableConfidenceCount}/{promptCount} tests)");
            await writer.WriteLineAsync($"  Low Confidence (<0.4): {metrics.LowConfidencePercentage:F1}% ({metrics.LowConfidenceCount}/{promptCount} tests)");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("Top Choice + Confidence Combinations:");
            await writer.WriteLineAsync($"  Top + Very High Confidence (≥0.8): {metrics.TopChoiceVeryHighConfidencePercentage:F1}% ({metrics.TopChoiceVeryHighConfidenceCount}/{promptCount} tests)");
            await writer.WriteLineAsync($"  Top + High Confidence (≥0.7): {metrics.TopChoiceHighConfidencePercentage:F1}% ({metrics.TopChoiceHighConfidenceCount}/{promptCount} tests)");
            await writer.WriteLineAsync($"  Top + Good Confidence (≥0.6): {metrics.TopChoiceGoodConfidencePercentage:F1}% ({metrics.TopChoiceGoodConfidenceCount}/{promptCount} tests)");
            await writer.WriteLineAsync($"  Top + Fair Confidence (≥0.5): {metrics.TopChoiceFairConfidencePercentage:F1}% ({metrics.TopChoiceFairConfidenceCount}/{promptCount} tests)");
            await writer.WriteLineAsync($"  Top + Acceptable Confidence (≥0.4): {metrics.TopChoiceAcceptableConfidencePercentage:F1}% ({metrics.TopChoiceAcceptableConfidenceCount}/{promptCount} tests)");
        }
        else
        {
            await writer.WriteLineAsync("## Summary");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync($"**Total Prompts Tested:** {promptCount}  ");
            await writer.WriteLineAsync($"**Analysis Execution Time:** {stopwatch.Elapsed.TotalSeconds:F7}s  ");
            await writer.WriteLineAsync();

            // Calculate success rate metrics
            var metrics = await CalculateSuccessRateAsync(db, toolNameWithPrompts, embeddingService);
            await writer.WriteLineAsync("### Success Rate Metrics");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync($"**Top Choice Success:** {metrics.TopChoicePercentage:F1}% ({metrics.TopChoiceCount}/{promptCount} tests)  ");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("#### Confidence Level Distribution");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync($"**💪 Very High Confidence (≥0.8):** {metrics.VeryHighConfidencePercentage:F1}% ({metrics.VeryHighConfidenceCount}/{promptCount} tests)  ");
            await writer.WriteLineAsync($"**🎯 High Confidence (≥0.7):** {metrics.HighConfidencePercentage:F1}% ({metrics.HighConfidenceCount}/{promptCount} tests)  ");
            await writer.WriteLineAsync($"**✅ Good Confidence (≥0.6):** {metrics.GoodConfidencePercentage:F1}% ({metrics.GoodConfidenceCount}/{promptCount} tests)  ");
            await writer.WriteLineAsync($"**👍 Fair Confidence (≥0.5):** {metrics.FairConfidencePercentage:F1}% ({metrics.FairConfidenceCount}/{promptCount} tests)  ");
            await writer.WriteLineAsync($"**👌 Acceptable Confidence (≥0.4):** {metrics.AcceptableConfidencePercentage:F1}% ({metrics.AcceptableConfidenceCount}/{promptCount} tests)  ");
            await writer.WriteLineAsync($"**❌ Low Confidence (<0.4):** {metrics.LowConfidencePercentage:F1}% ({metrics.LowConfidenceCount}/{promptCount} tests)  ");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("#### Top Choice + Confidence Combinations");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync($"**💪 Top Choice + Very High Confidence (≥0.8):** {metrics.TopChoiceVeryHighConfidencePercentage:F1}% ({metrics.TopChoiceVeryHighConfidenceCount}/{promptCount} tests)  ");
            await writer.WriteLineAsync($"**🎯 Top Choice + High Confidence (≥0.7):** {metrics.TopChoiceHighConfidencePercentage:F1}% ({metrics.TopChoiceHighConfidenceCount}/{promptCount} tests)  ");
            await writer.WriteLineAsync($"**✅ Top Choice + Good Confidence (≥0.6):** {metrics.TopChoiceGoodConfidencePercentage:F1}% ({metrics.TopChoiceGoodConfidenceCount}/{promptCount} tests)  ");
            await writer.WriteLineAsync($"**👍 Top Choice + Fair Confidence (≥0.5):** {metrics.TopChoiceFairConfidencePercentage:F1}% ({metrics.TopChoiceFairConfidenceCount}/{promptCount} tests)  ");
            await writer.WriteLineAsync($"**👌 Top Choice + Acceptable Confidence (≥0.4):** {metrics.TopChoiceAcceptableConfidencePercentage:F1}% ({metrics.TopChoiceAcceptableConfidenceCount}/{promptCount} tests)  ");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("### Success Rate Analysis");
            await writer.WriteLineAsync();

            var overallScore = metrics.TopChoiceAcceptableConfidencePercentage; // Use ≥0.4 (acceptable) as the primary metric

            if (overallScore >= 90)
            {
                await writer.WriteLineAsync("🟢 **Excellent** - The tool selection system is performing exceptionally well.");
            }
            else if (overallScore >= 75)
            {
                await writer.WriteLineAsync("🟡 **Good** - The tool selection system is performing well.");
            }
            else if (overallScore >= 50)
            {
                await writer.WriteLineAsync("🟠 **Fair** - The tool selection system is performing adequately but has room for improvement.");
            }
            else
            {
                await writer.WriteLineAsync("🔴 **Poor** - The tool selection system requires major improvements.");
            }

            // Add recommendation based on confidence distribution
            if (metrics.VeryHighConfidencePercentage >= 70)
            {
                await writer.WriteLineAsync();
                await writer.WriteLineAsync("🎯 **Recommendation:** Tool descriptions are highly optimized for user intent matching.");
            }
            else if (metrics.GoodConfidencePercentage >= 80)
            {
                await writer.WriteLineAsync();
                await writer.WriteLineAsync("📈 **Recommendation:** Consider optimizing tool descriptions to achieve higher confidence scores (≥0.8).");
            }
            else if (metrics.AcceptableConfidencePercentage + metrics.GoodConfidencePercentage >= 70)
            {
                await writer.WriteLineAsync();
                await writer.WriteLineAsync("⚠️ **Recommendation:** Tool descriptions need improvement to better match user intent (targets: ≥0.6 good, ≥0.7 high).");
            }
            else
            {
                await writer.WriteLineAsync();
                await writer.WriteLineAsync("🔧 **Recommendation:** Significant improvements needed in tool descriptions for better semantic matching.");
            }

            await writer.WriteLineAsync();
        }

        // Calculate success rate metrics for console output
        var metricsForConsole = await CalculateSuccessRateAsync(db, toolNameWithPrompts, embeddingService);

        // Print summary to console for feedback
        Console.WriteLine($"🧪 Tested {promptCount} prompts:");
        Console.WriteLine($"   📊 Top choice: {metricsForConsole.TopChoicePercentage:F1}%");
        Console.WriteLine($"   💪 Very High confidence (≥0.8): {metricsForConsole.VeryHighConfidencePercentage:F1}%");
        Console.WriteLine($"   🎯 High confidence (≥0.7): {metricsForConsole.HighConfidencePercentage:F1}%");
        Console.WriteLine($"   ✅ Good confidence (≥0.6): {metricsForConsole.GoodConfidencePercentage:F1}%");
        Console.WriteLine($"   👍 Fair confidence (≥0.5): {metricsForConsole.FairConfidencePercentage:F1}%");
        Console.WriteLine($"   👌 Acceptable confidence (≥0.4): {metricsForConsole.AcceptableConfidencePercentage:F1}%");
        Console.WriteLine($"   ⭐ Top + acceptable confidence (≥0.4): {metricsForConsole.TopChoiceAcceptableConfidencePercentage:F1}%");
    }

    private static async Task<SuccessRateMetrics> CalculateSuccessRateAsync(VectorDB db, Dictionary<string, List<string>> toolNameWithPrompts, EmbeddingService embeddingService, int maxResultsPerTest = 5)
    {
        var metrics = new SuccessRateMetrics();

        foreach (var (toolName, prompts) in toolNameWithPrompts)
        {
            foreach (var prompt in prompts)
            {
                metrics.TotalTests++;
                var vector = await embeddingService.CreateEmbeddingsAsync(prompt);
                // Query a little more than requested so confidence metrics (which currently assume TopK=10) remain stable.
                // If user requests more than 10, expand TopK accordingly so we have enough rows.
                var topK = Math.Max(10, maxResultsPerTest);
                var queryResults = db.Query(vector, new QueryOptions(TopK: topK));

                if (queryResults.Count > 0)
                {
                    var topResult = queryResults[0];
                    var expectedToolResult = queryResults.FirstOrDefault(r => r.Entry.Id == toolName);

                    // Check if expected tool is top choice
                    if (topResult.Entry.Id == toolName)
                    {
                        metrics.TopChoiceCount++;

                        // Granular confidence categories for top choice
                        if (topResult.Score >= 0.8)
                        {
                            metrics.TopChoiceVeryHighConfidenceCount++;
                        }
                        if (topResult.Score >= 0.7)
                        {
                            metrics.TopChoiceHighConfidenceCount++;
                        }
                        if (topResult.Score >= 0.6)
                        {
                            metrics.TopChoiceGoodConfidenceCount++;
                        }
                        if (topResult.Score >= 0.5)
                        {
                            metrics.TopChoiceFairConfidenceCount++;
                        }
                        if (topResult.Score >= 0.4)
                        {
                            metrics.TopChoiceAcceptableConfidenceCount++;
                        }
                    }

                    // Count confidence levels for expected tool (regardless of rank)
                    if (expectedToolResult != null)
                    {
                        var score = expectedToolResult.Score;

                        // Granular confidence categories
                        if (score >= 0.8)
                        {
                            metrics.VeryHighConfidenceCount++;
                        }
                        if (score >= 0.7)
                        {
                            metrics.HighConfidenceCount++;
                        }
                        if (score >= 0.6)
                        {
                            metrics.GoodConfidenceCount++;
                        }
                        if (score >= 0.5)
                        {
                            metrics.FairConfidenceCount++;
                        }
                        if (score >= 0.4)
                        {
                            metrics.AcceptableConfidenceCount++;
                        }
                        if (score < 0.4)
                        {
                            metrics.LowConfidenceCount++;
                        }
                    }
                    else
                    {
                        // Expected tool not found in top 10 results
                        metrics.LowConfidenceCount++;
                    }
                }
                else
                {
                    // No results at all
                    metrics.LowConfidenceCount++;
                }
            }
        }

        return metrics;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Tool Selection Confidence Score Analyzer");
        Console.WriteLine();
        Console.WriteLine("USAGE:");
        Console.WriteLine("  ToolDescriptionEvaluator [OPTIONS]");
        Console.WriteLine();
        Console.WriteLine("MODES:");
        Console.WriteLine("  Default mode         Run full analysis on all tools and prompts");
        Console.WriteLine("  --test-single-tool   Test a specific tool description against one or more prompts");
        Console.WriteLine("                       (ignores --area, --server, --server-exe, --prompts-file)");
        Console.WriteLine();
        Console.WriteLine("OPTIONS:");
        Console.WriteLine("  --help, -h                    Show this help message");
        Console.WriteLine("  --ci                          Run in CI mode (graceful failures)");
        Console.WriteLine("  --tools-file <path>           Use a custom JSON file for tools instead of dynamic loading from docs .md");
        Console.WriteLine("  --prompts-file <path>         Use custom prompts file (supported formats: .md or .json)");
        Console.WriteLine("                                (ignored in --test-single-tool mode)");
        Console.WriteLine("  --area <area>                 Filter prompts by tool name prefix(es) (e.g., \"keyvault\", \"storage,functionapp\")");
        Console.WriteLine("                                (ignored in --test-single-tool mode)");
        Console.WriteLine("  --output-file-name <name>     Custom output file name (no extension)");
        Console.WriteLine("  --text-results                Output results in .txt format");
        Console.WriteLine("  --top <N>                     Number of results to display per test (default 5)");
        Console.WriteLine("  --tool-description <text>     A single tool description to test (required with --test-single-tool)");
        Console.WriteLine("  --prompt <text>               Test prompt (required with --test-single-tool, can be repeated)");
        Console.WriteLine("  --server <name>               Specify the server name (default: Azure). Valid options: Azure, Fabric");
        Console.WriteLine("                                (ignored in --test-single-tool mode)");
        Console.WriteLine("  --server-exe <path>           Specify the server executable path in the format (examples: ./azmcp.exe, ./azmcp, ./azmcp.dll)");
        Console.WriteLine("                                If both this and --server are provided, --server is ignored.");
        Console.WriteLine("                                (ignored in --test-single-tool mode)");
        Console.WriteLine();
        Console.WriteLine("ENVIRONMENT VARIABLES:");
        Console.WriteLine("  AOAI_ENDPOINT           Azure OpenAI endpoint URL");
        Console.WriteLine("  TEXT_EMBEDDING_API_KEY  Azure OpenAI API key");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  ToolDescriptionEvaluator                                          # Use dynamic tool loading (default)");
        Console.WriteLine("  ToolDescriptionEvaluator --tools-file my-tools.json               # Use custom tools file");
        Console.WriteLine("  ToolDescriptionEvaluator --prompts-file my-prompts.md             # Use custom prompts file");
        Console.WriteLine("  ToolDescriptionEvaluator --area \"keyvault\"                      # Test only Key Vault prompts");
        Console.WriteLine("  ToolDescriptionEvaluator --area \"storage\"                       # Test only Storage prompts");
        Console.WriteLine("  ToolDescriptionEvaluator --area \"keyvault,storage\"              # Test Key Vault and Storage prompts (multiple areas)");
        Console.WriteLine("  ToolDescriptionEvaluator --area \"functionapp\"                   # Test only Function App prompts");
        Console.WriteLine("  ToolDescriptionEvaluator --output-file-name my-results            # Use custom output file name (don't include extension)");
        Console.WriteLine("  ToolDescriptionEvaluator --text-results                           # Output in text format");
        Console.WriteLine("  ToolDescriptionEvaluator --ci --tools-file tools.json             # CI mode with JSON file");
        Console.WriteLine();
        Console.WriteLine("  # Test a single tool description against a single prompt:");
        Console.WriteLine("  ToolDescriptionEvaluator --test-single-tool \\");
        Console.WriteLine("    --tool-description \"Lists all storage accounts in a subscription\" \\");
        Console.WriteLine("    --prompt \"show me my storage accounts\"");
        Console.WriteLine();
        Console.WriteLine("  # Test a tool description against multiple prompts:");
        Console.WriteLine("  ToolDescriptionEvaluator --test-single-tool \\");
        Console.WriteLine("    --tool-description \"Lists storage accounts\" \\");
        Console.WriteLine("    --prompt \"show me storage accounts\" \\");
        Console.WriteLine("    --prompt \"list my storage accounts\" \\");
        Console.WriteLine("    --prompt \"what storage accounts do I have\"");
    }

    private static async Task TestSingleToolAsync(string? toolDescription, List<string> testPrompts, EmbeddingService embeddingService, VectorDB db, int maxResultsPerTest = 5)
    {
        Console.WriteLine("🔧 Testing Single Tool Description");
        Console.WriteLine($"📋 Tool Description: {toolDescription}");
        Console.WriteLine($"❓ Test Prompts: {testPrompts.Count}");

        for (int i = 0; i < testPrompts.Count; i++)
        {
            Console.WriteLine($"   Prompt {i + 1}: {testPrompts[i]}");
        }

        Console.WriteLine();

        try
        {
            // Create test tools with the provided description
            var testTools = new List<Tool>
            {
                new Tool
                {
                    Name = $"{TestToolIdPrefix}1",
                    Description = toolDescription
                }
            };

            await PopulateDatabaseAsync(db, testTools, embeddingService);

            // Test each prompt against all tools
            var testNumber = 1;

            foreach (var testPrompt in testPrompts)
            {
                Console.WriteLine($"🎯 Test {testNumber}: \"{testPrompt}\"");
                Console.WriteLine();

                var vector = await embeddingService.CreateEmbeddingsAsync(testPrompt);
                // Query a little more than requested so confidence metrics (which currently assume TopK=10) remain stable.
                // If user requests more than 10, expand TopK accordingly so we have enough rows.
                var topK = Math.Max(10, maxResultsPerTest);
                var queryResults = db.Query(vector, new QueryOptions(TopK: topK));

                // Find test tool rankings
                var testToolResults = new List<(int rank, float score, string toolName)>();

                for (int i = 0; i < queryResults.Count; i++)
                {
                    var result = queryResults[i];

                    if (result.Entry.Id.StartsWith(TestToolIdPrefix))
                    {
                        testToolResults.Add((i + 1, result.Score, $"{TestToolIdPrefix}1"));
                    }
                }

                // Display results for test tool
                Console.WriteLine("📊 Test Tool Results:");

                if (testToolResults.Count > 0)
                {
                    var (rank, score, toolName) = testToolResults.First();
                    var quality = rank == 1 ? "✅ Excellent" :
                                  rank <= 3 ? "🟡 Good" :
                                  rank <= 5 ? "🟠 Fair" : "🔴 Poor";
                    var confidence = score >= 0.8 ? "💪 Very high confidence" :
                                     score >= 0.7 ? "🎯 High confidence" :
                                     score >= 0.6 ? "✅ Good confidence" :
                                     score >= 0.5 ? "👍 Fair confidence" :
                                     score >= 0.4 ? "👌 Acceptable confidence" : "❌ Low confidence";

                    Console.WriteLine($"   {toolName}:");
                    Console.WriteLine($"      Rank #{rank} - {quality}");
                    Console.WriteLine($"      Score {score:F4} - {confidence}");
                    Console.WriteLine($"      Description: \"{toolDescription}\"");
                }
                else
                {
                    Console.WriteLine("   Test tool not found in top 10 results");
                }

                Console.WriteLine();
                Console.WriteLine("📋 Top 5 competing tools:");

                for (int i = 0; i < Math.Min(5, queryResults.Count); i++)
                {
                    var result = queryResults[i];
                    var isTestTool = result.Entry.Id.StartsWith(TestToolIdPrefix);
                    var indicator = isTestTool ? "👉 TEST TOOL" : "";

                    Console.WriteLine($"   {i + 1:D2}. {result.Score:F4} - {result.Entry.Id} {indicator}");
                }

                // Suggestions for improvement
                Console.WriteLine();

                var bestTestTool = testToolResults.FirstOrDefault();

                if (bestTestTool.rank > 1 || bestTestTool.score < 0.6)
                {
                    Console.WriteLine("💡 Suggestions for improvement:");

                    if (bestTestTool.score < 0.4)
                    {
                        Console.WriteLine("   • Consider using more specific keywords from the prompt");
                        Console.WriteLine("   • Include common synonyms or alternative phrasings");
                    }

                    if (bestTestTool.rank > 5)
                    {
                        Console.WriteLine("   • Look at top-ranking tool descriptions for inspiration");
                        Console.WriteLine("   • Ensure the descriptions clearly match the user intent");
                    }

                    Console.WriteLine("   • Test with multiple different prompts users might use");
                }

                Console.WriteLine();
                Console.WriteLine("---");
                Console.WriteLine();

                testNumber++;
            }

            // Summary
            Console.WriteLine("📈 Rankings Summary:");

            var totalTests = testPrompts.Count;
            var excellentCount = 0;
            var goodCount = 0;
            var fairCount = 0;
            var poorCount = 0;

            foreach (var testPrompt in testPrompts)
            {
                var vector = await embeddingService.CreateEmbeddingsAsync(testPrompt);
                // Query a little more than requested so confidence metrics (which currently assume TopK=10) remain stable.
                // If user requests more than 10, expand TopK accordingly so we have enough rows.
                var topK = Math.Max(10, maxResultsPerTest);
                var queryResults = db.Query(vector, new QueryOptions(TopK: topK));
                var testToolId = $"{TestToolIdPrefix}1";
                var rank = queryResults.FindIndex(r => r.Entry.Id == testToolId) + 1;

                if (rank == 1)
                    excellentCount++;
                else if (rank <= 3)
                    goodCount++;
                else if (rank <= 10)
                    fairCount++;
                else
                    poorCount++;
            }

            Console.WriteLine($"   Total prompts tested: {totalTests}");
            Console.WriteLine($"   ✅ Excellent (rank #1): {excellentCount} ({excellentCount * 100.0 / totalTests:F1}%)");
            Console.WriteLine($"   🟡 Good (rank #2-3): {goodCount} ({goodCount * 100.0 / totalTests:F1}%)");
            Console.WriteLine($"   🟠 Fair (rank #4-10): {fairCount} ({fairCount * 100.0 / totalTests:F1}%)");
            Console.WriteLine($"   🔴 Poor (rank #11+): {poorCount} ({poorCount * 100.0 / totalTests:F1}%)");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during validation: {ex.Message}");
            Console.WriteLine(ex.StackTrace);

            Environment.Exit(1);
        }
    }
}

internal static class UnicodeChars
{
    public const string LeftSingleQuote = "\u2018";
    public const string RightSingleQuote = "\u2019";
    public const string LeftDoubleQuote = "\u201C";
    public const string RightDoubleQuote = "\u201D";
}
