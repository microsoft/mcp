// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using ToolSelection.Models;
using Xunit;

namespace ToolMetadataExporter.UnitTests.Models;

public class ListToolsResultTests
{
    [Fact]
    public void Deserialize_ReadsCommandsFromObjectRoot()
    {
        const string json = """
            {
              "status": 200,
              "results": {
                "commands": [
                  {
                    "name": "list",
                    "command": "tools list"
                  }
                ]
              }
            }
            """;

        var result = Assert.IsType<ListToolsResult>(
            JsonSerializer.Deserialize(json, SourceGenerationContext.Default.ListToolsResult));
        var tool = Assert.Single(Assert.IsType<List<Tool>>(result.Tools));

        Assert.Equal("list", tool.Name);
        Assert.Equal("tools list", tool.Command);
    }
}
