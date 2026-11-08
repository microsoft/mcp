// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class FolderCreateTestData
{
    internal static string CreatedFolder(string? parent = null, string name = "Sales") =>
        JsonSerializer.Serialize(new FabricFolder(Guid.Parse(FolderId), name, Guid.Parse(WorkspaceId),
            parent is null ? null : Guid.Parse(parent)), CoreJsonContext.Default.FabricFolder);

    internal static IEnumerable<string> InvalidNames =>
        new[] { "", " ", " Sales", "Sales ", "\u00a0Sales", "Sales\u2000", "recycled", "RECYCLER", "$recycle.bin",
            "\uD800", "\uDC00", "Sales\uD800", new string('a', 256), string.Concat(Enumerable.Repeat("\U0001F680", 256)) }
        .Concat("~\"#.&*:<>?/{|}".Select(character => $"Sales{character}Team"))
        .Concat(Enumerable.Range(0, 32).Concat(Enumerable.Range(127, 33)).Select(code => $"Sales{(char)code}Team"));
}
