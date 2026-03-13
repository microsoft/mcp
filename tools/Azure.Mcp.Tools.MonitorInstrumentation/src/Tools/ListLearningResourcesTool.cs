namespace Azure.Mcp.Tools.MonitorInstrumentation.Tools;

public sealed class ListLearningResourcesTool
{
    public static string ListLearningResources()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var resourcesPath = Path.Combine(baseDirectory, "Resources");

        if (!Directory.Exists(resourcesPath))
        {
            return "No learning resources found.";
        }

        var resources = Directory.GetFiles(resourcesPath, "*.md", SearchOption.AllDirectories)
            .Select(filePath => Path.GetRelativePath(resourcesPath, filePath).Replace("\\", "/"))
            .OrderBy(x => x)
            .ToList();

        if (resources.Count == 0)
        {
            return "No learning resources found.";
        }

        return "Available learning resources:\n" + string.Join("\n", resources.Select(r => $"  {r}"));
    }

    // Embedded resources approach (commented out):
    // private const string ResourcePrefix = "Azure.Mcp.Tools.MonitorInstrumentation.Resources.";
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
