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

        if (!string.IsNullOrEmpty(runConfig.NamespacesValue))
        {
            runConfig.Namespaces = [.. runConfig.NamespacesValue.Split(',')];
        }

        var outputDirectory = runConfig.OutputDirectory;
        if (string.IsNullOrEmpty(outputDirectory))
        {
            Console.WriteLine("Output directory is required.");
            return -1;
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

        try
        {
            await RunEvaluationAsync(repoRoot, runConfig);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during evaluation: {ex.Message}");
            Console.WriteLine(ex);
            return -1;
        }

        return 0;
    }

    private static async Task RunEvaluationAsync(string repoRoot, RunConfiguration configuration)
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

        List<string> namespaces;
        if (configuration.Namespaces == null || configuration.Namespaces.Count == 0)
        {
            Console.WriteLine("No namespaces specified. Using all available namespaces.");
            namespaces = promptDatastore.GetNamespaces();
        }
        else
        {
            namespaces = configuration.Namespaces;
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

            var outputDirectory = Path.Combine(vallyEvalDirectory, ns);
            Directory.CreateDirectory(outputDirectory);
            var outputFile = Path.Combine(outputDirectory, "eval.yaml");

            await VallyUtilities.WritePromptsAsync(prompts, outputFile, force: true);
        }
    }
}
