// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Models.Instrumentation;
using Microsoft.Mcp.Core.Services.Host;

namespace Azure.Mcp.Tools.Monitor.Instrumentation.Detectors;

/// <summary>
/// Detects Python application type based on framework dependencies.
/// Uses PythonInstrumentationRegistry to identify known frameworks.
/// </summary>
public class PythonAppTypeDetector(IFileSystem fileSystem, PythonInstrumentationRegistry registry) : IAppTypeDetector
{
    public Language SupportedLanguage => Language.Python;

    public List<ProjectInfo> DetectProjects(string workspacePath)
    {
        var projects = new List<ProjectInfo>();

        // Look for Python dependency files
        var requirementsPath = Path.Combine(workspacePath, "requirements.txt");
        var pyprojectPath = Path.Combine(workspacePath, "pyproject.toml");

        string? projectFile = null;
        var dependencies = new List<string>();

        // Parse requirements.txt
        if (fileSystem.FileExists(requirementsPath))
        {
            var content = PythonInstrumentationDetector.TryReadFile(requirementsPath, fileSystem);
            if (content != null)
            {
                projectFile = requirementsPath;
                dependencies.AddRange(PythonInstrumentationDetector.ParseRequirementsTxt(content));
            }
        }

        // Parse pyproject.toml
        if (fileSystem.FileExists(pyprojectPath))
        {
            var content = PythonInstrumentationDetector.TryReadFile(pyprojectPath, fileSystem);
            if (content != null)
            {
                projectFile ??= pyprojectPath;
                dependencies.AddRange(PythonInstrumentationDetector.ParsePyprojectToml(content));
            }
        }

        // No Python dependency files found
        if (projectFile == null)
        {
            return projects;
        }

        // Normalize package names
        dependencies = dependencies
            .Select(PythonInstrumentationRegistry.NormalizePackageName)
            .Distinct()
            .ToList();

        // Detect app type based on dependencies
        var appType = DetectAppType(dependencies);

        // Detect entry point
        var entryPoint = DetectEntryPoint(workspacePath, appType);

        projects.Add(new ProjectInfo
        {
            ProjectFile = projectFile,
            EntryPoint = entryPoint,
            AppType = appType,
            Dependencies = dependencies
        });

        return projects;
    }

    /// <summary>
    /// Detect Python app type by checking for framework packages.
    /// Priority order: Django > FastAPI > Flask > GenAI > others
    /// (Django and FastAPI are more specific, Flask is more general)
    /// </summary>
    private AppType DetectAppType(List<string> dependencies)
    {
        // Get known frameworks from registry
        var frameworks = registry.GetByCategory("framework")
            .Select(f => PythonInstrumentationRegistry.NormalizePackageName(f.LibraryName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Get GenAI libraries from registry
        var genaiLibraries = registry.GetByCategory("genai")
            .Select(f => PythonInstrumentationRegistry.NormalizePackageName(f.LibraryName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Check in priority order (most specific first)
        // Django - full-featured framework
        if (dependencies.Contains("django"))
        {
            return AppType.Django;
        }

        // FastAPI - modern async framework (uses Starlette internally)
        if (dependencies.Contains("fastapi"))
        {
            return AppType.FastAPI;
        }

        // Flask - lightweight framework
        if (dependencies.Contains("flask"))
        {
            return AppType.Flask;
        }

        // Starlette - async framework (FastAPI is built on this)
        if (dependencies.Contains("starlette"))
        {
            return AppType.Starlette;
        }

        // Falcon - REST API framework
        if (dependencies.Contains("falcon"))
        {
            return AppType.Falcon;
        }

        // GenAI - Check for OpenAI, Azure OpenAI, Anthropic, LangChain, etc.
        var foundGenAI = dependencies.FirstOrDefault(d => genaiLibraries.Contains(d));
        if (foundGenAI != null)
        {
            return AppType.GenAI;
        }

        // Check for any other known framework from registry
        var foundFramework = dependencies.FirstOrDefault(d => frameworks.Contains(d));
        if (foundFramework != null)
        {
            // Unrecognized framework — treat as Console since we have no specific generator.
            // The generic Python instrumentation setup still applies.
            return AppType.Console;
        }

        // Console/generic Python app — even with no dependencies, Python projects
        // should get the generic Console generator rather than "unsupported"
        return AppType.Console;
    }

    /// <summary>
    /// Detect the entry point file for a Python application.
    /// </summary>
    private string? DetectEntryPoint(string workspacePath, AppType appType)
    {
        // Django uses manage.py as the main entry point
        if (appType == AppType.Django)
        {
            var managePy = Path.Combine(workspacePath, "manage.py");
            if (fileSystem.FileExists(managePy))
            {
                return managePy;
            }
        }

        // Common entry point files for Python web apps
        var commonEntryPoints = new[]
        {
            "app.py",           // Flask/FastAPI convention
            "main.py",          // Common convention
            "server.py",        // Server apps
            "wsgi.py",          // WSGI apps
            "asgi.py",          // ASGI apps (FastAPI)
            "src/app.py",
            "src/main.py",
            "application.py"
        };

        foreach (var entryPoint in commonEntryPoints)
        {
            var fullPath = Path.Combine(workspacePath, entryPoint);
            if (fileSystem.FileExists(fullPath))
            {
                return fullPath;
            }
        }

        // Default fallback
        return null;
    }
}
