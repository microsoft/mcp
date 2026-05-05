// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Areas.Tools.Options;

public static class ToolsListOptionDefinitions
{
    public const string NamespaceModeOptionName = "namespace-mode";
    public const string NamespaceOptionName = "namespace";
    public const string NameOnlyOptionName = "name-only";
    public const string IncludeHiddenOptionName = "include-hidden";

    public static readonly Option<bool> NamespaceMode = new($"--{NamespaceModeOptionName}")
    {
        Description = "If specified, returns a list of top-level service namespaces instead of individual tools.",
        Required = false
    };

    public static readonly Option<string[]> Namespace = new($"--{NamespaceOptionName}")
    {
        Description = "Filter tools by namespace (e.g., 'storage', 'keyvault'). Can be specified multiple times to include multiple namespaces.",
        Required = false,
        AllowMultipleArgumentsPerToken = true
    };

    public static readonly Option<bool> NameOnly = new($"--{NameOnlyOptionName}")
    {
        Description = "If specified, returns only tool names without descriptions or metadata.",
        Required = false
    };

    public static readonly Option<bool> IncludeHidden = new($"--{IncludeHiddenOptionName}")
    {
        Description = "If specified, includes hidden commands in the output.",
        Required = false
    };
}
