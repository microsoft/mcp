// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Areas.Server.Options;

public static class ServiceOptionDefinitions
{
    public const string TransportName = "transport";
    public const string NamespaceName = "namespace";
    public const string ModeName = "mode";
    public const string ReadOnlyName = "read-only";

    public static readonly Option<string> Transport = new(
        $"--{TransportName}",
        () => TransportTypes.StdIo,
        "Transport mechanism to use for Azure MCP Server."
    )
    {
        IsRequired = false
    };

    public static readonly Option<string[]?> Namespace = new(
        $"--{NamespaceName}",
        () => null,
        "The Azure service namespaces to expose on the MCP server (e.g., storage, keyvault, cosmos)."
    )
    {
        IsRequired = false,
        Arity = ArgumentArity.OneOrMore,
        AllowMultipleArgumentsPerToken = true
    };

    public static readonly Option<string?> Mode = new Option<string?>(
        $"--{ModeName}",
        () => ModeTypes.NamespaceProxy,
        "Mode for the MCP server. 'single' exposes one azure tool that routes to all services. 'namespace' (default) exposes one tool per service namespace. 'all' exposes all tools individually."
    )
    {
        IsRequired = false,
        Arity = ArgumentArity.ZeroOrOne
    };

    public static readonly Option<bool?> ReadOnly = new(
        $"--{ReadOnlyName}",
        () => null,
        "Whether the MCP server should be read-only. If true, no write operations will be allowed.");
}
