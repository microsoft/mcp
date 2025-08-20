// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.BicepSchema.Services.Support;

public static class ResourceParser
{
    public static (string Provider, string ResourceName, string? ApiVersion) ParseResourceType(string resourceTypeNameWithOrWithoutVersion)
    {
        int slashIndex = resourceTypeNameWithOrWithoutVersion.IndexOf('/');
        if (slashIndex < 0)
        {
            throw new ArgumentException($"Invalid resource type name format \"{resourceTypeNameWithOrWithoutVersion}\"");
        }

        string provider = resourceTypeNameWithOrWithoutVersion.Substring(0, slashIndex);
        string rest = resourceTypeNameWithOrWithoutVersion.Substring(slashIndex + 1);

        int atIndex = rest.IndexOf('@');
        if (atIndex < 0)
        {
            return (provider, rest, null);
        }

        return (provider, rest.Substring(0, atIndex), rest.Substring(atIndex + 1));
    }
}
