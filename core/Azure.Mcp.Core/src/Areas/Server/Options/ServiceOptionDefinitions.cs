// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Areas.Server.Options;

public static class ServiceOptionDefinitions
{
    public const string TransportName = "transport";
    public const string NamespaceName = "namespace";
    public const string ModeName = "mode";
    public const string ToolName = "tool";
    public const string ReadOnlyName = "read-only";
    public const string DebugName = "debug";
    public const string EnableInsecureTransportsName = "enable-insecure-transports";
    public const string InsecureDisableElicitationName = "insecure-disable-elicitation";
    public const string OutgoingAuthStrategyName = "outgoing-auth-strategy";

    public static readonly Option<TransportTypes> Transport = new($"--{TransportName}")
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

    public static readonly Option<string[]?> Tool = new Option<string[]?>(
        $"--{ToolName}"
    )
    {
        Description = "Expose only specific tools by name (e.g., 'acr_registry_list'). Repeat this option to include multiple tools, e.g., --tool \"acr_registry_list\" --tool \"group_list\". It automatically switches to \"all\" mode when \"--tool\" is used. It can't be used together with \"--namespace\".",
        Required = false,
        Arity = ArgumentArity.OneOrMore,
        AllowMultipleArgumentsPerToken = true,
        DefaultValueFactory = _ => null
    };

    public static readonly Option<bool?> ReadOnly = new(
        $"--{ReadOnlyName}")
    {
        Description = "Whether the MCP server should be read-only. If true, no write operations will be allowed.",
        DefaultValueFactory = _ => false
    };

    public static readonly Option<bool> Debug = new(
        $"--{DebugName}")
    {
        Description = "Enable debug mode with verbose logging to stderr.",
        DefaultValueFactory = _ => false
    };

    public static readonly Option<bool> EnableInsecureTransports = new(
        $"--{EnableInsecureTransportsName}")
    {
        Required = false,
        Hidden = true,
        Description = "Enables insecure, unauthenticated transport over streamable HTTP. Use with extreme caution, this disables all transport security and may expose sensitive data to interception.",
        DefaultValueFactory = _ => false
    };

    public static readonly Option<bool> InsecureDisableElicitation = new(
        $"--{InsecureDisableElicitationName}")
    {
        Required = false,
        Description = "Disable elicitation (user confirmation) before allowing high risk commands to run, such as returning Secrets (passwords) from KeyVault.",
        DefaultValueFactory = _ => false
    };

    public static readonly Option<OutgoingAuthStrategy> OutgoingAuthStrategy = new(
        $"--{OutgoingAuthStrategyName}")
    {
        Required = false,
        Description = "Outgoing authentication strategy for Azure service requests. Valid values: NotSet, UseHostingEnvironmentIdentity, UseOnBehalfOf.",
        DefaultValueFactory = _ => Options.OutgoingAuthStrategy.NotSet
    };
}
