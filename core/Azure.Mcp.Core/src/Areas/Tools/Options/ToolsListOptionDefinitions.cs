// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Areas.Tools.Options;

public static class ToolsListOptionDefinitions
{
    public const string NamespacesOptionName = "namespaces";

    public static readonly Option<bool> Namespaces = new($"--{NamespacesOptionName}")
    {
        Description = "If specified, returns a list of top-level service namespaces instead of individual tools.",
        Required = false
    };
}
