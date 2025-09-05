// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Tests.Client.Helpers;

public static class McpTestUtilities
{
    /// <summary>Gets the first text contents in the list.</summary>
    public static string? GetFirstText(IList<ContentBlock> contents)
    {
        foreach (var c in contents)
        {
            if (c is EmbeddedResourceBlock { Resource: TextResourceContents { MimeType: "application/json" } text })
            {
                return text.Text;
            }
            else if (c is TextContentBlock tc)
            {
                return tc.Text;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the path to the azmcp executable, handling OS-specific executable naming.
    /// </summary>
    /// <returns>The full path to the azmcp executable.</returns>
    public static string GetAzMcpExecutablePath()
    {
        string testAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string executableName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";
        return Path.Combine(testAssemblyPath, executableName);
    }
}
