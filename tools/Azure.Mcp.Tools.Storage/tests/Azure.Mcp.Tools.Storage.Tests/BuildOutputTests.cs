// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Storage.Tests;

public sealed class BuildOutputTests()
{
    [Fact]
    public void ServerRuntime_IsNotCopiedIntoSeparateToolTestOutput()
    {
        string serverExecutable = McpTestUtilities.GetAzMcpExecutablePath();
        Assert.True(File.Exists(serverExecutable));
        Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, "Azure.Mcp.Tools.Storage.dll")));

        if (Path.GetRelativePath(AppContext.BaseDirectory, Path.GetDirectoryName(serverExecutable)!) != ".")
        {
            Assert.False(File.Exists(Path.Combine(AppContext.BaseDirectory, "Azure.Mcp.Tools.KeyVault.dll")));
            Assert.Empty(Directory.GetFiles(AppContext.BaseDirectory, "azmcp*"));
        }
    }
}
