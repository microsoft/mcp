// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

public sealed class ResourcePluginFileReferenceAllowlistProviderTests
{
    [Fact]
    public void ParseAllowedPaths_NestedReferenceArrays_ReturnsAllPaths()
    {
        var json = """
        {
            "references": {
                "azure-skills": ["azure-storage/references/account.md", "azure-ai/references/models.md"],
                "foundry": ["microsoft-foundry/references/agents.md"]
            }
        }
        """;

        var result = ResourcePluginFileReferenceAllowlistProvider.ParseAllowedPaths(json);

        Assert.Equal(
            [
                "azure-storage/references/account.md",
                "azure-ai/references/models.md",
                "microsoft-foundry/references/agents.md"
            ],
            result);
    }
}