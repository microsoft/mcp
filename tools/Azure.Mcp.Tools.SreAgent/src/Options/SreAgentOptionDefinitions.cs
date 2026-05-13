// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options;

public static class SreAgentOptionDefinitions
{
    public const string AgentNameName = "agent";
    public const string NameName = "name";
    public const string DescriptionName = "description";
    public const string InstructionsName = "instructions";
    public const string ToolsName = "tools";
    public const string HandoffsName = "handoffs";
    public const string ConfirmName = "confirm";
    public const string ToolTypeName = "tool-type";
    public const string ConnectorName = "connector";
    public const string DatabaseName = "database";
    public const string QueryName = "query";
    public const string UrlTemplateName = "url-template";
    public const string ParametersName = "parameters";
    public const string ContentName = "content";
    public const string AuthTypeName = "auth-type";
    public const string CommandName = "command";
    public const string ClusterUrlName = "cluster-url";
    public const string TimeoutSecondsName = "timeout-seconds";
    public const string HeadersJsonName = "headers-json";
    public const string EndpointName = "endpoint";
    public const string MessageName = "message";
    public const string ThreadIdName = "thread-id";
    public const string TaskIdName = "task-id";
    public const string TypeName = "type";
    public const string EnvsJsonName = "envs-json";
    public const string BearerTokenEnvName = "bearer-token-env";
    public const string ArgsName = "args";
    public const string HookNameName = "hook-name";
    public const string MaxIterationsName = "max-iterations";
    public const string CronExpressionName = "cron-expression";

    public static readonly Option<string> Agent = new(
        $"--{AgentNameName}"
    )
    {
        Description = "The name of the Azure SRE Agent resource to target."
    };

    public static readonly Option<string> Name = new($"--{NameName}")
    {
        Description = "The name of the SRE Agent item.",
        Required = true
    };

    public static readonly Option<string> Description = new($"--{DescriptionName}")
    {
        Description = "A description for the SRE Agent item."
    };

    public static readonly Option<string> Instructions = new($"--{InstructionsName}")
    {
        Description = "Instructions for the sub-agent."
    };

    public static readonly Option<string[]> Tools = new($"--{ToolsName}")
    {
        Description = "Tool names to attach. Multiple values are supported.",
        AllowMultipleArgumentsPerToken = true
    };

    public static readonly Option<string[]> Handoffs = new($"--{HandoffsName}")
    {
        Description = "Sub-agent handoff names. Multiple values are supported.",
        AllowMultipleArgumentsPerToken = true
    };

    public static readonly Option<bool> Confirm = new($"--{ConfirmName}")
    {
        Description = "Confirm a destructive operation."
    };

    public static readonly Option<string> ToolType = new($"--{ToolTypeName}")
    {
        Description = "The custom tool type, such as KustoTool or LinkTool.",
        Required = true
    };

    public static readonly Option<string> Connector = new($"--{ConnectorName}")
    {
        Description = "The connector name for Kusto tools."
    };

    public static readonly Option<string> Database = new($"--{DatabaseName}")
    {
        Description = "The Kusto database for Kusto tools."
    };

    public static readonly Option<string> Query = new($"--{QueryName}")
    {
        Description = "The Kusto query for Kusto tools."
    };

    public static readonly Option<string> UrlTemplate = new($"--{UrlTemplateName}")
    {
        Description = "The URL template for link tools."
    };

    public static readonly Option<string> Parameters = new($"--{ParametersName}")
    {
        Description = "JSON array of tool parameter definitions."
    };

    public static readonly Option<string> Content = new($"--{ContentName}")
    {
        Description = "Skill content.",
        Required = true
    };

