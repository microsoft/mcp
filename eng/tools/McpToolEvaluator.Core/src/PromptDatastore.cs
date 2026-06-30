using McpToolEvaluator.Core.Models;

namespace McpToolEvaluator.Core;

public class PromptDatastore
{
    private readonly List<TestPrompt> prompts;
    private readonly Dictionary<string, List<TestPrompt>> promptsByNamespace;

    public PromptDatastore(string promptFilePath)
    {
        prompts = PromptParser.ParseFile(promptFilePath);
        promptsByNamespace = prompts.GroupBy(x => x.Namespace)
                                    .ToDictionary(g => g.Key, g => g.ToList());
    }

    public List<string> GetNamespaces()
    {
        return [.. promptsByNamespace.Keys];
    }

    public List<TestPrompt> GetPromptsByNamespace(string toolNamespace)
    {
        return promptsByNamespace.TryGetValue(toolNamespace, out var prompts)
            ? prompts
            : [];
    }

    public async Task SaveAsync(string outputDirectory, bool force)
    {
        if (Directory.Exists(outputDirectory) && !force)
        {
            throw new InvalidOperationException($"Output directory {outputDirectory} already exists. Use force option to overwrite.");
        }

        foreach (var kvp in promptsByNamespace)
        {
            var namespaceDir = Path.Combine(outputDirectory, kvp.Key);
            Directory.CreateDirectory(namespaceDir);

            foreach (var prompt in kvp.Value)
            {
                var promptFile = Path.Combine(namespaceDir, $"{prompt.Tool}.md");
                await File.WriteAllTextAsync(promptFile, prompt.Prompt);
            }
        }
    }
}
