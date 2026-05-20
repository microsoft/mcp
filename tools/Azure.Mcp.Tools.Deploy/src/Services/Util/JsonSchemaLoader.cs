// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.Deploy.Services.Util;

public static class JsonSchemaLoader
{
    public static string LoadAppTopologyJsonSchema()
    {
        return LoadFileText("deploy-app-topology-schema.json");
    }

    private static string LoadFileText(string resourceFileName)
    {
        Assembly assembly = typeof(JsonSchemaLoader).Assembly;
        string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, resourceFileName);
        return EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
    }
}
