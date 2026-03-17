namespace Azure.Mcp.Tools.MonitorInstrumentation.Tools;

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
            return "Invalid resource path. Use list_learning_resources to see available resources.";
        }

        // File-based approach: Read from copied resources in output directory
        var baseDirectory = AppContext.BaseDirectory;
        var resourcesRoot = Path.GetFullPath(Path.Combine(baseDirectory, "Resources"));
        var resourcePath = Path.GetFullPath(Path.Combine(resourcesRoot, path));

        // Additional check: ensure resolved path is within Resources directory
        if (!resourcePath.StartsWith(resourcesRoot, StringComparison.OrdinalIgnoreCase))
        {
            return "Invalid resource path. Use list_learning_resources to see available resources.";
        }

        if (!File.Exists(resourcePath))
        {
            return $"Resource not found: {path}\n\nUse list_learning_resources to see available resources.";
        }

        return File.ReadAllText(resourcePath);

        // Embedded resources approach (commented out):
        // var assembly = Assembly.GetExecutingAssembly();
        // var resourceName = ResourcePrefix + path.Replace("/", ".").Replace("\\", ".");
        // using var stream = assembly.GetManifestResourceStream(resourceName);
        // if (stream == null)
        // {
        //     return $"Resource not found: {path}\n\nUse list_learning_resources to see available resources.";
        // }
        // using var reader = new StreamReader(stream);
        // return reader.ReadToEnd();
    }
}
