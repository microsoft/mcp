// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options;

public static class SreAgentOptionDefinitions
{
    public const string AgentNameName = "agent";
    public const string NameName = "name";
    public const string ClusterUrlName = "cluster-url";
    public const string DatabaseName = "database";
    public const string TypeName = "type";
    public const string CommandName = "command";
    public const string ArgsName = "args";
    public const string EnvsJsonName = "envs-json";
    public const string EndpointName = "endpoint";
    public const string AuthTypeName = "auth-type";
    public const string BearerTokenEnvName = "bearer-token-env";
    public const string HeadersJsonName = "headers-json";
    public const string ThreadIdName = "thread-id";
    public const string HookNameName = "hook-name";

    public static readonly Option<string> Agent = new(
        $"--{AgentNameName}"
    )
    {
        Description = "The name of the Azure SRE Agent resource to target."
    };
    public static readonly Option<string> Name = new(
        $"--{NameName}"
    )
    {
        Description = "The connector or hook name."
    };

    public static readonly Option<string> ClusterUrl = new(
        $"--{ClusterUrlName}"
    )
    {
        Description = "The Azure Data Explorer cluster URL."
    };

    public static readonly Option<string> Database = new(
        $"--{DatabaseName}"
    )
    {
        Description = "The default Kusto database name."
    };

    public static readonly Option<string> Type = new(
        $"--{TypeName}"
    )
    {
        Description = "The MCP connector type: stdio or http."
    };

    public static readonly Option<string> Command = new(
        $"--{CommandName}"
    )
    {
        Description = "The command for stdio MCP connectors."
    };

    public static readonly Option<string[]> Args = new(
        $"--{ArgsName}"
    )
    {
        Description = "Arguments for stdio MCP connectors.",
        Arity = ArgumentArity.ZeroOrMore
    };

    public static readonly Option<string> EnvsJson = new(
        $"--{EnvsJsonName}"
    )
    {
        Description = "JSON object of environment variables for stdio MCP connectors."
    };

    public static readonly Option<string> Endpoint = new(
        $"--{EndpointName}"
    )
    {
        Description = "The HTTP MCP connector endpoint."
    };

    public static readonly Option<string> AuthType = new(
        $"--{AuthTypeName}"
    )
    {
        Description = "The HTTP MCP connector authentication type."
    };

    public static readonly Option<string> BearerTokenEnv = new(
        $"--{BearerTokenEnvName}"
    )
    {
        Description = "Environment variable containing the bearer token."
    };

    public static readonly Option<string> HeadersJson = new(
        $"--{HeadersJsonName}"
    )
    {
        Description = "JSON object of HTTP headers."
    };

    public static readonly Option<string> ThreadId = new(
        $"--{ThreadIdName}"
    )
    {
        Description = "The SRE Agent thread ID."
    };

    public static readonly Option<string> HookName = new(
        $"--{HookNameName}"
    )
    {
        Description = "The hook name."
    };
}
