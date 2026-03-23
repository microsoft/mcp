using Azure.Mcp.Tools.Monitor.Models;

namespace Azure.Mcp.Tools.Monitor.Detectors;

public class PythonLanguageDetector : ILanguageDetector
{
    public bool CanHandle(string workspacePath)
    {
        // Check for pyproject.toml which indicates a Python project
        var pyprojectTomlPath = Path.Combine(workspacePath, "pyproject.toml");
        if (File.Exists(pyprojectTomlPath))
        {
            return true;
        }

        // Check for setup.py which indicates a Python project
        var setupPyPath = Path.Combine(workspacePath, "setup.py");
        if (File.Exists(setupPyPath))
        {
            return true;
        }

        // Check for setup.cfg which indicates a Python project
        var setupCfgPath = Path.Combine(workspacePath, "setup.cfg");
        if (File.Exists(setupCfgPath))
        {
            return true;
        }

        try
        {
            // Abstract out pattern for checking single .py/ .js files later
            return Directory.EnumerateFiles(workspacePath, "*.py", SearchOption.AllDirectories).Any();
        }
        catch (UnauthorizedAccessException)
        {
            // Log or handle the exception as needed
            return false;
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
