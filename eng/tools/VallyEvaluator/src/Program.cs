// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using McpToolEvaluator.Core;
using McpToolEvaluator.Core.Models;
using Microsoft.Extensions.Configuration;

namespace VallyEvaluator;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build();

        var runConfig = new RunConfiguration();
        configuration.Bind(runConfig);

        if (string.IsNullOrEmpty(runConfig.ServerName))
        {
            Console.WriteLine("No MCP server specified. Using default: Azure.Mcp.Server");
            runConfig.ServerName = "Azure.Mcp.Server";
        }

        if (!string.IsNullOrEmpty(runConfig.NamespacesValue))
        {
            Console.WriteLine("Using namespaces from command line: " + runConfig.NamespacesValue);
            runConfig.Namespaces = [.. runConfig.NamespacesValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
        }
        else
        {
            Console.WriteLine("No namespaces specified. Writing evals for all namespaces");
        }

        var repoRoot = Utilities.FindRepoRoot(AppContext.BaseDirectory);
        if (string.IsNullOrEmpty(runConfig.WorkingDirectory))
        {
            var workDir = Path.Join(repoRoot, ".work");
            runConfig.WorkingDirectory = workDir;
            Console.WriteLine("No working directory specified. Using default: " + workDir);
        }

        if (!Directory.Exists(runConfig.WorkingDirectory))
        {
            Directory.CreateDirectory(runConfig.WorkingDirectory);
        }

        if (string.IsNullOrEmpty(runConfig.PromptFilePath))
        {
            Console.WriteLine("No prompts file specified specified. Using prompts from Azure.Mcp.Server");
            runConfig.PromptFilePath = Path.Combine(repoRoot, "servers", "Azure.Mcp.Server", "docs", "e2eTestPrompts.md");
        }
        else
        {
            runConfig.PromptFilePath = Path.GetFullPath(runConfig.PromptFilePath);
        }

        BuildInfo? buildInfo = null;
        if (!string.IsNullOrEmpty(runConfig.BuildInfo))
        {
            if (!File.Exists(runConfig.BuildInfo))
            {
                Console.WriteLine($"Build info file not found: {runConfig.BuildInfo}");
                return -1;
            }

            buildInfo = new BuildInfo(runConfig.BuildInfo);
        }

        McpServerInformation? mcpServerInformation = null;
        if (buildInfo != null)
        {
            mcpServerInformation = await GetMcpServerInfo(runConfig, buildInfo);
        }

        try
        {
            Console.WriteLine($"Creating evals for MCP server: {runConfig.ServerName}");
            await CreateEvalsAsync(repoRoot, runConfig, buildInfo, mcpServerInformation);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during evaluation: {ex.Message}");
            Console.WriteLine(ex);
            return -1;
        }

        return 0;
    }

    internal static List<string> GetTestToolNamespaces(BuildInfo buildInfo, PromptDatastore promptDatastore, McpServerInformation? mcpServerInformation)
    {
        var promptNamespaces = promptDatastore.GetNamespaces().ToHashSet(StringComparer.InvariantCultureIgnoreCase);
        var results = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var pathToTest in buildInfo.Data.PathsToTest)
        {
            var split = pathToTest.Path.Split(['/', '\\'], 2);
            if (split.Length != 2)
            {
                continue;
            }

            if (!"tools".Equals(split[0], StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            var lastPeriod = split[1].LastIndexOf('.');
            var possibleNamespace = split[1].Substring(lastPeriod + 1).ToLowerInvariant();

            if (promptNamespaces.Contains(possibleNamespace))
            {
                results.Add(possibleNamespace);
            }
            else if (mcpServerInformation != null)
            {
                var assemblyName = split[1];
                if (mcpServerInformation.AssemblyNameToToolsMap.TryGetValue(assemblyName, out var matching))
                {
                    results.Add(matching.ToolNamespace);
                }
                else
                {
                    Console.Error.WriteLine($"Namespace not found in MCP server referenced assemblies: {possibleNamespace}. Original: {assemblyName}");
                }
            else
                {
                    Console.Error.WriteLine($"Namespace not found.  No MCP server to probe for namespaces: {possibleNamespace}. Original: {split[1]}");
                }
            }

            return results.ToList();
        }

    internal static Task<McpServerInformation> GetMcpServerInfo(RunConfiguration configuration, BuildInfo buildInfo)
    {
        var serverInfo = buildInfo.Data.Servers.FirstOrDefault(x => x.Name.Equals(configuration.ServerName, StringComparison.OrdinalIgnoreCase));
        if (serverInfo == null)
        {
            throw new InvalidOperationException($"Server information not found for server: {configuration.ServerName}");
        }

        var platformInfo = serverInfo.Platforms.FirstOrDefault(p =>
        {
            var pIdentifier = $"{p.DotnetOs}-{p.Architecture}";
            return RuntimeInformation.RuntimeIdentifier.Equals(pIdentifier);
        });
        if (platformInfo == null)
        {
            throw new InvalidOperationException($"Platform information not found for server: {configuration.ServerName}");
        }

        var artifactPath = Path.Combine(configuration.BuildDirectory, platformInfo.ArtifactPath);
        if (!Directory.Exists(artifactPath))
        {
            throw new InvalidOperationException($"Artifact path does not exist: {artifactPath}");
        }

        return Task.FromResult(new McpServerInformation(artifactPath));
    }

    private static async Task CreateEvalsAsync(string repoRoot, RunConfiguration configuration, BuildInfo? buildInfo = null, McpServerInformation? mcpServerInformation = null)
    {
        if (mcpServerInformation == null)
        {
            // Assuming that Azure.Mcp.Tools.Acr will also have E2E prompts that are `acr_`
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("WARNING: MCP server information is null. Assuming the assembly name and tool area match.");
            Console.ResetColor();
        }

        var promptDatastore = new PromptDatastore(configuration.PromptFilePath);
        var vallyEvalDirectory = Path.Combine(configuration.VallyBaseDirectory, "evals");

        Directory.CreateDirectory(vallyEvalDirectory);

        var namespaces = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        if (buildInfo != null)
        {
            var testToolNamespaces = GetTestToolNamespaces(buildInfo, promptDatastore, mcpServerInformation);
            if (testToolNamespaces.Count == 0)
            {
                Console.WriteLine("No valid namespaces found in build info. Exiting.");
                return;
            }

            namespaces.UnionWith(testToolNamespaces);
        }

        if (configuration.Namespaces != null && configuration.Namespaces.Count > 0)
        {
            Console.WriteLine($"Using specified namespaces: {string.Join(", ", configuration.Namespaces)}");

            namespaces.UnionWith(configuration.Namespaces);
        }
        else if (buildInfo == null)
        {
            Console.WriteLine("No namespaces specified and no build info provided. Using all available namespaces.");
            namespaces.UnionWith(promptDatastore.GetNamespaces());
        }

        foreach (var ns in namespaces)
        {
            var prompts = promptDatastore.GetPromptsByNamespace(ns)
                .Select(p =>
                {
                    var prompt = p.Prompt.Replace("\\<", "<");
                    p.Prompt = prompt;

                    return p;
                })
                .OrderBy(p => p.Prompt);

            if (!prompts.Any())
            {
                Console.WriteLine($"- No prompts found for namespace: {ns}");
                continue;
            }

            var noInteractionPrompts = prompts.Where(p => p.Interaction == PromptInteraction.None).ToList();

            if (!noInteractionPrompts.Any())
            {
                Console.WriteLine($"- All prompts require interaction for namespace: {ns}");
                continue;
            }

            var outputDirectory = Path.Combine(vallyEvalDirectory, ns);
            Directory.CreateDirectory(outputDirectory);
            var outputFile = Path.Combine(outputDirectory, "eval.yaml");

            await VallyUtilities.WritePromptsAsync(noInteractionPrompts, outputFile, force: true);
        }
    }
}
