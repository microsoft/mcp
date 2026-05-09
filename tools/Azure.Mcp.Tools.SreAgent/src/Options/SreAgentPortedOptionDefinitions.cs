// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options;

public static partial class SreAgentPortedOptionDefinitions
{
    public const string NameName = "name";
    public const string DescriptionName = "description";
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
    public const string AuthTypeName = "auth-type";
    public const string TokenEnvName = "token-env";
    public const string UsernameEnvName = "username-env";
    public const string PasswordEnvName = "password-env";
    public const string TitleName = "title";
    public const string KindName = "kind";
    public const string ModelOrTypeName = "model-or-type";
    public const string ToolsName = "tools";
    public const string HandoffsName = "handoffs";
    public const string ConnectorName = "connector";
    public const string DatabaseName = "database";
    public const string QueryName = "query";
    public const string UrlTemplateName = "url-template";
    public const string ParametersName = "parameters";
    public const string YamlContentName = "yaml-content";
    public const string SourceNameName = "source-name";
    public const string TopicName = "topic";
    public const string ContentName = "content";
    public const string ConfirmName = "confirm";
    public const string RequirementsName = "requirements";
    public const string TriggerTypeName = "trigger-type";
    public const string KustoConnectorName = "kusto-connector";
    public const string SearchName = "search";

    public static readonly Option<string> Agent = new($"--{SreAgentOptionDefinitions.AgentNameName}") { Description = "The SRE Agent resource name.", Required = true };
    public static readonly Option<string> Name = new($"--{NameName}") { Description = "Name for the resource.", Required = true };
    public static readonly Option<string> Description = new($"--{DescriptionName}") { Description = "Description text.", Required = true };
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
    public static readonly Option<string> AuthType = new($"--{AuthTypeName}") { Description = "Authentication type: BearerToken or BasicAuth.", Required = true };
    public static readonly Option<string?> TokenEnv = new($"--{TokenEnvName}") { Description = "Environment variable containing bearer token." };
    public static readonly Option<string?> UsernameEnv = new($"--{UsernameEnvName}") { Description = "Environment variable containing username." };
    public static readonly Option<string?> PasswordEnv = new($"--{PasswordEnvName}") { Description = "Environment variable containing password." };
    public static readonly Option<string> Title = new($"--{TitleName}") { Description = "Incident title.", Required = true };
    public static readonly Option<string> Kind = new($"--{KindName}") { Description = "YAML kind: agent or tool.", Required = true };
    public static readonly Option<string?> ModelOrType = new($"--{ModelOrTypeName}") { Description = "Tool type, such as KustoTool or LinkTool." };
    public static readonly Option<string[]> Tools = new($"--{ToolsName}") { Description = "Tool names.", AllowMultipleArgumentsPerToken = true };
    public static readonly Option<string[]> Handoffs = new($"--{HandoffsName}") { Description = "Handoff agent names.", AllowMultipleArgumentsPerToken = true };
    public static readonly Option<string?> Connector = new($"--{ConnectorName}") { Description = "Connector name." };
    public static readonly Option<string?> Database = new($"--{DatabaseName}") { Description = "Database name." };
    public static readonly Option<string?> Query = new($"--{QueryName}") { Description = "Kusto query." };
    public static readonly Option<string?> UrlTemplate = new($"--{UrlTemplateName}") { Description = "URL template." };
    public static readonly Option<string[]> Parameters = new($"--{ParametersName}") { Description = "Parameters as name:description.", AllowMultipleArgumentsPerToken = true };
    public static readonly Option<string> YamlContent = new($"--{YamlContentName}") { Description = "YAML content.", Required = true };
    public static readonly Option<string?> SourceName = new($"--{SourceNameName}") { Description = "Optional source name." };
    public static readonly Option<string> Topic = new($"--{TopicName}") { Description = "Documentation topic.", Required = true };
    public static readonly Option<string> Content = new($"--{ContentName}") { Description = "Content text.", Required = true };
    public static readonly Option<bool> Confirm = new($"--{ConfirmName}") { Description = "Confirm destructive operation." };
    public static readonly Option<string> Requirements = new($"--{RequirementsName}") { Description = "Architecture requirements.", Required = true };
    public static readonly Option<string?> TriggerType = new($"--{TriggerTypeName}") { Description = "Trigger type, such as manual or scheduled." };
    public static readonly Option<string?> KustoConnector = new($"--{KustoConnectorName}") { Description = "Kusto connector name." };
    public static readonly Option<string?> Search = new($"--{SearchName}") { Description = "Optional search filter." };
}
