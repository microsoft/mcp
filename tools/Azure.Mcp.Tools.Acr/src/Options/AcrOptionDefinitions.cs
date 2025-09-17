// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Acr.Options;

public static class AcrOptionDefinitions
{
    public const string RegistryName = "registry";

    public static readonly Option<string> Registry = new(
        $"--{RegistryName}"
    )
    {
        Description = "The name of the Azure Container Registry. This is the unique name you chose for your container registry."
    };
}
