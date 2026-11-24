// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Models.Instrumentation;
using Microsoft.Mcp.Core.Services.Host;

namespace Azure.Mcp.Tools.Monitor.Instrumentation.Detectors;

public class NodeJsLanguageDetector(IFileSystem fileSystem) : ILanguageDetector
{
    public bool CanHandle(string workspacePath)
    {
        // Check for package.json which indicates a Node.js project
        var packageJsonPath = Path.Combine(workspacePath, "package.json");
        return fileSystem.FileExists(packageJsonPath);
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
