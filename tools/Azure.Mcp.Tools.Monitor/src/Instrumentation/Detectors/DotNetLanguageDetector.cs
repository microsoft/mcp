// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Models.Instrumentation;
using Microsoft.Mcp.Core.Services.Host;

namespace Azure.Mcp.Tools.Monitor.Instrumentation.Detectors;

public class DotNetLanguageDetector(IFileSystem fileSystem) : ILanguageDetector
{
    public bool CanHandle(string workspacePath)
    {
        return Detect(workspacePath) == Language.DotNet;
    }

    public Language Detect(string workspacePath)
    {
        // Look for .csproj, .fsproj, .vbproj, or .sln files
        var projectFiles = fileSystem.GetFiles(workspacePath, "*.csproj", SearchOption.AllDirectories)
            .Concat(fileSystem.GetFiles(workspacePath, "*.fsproj", SearchOption.AllDirectories))
            .Concat(fileSystem.GetFiles(workspacePath, "*.vbproj", SearchOption.AllDirectories))
            .ToList();

        if (projectFiles.Count > 0)
            return Language.DotNet;

        var slnFiles = fileSystem.GetFiles(workspacePath, "*.sln", SearchOption.TopDirectoryOnly);
        if (slnFiles.Length > 0)
            return Language.DotNet;

        // Check for global.json
        if (fileSystem.FileExists(Path.Combine(workspacePath, "global.json")))
            return Language.DotNet;

        return Language.Unknown;
    }
}
