// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Areas.Server.Options;

public static class ServiceOptionDefinitions
{
    public const string TransportName = "transport";
    public const string NamespaceName = "namespace";
    public const string ModeName = "mode";
    public const string ReadOnlyName = "read-only";
    public const string EnableInsecureTransportsName = "enable-insecure-transports";

    public static readonly Option<string> Transport = new($"--{TransportName}")
    {
        Description = "Transport mechanism to use for Azure MCP Server.",
        DefaultValueFactory = _ => TransportTypes.StdIo,
        Required = false
    };

    public static readonly Option<string[]?> Namespace = new(
        $"--{NamespaceName}"
    )
    {
        Description = "The Azure service namespaces to expose on the MCP server (e.g., storage, keyvault, cosmos).",
        Required = false,
        Arity = ArgumentArity.OneOrMore,
        AllowMultipleArgumentsPerToken = true,
        DefaultValueFactory = _ => null
    };

    public static readonly Option<string?> Mode = new Option<string?>(
        $"--{ModeName}"
    )
    {
        Description = "Mode for the MCP server. 'single' exposes one azure tool that routes to all services. 'namespace' (default) exposes one tool per service namespace. 'all' exposes all tools individually.",
        Required = false,
        Arity = ArgumentArity.ZeroOrOne,
        DefaultValueFactory = _ => (string?)ModeTypes.NamespaceProxy
    };

    public static readonly Option<bool?> ReadOnly = new(
        $"--{ReadOnlyName}")
    {
        Description = "Whether the MCP server should be read-only. If true, no write operations will be allowed.",
        DefaultValueFactory = _ => false
    };

    public static readonly Option<bool> EnableInsecureTransports = new(
        $"--{EnableInsecureTransportsName}")
    {
        Required = false,
        Hidden = true,
        Description = "Enable insecure transport",
        DefaultValueFactory = _ => false
    };
}
