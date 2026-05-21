// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Options.Workflows;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.SreAgent.Commands.Workflows;

[CommandMetadata(Id = "6cb0ff68-d279-44bc-9935-84f0b181240b", Name = "generate", Title = "Generate Workflow YAML", Description = "Generate validated YAML for SRE Agent agents or tools.", Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, Secret = false, LocalRequired = false)]
public sealed class WorkflowsGenerateCommand(ILogger<WorkflowsGenerateCommand> logger) : GlobalCommand<WorkflowsGenerateOptions>
{
    private readonly ILogger<WorkflowsGenerateCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Kind);
        command.Options.Add(SreAgentOptionDefinitions.Name);
        command.Options.Add(SreAgentOptionDefinitions.Description.AsRequired());
        command.Options.Add(SreAgentOptionDefinitions.ModelOrType);
        command.Options.Add(SreAgentOptionDefinitions.Tools);
        command.Options.Add(SreAgentOptionDefinitions.Handoffs);
        command.Options.Add(SreAgentOptionDefinitions.Connector);
        command.Options.Add(SreAgentOptionDefinitions.Database);
        command.Options.Add(SreAgentOptionDefinitions.Query);
        command.Options.Add(SreAgentOptionDefinitions.UrlTemplate);
        command.Options.Add(SreAgentOptionDefinitions.ParametersList);
    }

    protected override WorkflowsGenerateOptions BindOptions(ParseResult parseResult)
    {
        var o = base.BindOptions(parseResult);
        o.Kind = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Kind.Name);
        o.Name = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Name.Name) ?? string.Empty;
        o.Description = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Description.Name);
        o.ModelOrType = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.ModelOrType.Name);
        o.Tools = parseResult.GetValueOrDefault<string[]>(SreAgentOptionDefinitions.Tools.Name);
        o.Handoffs = parseResult.GetValueOrDefault<string[]>(SreAgentOptionDefinitions.Handoffs.Name);
        o.Connector = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Connector.Name);
        o.Database = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Database.Name);
        o.Query = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Query.Name);
        o.UrlTemplate = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.UrlTemplate.Name);
        o.Parameters = parseResult.GetValueOrDefault<string[]>(SreAgentOptionDefinitions.ParametersList.Name);
        return o;
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        var o = BindOptions(parseResult);

        try
        {
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, Generate(o));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating workflow YAML");
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    private static string Generate(WorkflowsGenerateOptions o)
    {
        var safe = SreAgentPortedCommandHelpers.SanitizeKebabCase(o.Name);

        if (string.Equals(o.Kind, "agent", StringComparison.OrdinalIgnoreCase))
        {
            return GenerateAgent(safe, o.Description!, o.Tools, o.Handoffs);
        }

        var toolType = (o.ModelOrType ?? string.Empty).ToLowerInvariant() switch
        {
            "kusto" or "kustotool" => "KustoTool",
            "link" or "linktool" => "LinkTool",
            _ => o.ModelOrType ?? string.Empty
        };

        if (toolType is not ("KustoTool" or "LinkTool"))
        {
            return $"Error: Unsupported tool type '{toolType}'. Supported: KustoTool, LinkTool. Run list_stable_tool_types.";
        }

        var paramsYaml = BuildParameters(o.Parameters);
        return toolType == "KustoTool"
            ? GenerateKusto(safe, o.Description!, o.Connector, o.Database, o.Query, paramsYaml)
            : GenerateLink(safe, o.Description!, o.UrlTemplate, paramsYaml);
    }

    private static string BuildParameters(string[]? parameters)
    {
        if (parameters is null || parameters.Length == 0)
            return string.Empty;
        var lines = new List<string> { "  parameters:" };
        foreach (var param in parameters)
        {
            // Format: name:description (everything after the first ':' is the description so values containing ':' survive).
            var parts = param.Split(':', 2);
            var name = parts[0].Trim();
            var desc = parts.Length > 1 ? parts[1].Trim() : $"Parameter {name}";
            lines.Add($"    - name: {name}");
            lines.Add("      type: string");
            lines.Add($"      description: {desc}");
            lines.Add("      required: true");
            lines.Add("      target: dictionary:args:string");
        }
        return string.Join('\n', lines);
    }

    private static string GenerateAgent(string name, string description, string[]? tools, string[]? handoffs)
    {
        var toolsList = tools?.Length > 0
            ? "\n" + string.Join('\n', tools.Distinct().Select(t => $"    - {t}"))
            : "[]";
        var warnings = new List<string>();
        var handoffsList = handoffs?.Length > 0
            ? "\n" + string.Join('\n', handoffs.Distinct().Select(h => $"    - {SreAgentPortedCommandHelpers.SanitizeKebabCase(h)}"))
            : "[]";
        if (handoffs?.Length > 0)
        {
            warnings.Add($"Handoffs [{string.Join(", ", handoffs.Distinct())}] need agent YAML first.");
        }
        var yaml = $"api_version: azuresre.ai/v2\nkind: ExtendedAgent\nmetadata:\n  name: {name}\nspec:\n  instructions: |-\n    You are '{name}'. {description}\n\n    ## Responsibilities\n    - Analyze requests and context\n    - Use tools to gather information\n    - Provide clear, actionable responses\n  handoffDescription: '{description}'\n  handoffs: {handoffsList}\n  tools: {toolsList}\n  maxReflectionCount: 0\n  customReflectionNote: ''\n  commonPrompts: []\n  enableVanillaMode: false";
        var text = $"# Generated Agent YAML\n\n```yaml\n{yaml}\n```";
        return warnings.Count > 0
            ? text + $"\n\n## Warnings\n{string.Join('\n', warnings.Select(w => $"- {w}"))}"
            : text;
    }

    private static string GenerateKusto(string name, string description, string? connector, string? database, string? query, string paramsYaml)
    {
        if (string.IsNullOrWhiteSpace(connector))
        {
            return "Error: KustoTool requires 'connector'. Run `connectors -> list`";
        }

        if (string.IsNullOrWhiteSpace(database))
        {
            return "Error: KustoTool requires 'database'.";
        }

        var warnings = new List<string>();
        var queryYaml = "    // <REPLACE:your-kql-query>\n    // CRITICAL: Use ##paramName## for parameters (NOT {{paramName}})";
        if (!string.IsNullOrWhiteSpace(query))
        {
            queryYaml = string.Join('\n', query.Split('\n').Select(l => $"    {l}"));
        }
        else
        {
            warnings.Add("Query is empty. Add KQL before applying. Use ##param## format for placeholders.");
        }
        var yaml = $"api_version: azuresre.ai/v2\nkind: ExtendedAgentTool\nmetadata:\n  name: {name}\nspec:\n  type: KustoTool\n  connector: {connector}\n  toolMode: Auto\n  description: |\n    {description}\n  database: {database}\n  query: |\n{queryYaml}\n{paramsYaml}".Trim();
        var text = $"# Generated KustoTool YAML\n\n```yaml\n{yaml}\n```";
        return warnings.Count > 0
            ? text + $"\n\n## Warnings\n{string.Join('\n', warnings.Select(w => $"- {w}"))}"
            : text;
    }

    private static string GenerateLink(string name, string description, string? urlTemplate, string paramsYaml)
    {
        if (string.IsNullOrWhiteSpace(urlTemplate))
            return "Error: LinkTool requires 'urlTemplate'.";
        var yaml = $"api_version: azuresre.ai/v2\nkind: ExtendedAgentTool\nmetadata:\n  name: {name}\nspec:\n  type: LinkTool\n  toolMode: Auto\n  description: |\n    {description}\n  template: \"{urlTemplate}\"\n{paramsYaml}".Trim();
        return $"# Generated LinkTool YAML\n\n```yaml\n{yaml}\n```";
    }
}
