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
    public const string SkillAgentNameName = "agent-name";

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

    public static readonly Option<string> SkillAgentName = new($"--{SkillAgentNameName}")
    {
        Description = "Optional sub-agent name to assign the skill to."
    };
}
