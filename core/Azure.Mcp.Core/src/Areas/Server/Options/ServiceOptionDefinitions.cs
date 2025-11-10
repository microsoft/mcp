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
    public const string DangerouslyDisableHttpIncomingAuthName = "dangerously-disable-http-incoming-auth";
    public const string InsecureDisableElicitationName = "insecure-disable-elicitation";
    public const string OutgoingAuthStrategyName = "outgoing-auth-strategy";
    public const string LogLevelName = "log-level";
    public const string LogFilePathName = "log-file-path";

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

    public static readonly Option<bool> DangerouslyDisableHttpIncomingAuth = new(
        $"--{DangerouslyDisableHttpIncomingAuthName}")
    {
        Required = false,
        Description = "Dangerously disables HTTP incoming authentication, exposing the server to unauthenticated access over HTTP. Use with extreme caution, this disables all transport security and may expose sensitive data to interception.",
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

    public static readonly Option<string?> LogLevel = new(
        $"--{LogLevelName}")
    {
        Required = false,
        Description = "Minimum logging level. Valid values: Trace, Debug, Information, Warning, Error, Critical, None. Default is Information (or Debug if --debug is set).",
        DefaultValueFactory = _ => null
    };

    public static readonly Option<string?> LogFilePath = new(
        $"--{LogFilePathName}")
    {
        Required = false,
        Description = "Path to write log file output. When specified, logs will be written to the specified file in addition to console output.",
        DefaultValueFactory = _ => null
    };
}
