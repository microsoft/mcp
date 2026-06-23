namespace McpToolEvaluator.Core;

public static class Utilities
{
    public static string FindRepoRoot(string startingDir)
    {
        string? dir = startingDir;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "Microsoft.Mcp.slnx")))
            {
                return dir;
            }

            dir = Path.GetDirectoryName(dir);
        }

        throw new InvalidOperationException(
            "Could not find repo root (directory containing Microsoft.Mcp.slnx). Make sure you're running from within the repo.");
    }
}
