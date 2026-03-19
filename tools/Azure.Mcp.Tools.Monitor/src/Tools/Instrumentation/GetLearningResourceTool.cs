namespace Azure.Mcp.Tools.Monitor.Tools;

public sealed class GetLearningResourceTool
{
    public static string GetLearningResource(string path)
    {
        // Strip learn:// prefix if present
        if (path.StartsWith("learn://", StringComparison.OrdinalIgnoreCase))
        {
            path = path["learn://".Length..];
        }

        // Security validation: reject path traversal and absolute paths
        if (string.IsNullOrWhiteSpace(path) ||
            path.Contains("..") ||
            Path.IsPathRooted(path) ||
            path.Contains(':') ||
            path.StartsWith('/') ||
            path.StartsWith('\\'))
        {
            return "Invalid resource path. Call get_learning_resource without the path parameter to list all available resources.";
        }

        // File-based approach: Read from copied resources in output directory
        var baseDirectory = AppContext.BaseDirectory;
        var resourcesRoot = Path.GetFullPath(Path.Combine(baseDirectory, "Instrumentation", "Resources"));
        var resourcePath = Path.GetFullPath(Path.Combine(resourcesRoot, path));

        // Additional check: ensure resolved path is within Resources directory
        if (!resourcePath.StartsWith(resourcesRoot, StringComparison.OrdinalIgnoreCase))
        {
            return "Invalid resource path. Call get_learning_resource without the path parameter to list all available resources.";
        }

        if (!File.Exists(resourcePath))
        {
            return $"Resource not found: {path}\n\nCall get_learning_resource without the path parameter to list all available resources.";
        }

        return File.ReadAllText(resourcePath);

    }
}
