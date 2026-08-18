// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

public sealed class ResourcePluginSkillNameAllowlistProviderTests
{
    [Fact]
    public void ParseAllowedSkillNames_NestedSkillArrays_ReturnsAllSkillNames()
    {
        var json = """
        {
            "skills": {
                "azure-skills": ["azure-storage", "azure-ai"],
                "foundry": ["microsoft-foundry"]
            }
        }
        """;

        var result = ResourcePluginSkillNameAllowlistProvider.ParseAllowedSkillNames(json);

        Assert.Equal(["azure-storage", "azure-ai", "microsoft-foundry"], result);
    }
}
