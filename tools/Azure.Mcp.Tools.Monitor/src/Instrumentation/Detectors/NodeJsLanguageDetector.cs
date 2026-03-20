using Azure.Mcp.Tools.Monitor.Models;

namespace Azure.Mcp.Tools.Monitor.Detectors;

public class NodeJsLanguageDetector : ILanguageDetector
{
    public bool CanHandle(string workspacePath)
    {
        // Check for package.json which indicates a Node.js project
        var packageJsonPath = Path.Combine(workspacePath, "package.json");
        return File.Exists(packageJsonPath);
    }

    public Language Detect(string workspacePath)
    {
        if (CanHandle(workspacePath))
        {
            return Language.NodeJs;
        }

        return Language.Unknown;
    }
}
