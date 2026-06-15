using Microsoft.Extensions.Configuration;

namespace VallyEvaluator;

public class RunConfiguration
{
    [ConfigurationKeyName("output")]
    public string OutputDirectory { get; set; } = string.Empty;

    [ConfigurationKeyName("namespaces")]
    public string NamespacesValue { get; set; } = string.Empty;

    public List<string> Namespaces { get; set; } = new List<string>();

    [ConfigurationKeyName("promptFile")]
    public string PromptFilePath { get; set; } = string.Empty;

    [ConfigurationKeyName("workingDirectory")]
    public string WorkingDirectory { get; set; } = string.Empty;
}