    // B
    public static readonly Option<string> ClusterUrl = new(
            $"--{ClusterUrlName}"
        )
    {
        Description = "The Azure Data Explorer cluster URL."
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

    // C
    public static readonly Option<string> TaskId = new($"--{TaskIdName}")
    {
        Description = "The scheduled task ID.",
        Required = true
    };
    public static readonly Option<string> Message = new($"--{MessageName}")
    {
        Description = "The message to send.",
        Required = true
    };
    public static readonly Option<string> CronExpression = new($"--{CronExpressionName}")
    {
        Description = "The cron expression for the schedule.",
        Required = true
    };
    public static readonly Option<int> MaxIterations = new($"--{MaxIterationsName}")
    {
        Description = "The maximum number of automatic follow-up iterations.",
        Required = false,
        DefaultValueFactory = _ => 20
    };
    public static readonly Option<int> TimeoutSeconds = new($"--{TimeoutSecondsName}")
    {
        Description = "The investigation timeout in seconds.",
        Required = false,
        DefaultValueFactory = _ => 600
    };

    // D - Ported (merged from SreAgentPortedOptionDefinitions)
    public const string SeverityName = "severity";
    public const string TriggerConditionName = "trigger-condition";
    public const string ServicesName = "services";
    public const string StepsName = "steps";
    public const string EscalationName = "escalation";
    public const string RunbookUrlName = "runbook-url";
    public const string AgentModeName = "agent-mode";
    public const string ApiKeyEnvName = "api-key-env";
    public const string SubdomainName = "subdomain";
    public const string InstanceUrlName = "instance-url";
    public const string TokenEnvName = "token-env";
    public const string UsernameEnvName = "username-env";
    public const string PasswordEnvName = "password-env";
    public const string TitleName = "title";
    public const string KindName = "kind";
    public const string ModelOrTypeName = "model-or-type";
    public const string YamlContentName = "yaml-content";
    public const string SourceNameName = "source-name";
    public const string TopicName = "topic";
    public const string RequirementsName = "requirements";
    public const string TriggerTypeName = "trigger-type";
    public const string KustoConnectorName = "kusto-connector";
    public const string SearchName = "search";

    public static readonly Option<string> Severity = new($"--{SeverityName}") { Description = "Incident severity: critical, high, medium, or low.", Required = true };
    public static readonly Option<string> TriggerCondition = new($"--{TriggerConditionName}") { Description = "Text that triggers the incident response plan.", Required = true };
    public static readonly Option<string[]> Services = new($"--{ServicesName}") { Description = "Affected service names.", Required = true, AllowMultipleArgumentsPerToken = true };
    public static readonly Option<string[]> Steps = new($"--{StepsName}") { Description = "Incident response steps.", Required = true, AllowMultipleArgumentsPerToken = true };
    public static readonly Option<string?> Escalation = new($"--{EscalationName}") { Description = "Escalation procedure." };
    public static readonly Option<string?> RunbookUrl = new($"--{RunbookUrlName}") { Description = "Runbook URL." };
    public static readonly Option<string?> AgentMode = new($"--{AgentModeName}") { Description = "Agent mode: autonomous or review." };
    public static readonly Option<string> ApiKeyEnv = new($"--{ApiKeyEnvName}") { Description = "Environment variable containing the API key.", Required = true };
    public static readonly Option<string?> Subdomain = new($"--{SubdomainName}") { Description = "PagerDuty subdomain." };
    public static readonly Option<string> InstanceUrl = new($"--{InstanceUrlName}") { Description = "ServiceNow instance URL.", Required = true };
    public static readonly Option<string?> TokenEnv = new($"--{TokenEnvName}") { Description = "Environment variable containing bearer token." };
    public static readonly Option<string?> UsernameEnv = new($"--{UsernameEnvName}") { Description = "Environment variable containing username." };
    public static readonly Option<string?> PasswordEnv = new($"--{PasswordEnvName}") { Description = "Environment variable containing password." };
    public static readonly Option<string> Title = new($"--{TitleName}") { Description = "Incident title.", Required = true };
    public static readonly Option<string> Kind = new($"--{KindName}") { Description = "YAML kind: agent or tool.", Required = true };
    public static readonly Option<string?> ModelOrType = new($"--{ModelOrTypeName}") { Description = "Tool type, such as KustoTool or LinkTool." };
    public static readonly Option<string> YamlContent = new($"--{YamlContentName}") { Description = "YAML content.", Required = true };
    public static readonly Option<string?> SourceName = new($"--{SourceNameName}") { Description = "Optional source name." };
    public static readonly Option<string> Topic = new($"--{TopicName}") { Description = "Documentation topic.", Required = true };
    public static readonly Option<string> Requirements = new($"--{RequirementsName}") { Description = "Architecture requirements.", Required = true };
    public static readonly Option<string?> TriggerType = new($"--{TriggerTypeName}") { Description = "Trigger type, such as manual or scheduled." };
    public static readonly Option<string?> KustoConnector = new($"--{KustoConnectorName}") { Description = "Kusto connector name." };
    public static readonly Option<string?> Search = new($"--{SearchName}") { Description = "Optional search filter." };

    public const string ParametersListName = "parameters";
    public static readonly Option<string[]> ParametersList = new($"--{ParametersListName}") { Description = "Parameters as name:description.", AllowMultipleArgumentsPerToken = true };
}
