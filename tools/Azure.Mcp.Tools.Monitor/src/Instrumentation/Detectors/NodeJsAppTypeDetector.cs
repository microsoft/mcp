using System.Text.Json;
using Azure.Mcp.Tools.Monitor.Models;

namespace Azure.Mcp.Tools.Monitor.Detectors;

public class NodeJsAppTypeDetector : IAppTypeDetector
{
    public Language SupportedLanguage => Language.NodeJs;

    public List<ProjectInfo> DetectProjects(string workspacePath)
    {
        var projects = new List<ProjectInfo>();

        var packageJsonPath = Path.Combine(workspacePath, "package.json");
        if (!File.Exists(packageJsonPath))
        {
            return projects;
        }

        try
        {
            var packageJson = JsonDocument.Parse(File.ReadAllText(packageJsonPath));
            var root = packageJson.RootElement;

            // Get project name
            var projectName = root.TryGetProperty("name", out var nameElement)
                ? nameElement.GetString() ?? "nodejs-app"
                : "nodejs-app";

            // Detect app type based on dependencies
            var appType = DetectAppType(root);

            // Common entry points for Node.js apps
            var entryPoint = DetectEntryPoint(workspacePath, root);

            projects.Add(new ProjectInfo
            {
                ProjectFile = packageJsonPath,
                EntryPoint = entryPoint,
                AppType = appType
            });
        }
        catch (JsonException)
        {
            // If package.json is malformed, return empty list
            return projects;
        }

        return projects;
    }

    private AppType DetectAppType(JsonElement root)
    {
        var dependencies = new List<string>();

        // Collect all dependencies
        if (root.TryGetProperty("dependencies", out var depsElement))
        {
            foreach (var dep in depsElement.EnumerateObject())
            {
                dependencies.Add(dep.Name);
            }
        }

        if (root.TryGetProperty("devDependencies", out var devDepsElement))
        {
            foreach (var dep in devDepsElement.EnumerateObject())
            {
                dependencies.Add(dep.Name);
            }
        }

        // Check for framework-specific packages
        // AI/ML frameworks first (they may also include web frameworks as dependencies)
        if (dependencies.Contains("langchain") || dependencies.Contains("@langchain/core"))
        {
            return AppType.LangchainJs;
        }

        if (dependencies.Contains("@nestjs/core") || dependencies.Contains("@nestjs/common"))
        {
            return AppType.NestJs;
        }

        if (dependencies.Contains("next"))
        {
            return AppType.NextJs;
        }

        if (dependencies.Contains("fastify"))
        {
            return AppType.Fastify;
        }

        if (dependencies.Contains("express"))
        {
            return AppType.Express;
        }

        // Database integrations (when no web framework is detected)
        if (dependencies.Contains("pg") || dependencies.Contains("pg-pool") || dependencies.Contains("postgres"))
        {
            return AppType.PostgresNodeJs;
        }

        if (dependencies.Contains("mongodb") || dependencies.Contains("mongoose"))
        {
            return AppType.MongoDBNodeJs;
        }

        if (dependencies.Contains("redis") || dependencies.Contains("ioredis"))
        {
            return AppType.RedisNodeJs;
        }

        if (dependencies.Contains("mysql2") || dependencies.Contains("mysql"))
        {
            return AppType.MySQLNodeJs;
        }

        // Logging integrations (when no web framework or database is detected)
        if (dependencies.Contains("winston"))
        {
            return AppType.WinstonNodeJs;
        }

        if (dependencies.Contains("bunyan"))
        {
            return AppType.BunyanNodeJs;
        }

        // Console-based apps (basic Node.js with no specific framework)
        // This is a fallback for any Node.js app that uses console for logging
        // or has minimal dependencies without a specific framework
        return AppType.ConsoleNodeJs;
    }

    private string DetectEntryPoint(string workspacePath, JsonElement root)
    {
        // Check package.json "main" field
        if (root.TryGetProperty("main", out var mainElement))
        {
            var mainFile = mainElement.GetString();
            if (!string.IsNullOrEmpty(mainFile))
            {
                return Path.Combine(workspacePath, mainFile);
            }
        }

        // Common entry point files
        var commonEntryPoints = new[] { "index.js", "server.js", "app.js", "src/index.js", "src/server.js", "src/app.js" };

        foreach (var entryPoint in commonEntryPoints)
        {
            var fullPath = Path.Combine(workspacePath, entryPoint);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return Path.Combine(workspacePath, "index.js"); // Default fallback
    }
}
