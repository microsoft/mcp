// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Storage.Tests;

public sealed class BuildOutputTests()
{
    [Fact]
    public void ServerRuntime_IsNotCopiedIntoToolTestOutput()
    {
        Assert.True(File.Exists(McpTestUtilities.GetAzMcpExecutablePath()));
        Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, "Azure.Mcp.Tools.Storage.dll")));
        Assert.False(File.Exists(Path.Combine(AppContext.BaseDirectory, "Azure.Mcp.Tools.KeyVault.dll")));
        Assert.Empty(Directory.GetFiles(AppContext.BaseDirectory, "azmcp*"));
    }
}
