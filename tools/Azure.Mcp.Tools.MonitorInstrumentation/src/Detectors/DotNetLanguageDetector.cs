using Azure.Mcp.Tools.MonitorInstrumentation.Models;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Detectors;

public class DotNetLanguageDetector : ILanguageDetector
{
    public bool CanHandle(string workspacePath)
    {
        return Detect(workspacePath) == Language.DotNet;
    }

    public Language Detect(string workspacePath)
    {
        // Look for .csproj, .fsproj, .vbproj, or .sln files
        var projectFiles = Directory.GetFiles(workspacePath, "*.csproj", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(workspacePath, "*.fsproj", SearchOption.AllDirectories))
            .Concat(Directory.GetFiles(workspacePath, "*.vbproj", SearchOption.AllDirectories))
            .ToList();

        if (projectFiles.Count > 0)
            return Language.DotNet;

        var slnFiles = Directory.GetFiles(workspacePath, "*.sln", SearchOption.TopDirectoryOnly);
        if (slnFiles.Length > 0)
            return Language.DotNet;

        // Check for global.json
        if (File.Exists(Path.Combine(workspacePath, "global.json")))
            return Language.DotNet;

        return Language.Unknown;
    }
}
