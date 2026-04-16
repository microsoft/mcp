// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
            return "Invalid resource path. Call get-learning-resource without the path parameter to list all available resources.";
        }

        // File-based approach: Read from copied resources in output directory
        var baseDirectory = AppContext.BaseDirectory;
        var resourcesRoot = Path.GetFullPath(Path.Combine(baseDirectory, "Instrumentation", "Resources"));
        var resourcePath = Path.GetFullPath(Path.Combine(resourcesRoot, path));

        // Additional check: ensure resolved path is within Resources directory
        if (!resourcePath.StartsWith(resourcesRoot, StringComparison.OrdinalIgnoreCase))
        {
            return "Invalid resource path. Call get-learning-resource without the path parameter to list all available resources.";
        }

        if (!File.Exists(resourcePath))
        {
            return $"Resource not found: {path}\n\nCall get-learning-resource without the path parameter to list all available resources.";
        }

        return File.ReadAllText(resourcePath);

    }

    public static List<string> ListLearningResources()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var resourcesPath = Path.Combine(baseDirectory, "Instrumentation", "Resources");

        if (!Directory.Exists(resourcesPath))
        {
            return [];
        }

        return Directory.GetFiles(resourcesPath, "*.md", SearchOption.AllDirectories)
            .Select(filePath => Path.GetRelativePath(resourcesPath, filePath).Replace("\\", "/"))
            .OrderBy(x => x)
            .ToList();

    }

    // Embedded resources approach (commented out):
    // private const string ResourcePrefix = "Azure.Mcp.Tools.Monitor.Resources.";
    //
    // public static string ListLearningResources()
    // {
    //     var assembly = Assembly.GetExecutingAssembly();
    //     var resources = assembly.GetManifestResourceNames()
    //         .Where(name => name.StartsWith(ResourcePrefix))
    //         .Select(name => ConvertToPath(name.Substring(ResourcePrefix.Length)))
    //         .OrderBy(x => x)
    //         .ToList();
    //
    //     if (resources.Count == 0)
    //     {
    //         return "No learning resources found.";
    //     }
    //
    //     return "Available learning resources:\n" + string.Join("\n", resources.Select(r => $"  {r}"));
    // }
    //
    // private static string ConvertToPath(string embeddedName)
    // {
    //     var parts = embeddedName.Split('.');
    //     if (parts.Length >= 2 && parts[^1] == "md")
    //     {
    //         var pathParts = parts[..^2];
    //         var fileName = parts[^2] + ".md";
    //         return string.Join("/", pathParts.Append(fileName));
    //     }
    //     return embeddedName.Replace(".", "/");
    // }
}
