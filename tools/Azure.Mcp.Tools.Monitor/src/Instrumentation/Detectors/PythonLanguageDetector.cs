// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Models.Instrumentation;
using Microsoft.Mcp.Core.Services.Host;

namespace Azure.Mcp.Tools.Monitor.Instrumentation.Detectors;

public class PythonLanguageDetector(IFileSystem fileSystem) : ILanguageDetector
{
    public bool CanHandle(string workspacePath)
    {
        // Check for pyproject.toml which indicates a Python project
        var pyprojectTomlPath = Path.Combine(workspacePath, "pyproject.toml");
        if (fileSystem.FileExists(pyprojectTomlPath))
        {
            return true;
        }

        // Check for setup.py which indicates a Python project
        var setupPyPath = Path.Combine(workspacePath, "setup.py");
        if (fileSystem.FileExists(setupPyPath))
        {
            return true;
        }

        // Check for setup.cfg which indicates a Python project
        var setupCfgPath = Path.Combine(workspacePath, "setup.cfg");
        if (fileSystem.FileExists(setupCfgPath))
        {
            return true;
        }

        try
        {
            // Abstract out pattern for checking single .py/ .js files later
            return fileSystem.EnumerateFiles(workspacePath, "*.py", SearchOption.AllDirectories).Any();
        }
        catch (DirectoryNotFoundException)
        {
            // Log or handle the exception as needed
            return false;
        }

    }

    public Language Detect(string workspacePath)
    {
        if (CanHandle(workspacePath))
        {
            return Language.Python;
        }

        return Language.Unknown;
    }
}
