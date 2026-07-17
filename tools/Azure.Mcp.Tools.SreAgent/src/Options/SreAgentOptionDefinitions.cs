// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options;

public static class SreAgentOptionDefinitions
{
    public const string AgentNameName = "agent";
    public const string NameName = "name";
    public const string DescriptionName = "description";
    public const string ConfirmName = "confirm";
    public const string AuthTypeName = "auth-type";
    public const string HeadersJsonName = "headers-json";
    public const string MessageName = "message";
    public const string TaskIdName = "task-id";
    public const string EnvsJsonName = "envs-json";
    public const string CronExpressionName = "cron-expression";
    public const string SeverityName = "severity";
    public const string TriggerConditionName = "trigger-condition";
    public const string ServicesName = "services";
    public const string StepsName = "steps";
    public const string ApiKeyEnvName = "api-key-env";
    public const string SubdomainName = "subdomain";
    public const string InstanceUrlName = "instance-url";
    public const string TitleName = "title";
    public const string KindName = "kind";
    public const string YamlContentName = "yaml-content";
    public const string SourceNameName = "source-name";

    internal const string AgentDescription = "The name of the Azure SRE Agent resource to target.";
    internal const string NameDescription = "The name of the SRE Agent item.";
    internal const string DescriptionDescription = "A description for the SRE Agent item.";
    internal const string ToolsDescription = "Tool names to attach. Multiple values are supported.";
    internal const string HandoffsDescription = "Sub-agent handoff names. Multiple values are supported.";
    internal const string ConfirmDescription = "Confirm a destructive operation.";
    internal const string ConnectorDescription = "The connector name for Kusto tools.";
    internal const string DatabaseDescription = "The Kusto database for Kusto tools.";
    internal const string QueryDescription = "The Kusto query for Kusto tools.";
    internal const string UrlTemplateDescription = "The URL template for link tools.";
    internal const string ContentDescription = "Skill content.";
    internal const string AuthTypeDescription = "The HTTP MCP connector authentication type.";
    internal const string ThreadIdDescription = "The SRE Agent thread ID.";
    internal const string HookNameDescription = "The hook name.";
    internal const string TaskIdDescription = "The scheduled task ID.";
    internal const string MessageDescription = "The message to send.";
    internal const string MaxIterationsDescription = "The maximum number of automatic follow-up iterations.";
    internal const string TimeoutSecondsDescription = "The investigation timeout in seconds.";
    internal const string SeverityDescription = "Incident severity: critical, high, medium, or low.";
    internal const string ServicesDescription = "Affected service names.";
    internal const string YamlContentDescription = "YAML content.";
}
