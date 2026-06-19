// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options;

public static class SreAgentOptionDefinitions
{
    public const string AgentNameName = "agent";
    public const string NameName = "name";
    public const string DescriptionName = "description";
    public const string ToolsName = "tools";
    public const string HandoffsName = "handoffs";
    public const string ConfirmName = "confirm";
    public const string ConnectorName = "connector";
    public const string DatabaseName = "database";
    public const string QueryName = "query";
    public const string UrlTemplateName = "url-template";
    public const string ContentName = "content";
    public const string AuthTypeName = "auth-type";
    public const string TimeoutSecondsName = "timeout-seconds";
    public const string HeadersJsonName = "headers-json";
    public const string MessageName = "message";
    public const string ThreadIdName = "thread-id";
    public const string TaskIdName = "task-id";
    public const string EnvsJsonName = "envs-json";
    public const string MaxIterationsName = "max-iterations";
    public const string CronExpressionName = "cron-expression";

    internal const string AgentDescription = "The name of the Azure SRE Agent resource to target.";
    public static readonly Option<string> Agent = new(
        $"--{AgentNameName}"
    )
    {
        Description = "The name of the Azure SRE Agent resource to target."
    };

    internal const string NameDescription = "The name of the SRE Agent item.";
    public static readonly Option<string> Name = new($"--{NameName}")
    {
        Description = "The name of the SRE Agent item.",
        Required = true
    };

    internal const string DescriptionDescription = "A description for the SRE Agent item.";
    public static readonly Option<string> Description = new($"--{DescriptionName}")
    {
        Description = "A description for the SRE Agent item."
    };

    internal const string ToolsDescription = "Tool names to attach. Multiple values are supported.";
    public static readonly Option<string[]> Tools = new($"--{ToolsName}")
    {
        Description = "Tool names to attach. Multiple values are supported.",
        AllowMultipleArgumentsPerToken = true
    };

    internal const string HandoffsDescription = "Sub-agent handoff names. Multiple values are supported.";
    public static readonly Option<string[]> Handoffs = new($"--{HandoffsName}")
    {
        Description = "Sub-agent handoff names. Multiple values are supported.",
        AllowMultipleArgumentsPerToken = true
    };

    internal const string ConfirmDescription = "Confirm a destructive operation.";
    public static readonly Option<bool> Confirm = new($"--{ConfirmName}")
    {
        Description = "Confirm a destructive operation."
    };

    internal const string ConnectorDescription = "The connector name for Kusto tools.";
    public static readonly Option<string> Connector = new($"--{ConnectorName}")
    {
        Description = "The connector name for Kusto tools."
    };

    internal const string DatabaseDescription = "The Kusto database for Kusto tools.";
    public static readonly Option<string> Database = new($"--{DatabaseName}")
    {
        Description = "The Kusto database for Kusto tools."
    };

    internal const string QueryDescription = "The Kusto query for Kusto tools.";
    public static readonly Option<string> Query = new($"--{QueryName}")
    {
        Description = "The Kusto query for Kusto tools."
    };

    internal const string UrlTemplateDescription = "The URL template for link tools.";
    public static readonly Option<string> UrlTemplate = new($"--{UrlTemplateName}")
    {
        Description = "The URL template for link tools."
    };

    internal const string ContentDescription = "Skill content.";
    public static readonly Option<string> Content = new($"--{ContentName}")
    {
        Description = "Skill content.",
        Required = true
    };

    internal const string AuthTypeDescription = "The HTTP MCP connector authentication type.";

    internal const string ThreadIdDescription = "The SRE Agent thread ID.";
    public static readonly Option<string> ThreadId = new(
            $"--{ThreadIdName}"
        )
    {
        Description = "The SRE Agent thread ID."
    };

    internal const string HookNameDescription = "The hook name.";

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
    public const string ApiKeyEnvName = "api-key-env";
    public const string SubdomainName = "subdomain";
    public const string InstanceUrlName = "instance-url";
    public const string TitleName = "title";
    public const string KindName = "kind";
    public const string ModelOrTypeName = "model-or-type";
    public const string YamlContentName = "yaml-content";
    public const string SourceNameName = "source-name";

    internal const string SeverityDescription = "Incident severity: critical, high, medium, or low.";
    internal const string ServicesDescription = "Affected service names.";

    public static readonly Option<string> Kind = new($"--{KindName}") { Description = "YAML kind: agent or tool.", Required = true };
    public static readonly Option<string?> ModelOrType = new($"--{ModelOrTypeName}") { Description = "Tool type, such as KustoTool or LinkTool." };
    public static readonly Option<string> YamlContent = new($"--{YamlContentName}") { Description = "YAML content.", Required = true };
    public static readonly Option<string?> SourceName = new($"--{SourceNameName}") { Description = "Optional source name." };

    public const string ParametersListName = "parameters";
    public static readonly Option<string[]> ParametersList = new($"--{ParametersListName}") { Description = "Parameters as name:description.", AllowMultipleArgumentsPerToken = true };
}
