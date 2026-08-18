// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using McpToolEvaluator.Core.Models;
using Xunit;

namespace McpToolEvaluator.Core.Tests;

public sealed class ToolMetadataTests
{
    [Fact]
    public void Record_WithSameValues_IsEqual()
    {
        var left = new ToolMetadata
        {
            AssemblyName = "Azure.Mcp.Tools.Storage",
            ToolNamespace = "storage"
        };

        var right = new ToolMetadata
        {
            AssemblyName = "Azure.Mcp.Tools.Storage",
            ToolNamespace = "storage"
        };

        Assert.Equal(left, right);
    }

    [Fact]
    public void Record_WithDifferentValues_IsNotEqual()
    {
        var left = new ToolMetadata
        {
            AssemblyName = "Azure.Mcp.Tools.Storage",
            ToolNamespace = "storage"
        };

        var right = new ToolMetadata
        {
            AssemblyName = "Azure.Mcp.Tools.KeyVault",
            ToolNamespace = "keyvault"
        };

        Assert.NotEqual(left, right);
    }
}
