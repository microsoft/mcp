// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class ItemDefinitionTestData
{
    internal static FabricItemDefinition Definition => new(
        [
            Part("nested/source.txt", [0xef, 0xbb, 0xbf, 65, 13, 10]),
            Part("assets/binary.bin", [0, 1, 255, 254]),
            Part("empty", []),
            Part(".platform", [123, 125])
        ], "FutureFormat");

    internal static FabricItemDefinitionPart Part(string path, byte[] bytes) =>
        new(path, Convert.ToBase64String(bytes), "InlineBase64");

    internal static string Json(FabricItemDefinition? definition = null) =>
        JsonSerializer.Serialize(new FabricItemDefinitionResponse(definition ?? Definition),
            CoreJsonContext.Default.FabricItemDefinitionResponse);

    internal static HttpResponseMessage Response(FabricItemDefinition? definition = null) =>
        FabricOperationTestData.JsonResponse(Json(definition));
}
