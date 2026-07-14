using McpToolEvaluator.Core;
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

        if (!string.IsNullOrEmpty(runConfig.NamespacesValue))
        {
            runConfig.Namespaces = [.. runConfig.NamespacesValue.Split(',')];
        }

        var repoRoot = Utilities.FindRepoRoot(AppContext.BaseDirectory);
        if (string.IsNullOrEmpty(runConfig.WorkingDirectory))
        {
            runConfig.WorkingDirectory = Path.Join(repoRoot, ".work");
        }

        if (!Directory.Exists(runConfig.WorkingDirectory))
        {
            Directory.CreateDirectory(runConfig.WorkingDirectory);
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

        try
        {
            await RunEvaluationAsync(repoRoot, runConfig, buildInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during evaluation: {ex.Message}");
            Console.WriteLine(ex);
            return -1;
        }

        return 0;
    }

    internal static List<string> GetTestToolNamespaces(BuildInfo buildInfo, PromptDatastore promptDatastore)
    {
        var promptNamespaces = promptDatastore.GetNamespaces().ToHashSet(StringComparer.InvariantCultureIgnoreCase);
        var results = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        foreach(var pathToTest in buildInfo.Data.PathsToTest)
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
            else
            {
                Console.Error.WriteLine($"Namespace not found in prompt datastore: {possibleNamespace}. Original: {split[1]}");
            }
        }

        return results.ToList();
    }

    private static async Task RunEvaluationAsync(string repoRoot, RunConfiguration configuration, BuildInfo? buildInfo = null)
    {
        string promptsPath = string.Empty;
        if (string.IsNullOrEmpty(configuration.PromptFilePath))
        {
            promptsPath = Path.Combine(repoRoot, "servers", "Azure.Mcp.Server", "docs", "e2eTestPrompts.md");
        }
        else
        {
            promptsPath = Path.GetFullPath(configuration.PromptFilePath);
        }

        var promptDatastore = new PromptDatastore(promptsPath);
        var vallyEvalDirectory = Path.Combine(configuration.WorkingDirectory, "evals");
        Directory.CreateDirectory(vallyEvalDirectory);

        HashSet<string> namespaces = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        if (buildInfo != null)
        {
            var testToolNamespaces = GetTestToolNamespaces(buildInfo, promptDatastore);
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
                    p.Prompt = VallyUtilities.ReplaceAngleBracketPlaceholders(prompt, VallyUtilities.Replacements);
                    return p;
                })
                .OrderBy(p => p.Prompt)
                .ToList();

            if (prompts.Count == 0)
            {
                Console.WriteLine($"- No prompts found for namespace: {ns}");
                continue;
            }

            var outputDirectory = Path.Combine(vallyEvalDirectory, ns);
            Directory.CreateDirectory(outputDirectory);
            var outputFile = Path.Combine(outputDirectory, "eval.yaml");

            await VallyUtilities.WritePromptsAsync(prompts, outputFile, force: true);
        }
    }
}
