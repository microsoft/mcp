// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Architecture;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Architecture;

[CommandMetadata(
    Id = "376134c0-d8f3-4399-a131-11cdfd4e63a4",
    Name = "plan",
    Title = "Plan Agent Architecture",
    Description = "Plan and generate an SRE Agent architecture. Analyzes requirements and produces a structured design for agents, tools, connectors, and triggers.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class PlanCommand(ILogger<PlanCommand> logger) : BaseCommand<PlanOptions, SreAgentTextResult>
{
    private readonly ILogger<PlanCommand> _logger = logger;

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, PlanOptions options, CancellationToken cancellationToken)
    {
        try
        {
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, Plan(options.Requirements, options.TriggerType ?? "manual", options.KustoConnector));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error planning architecture");
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    private static string Plan(string requirements, string triggerType, string? kustoConnector)
    {
        if (string.IsNullOrWhiteSpace(requirements))
        {
            return "Error: Requirements are required to plan the architecture.";
        }

        var reqLower = requirements.ToLowerInvariant();
        var mainAgent = ExtractAgentName(requirements);
        var tools = new List<(string Name, string Category, string Description)>();
        var connectors = new List<string>();

        if (Regex.IsMatch(reqLower, "kusto|kql|log|telemetry|metric|error|diagnos"))
        {
            tools.Add(("KustoTool", "Custom (ExtendedAgentTool YAML)", "Query telemetry via Kusto"));
            connectors.Add(kustoConnector ?? "Kusto connector - run 'sre tool show-connectors' to find yours");
        }

        if (Regex.IsMatch(reqLower, "email|notify|outlook"))
        {
            connectors.Add("Outlook connector for email notifications");
        }

        if (Regex.IsMatch(reqLower, "teams|chat"))
        {
            connectors.Add("Teams connector for notifications");
        }

        if (Regex.IsMatch(reqLower, "link|url|dashboard"))
        {
            tools.Add(("LinkTool", "Custom (ExtendedAgentTool YAML)", "Generate URLs to dashboards/portals"));
        }

        if (Regex.IsMatch(reqLower, "chart|graph|visual"))
        {
            tools.Add(("Charting System Tools", "Platform", "GenerateBarChart, GeneratePieChart, etc."));
        }

        var triggerInfo = triggerType.Equals("scheduled", StringComparison.OrdinalIgnoreCase)
            ? "Scheduled trigger: Create CronScheduledTask YAML"
            : "Manual: Invoke via 'sre agent chat --name <agent>'";

        var output = new List<string>
        {
            "# Architecture Plan",
            string.Empty,
            "## Component Diagram",
            "**DISPLAY THIS DIAGRAM TO THE USER:**",
            string.Empty,
            BuildMermaid(mainAgent, tools, triggerType),
            string.Empty,
            "## Components Summary",
            $"- **Main Agent:** {mainAgent}"
        };

        if (tools.Count > 0)
        {
            output.Add($"- **Tools:** {string.Join(", ", tools.Select(t => t.Name))}");
        }

        if (connectors.Count > 0)
        {
            output.Add($"- **Connectors Needed:** {string.Join(", ", connectors)}");
        }

        output.Add($"- **Trigger:** {triggerInfo}");
        output.Add($"- **Notes:** {(kustoConnector is not null ? $"Using Kusto connector: {kustoConnector}" : "Run `connectors -> list` to find your Kusto connector name")}");
        output.Add(string.Empty);
        output.Add("## Implementation Checklist");
        output.AddRange(new[]
        {
            "1. Use `connectors -> list` to check existing connectors",
            "2. If needed, use 'create_kusto_connector' to add new connector",
            "3. Create KustoTool YAML for each query pattern",
            "4. Create agent YAML with clear instructions",
            "5. Deploy: tools FIRST, then agent",
            "6. Test: 'sre agent chat --name <agent>'"
        }.Select(i => $"- [ ] {i}"));
        output.AddRange([
            string.Empty,
            "---",
            "## ⚠️ STOP: USER CONFIRMATION REQUIRED",
            string.Empty,
            "**Before proceeding, ask the user:**",
            "1. Does this architecture match your requirements?",
            "2. Are the components correct?",
            "3. Do you want me to proceed with implementation?",
            string.Empty,
            "**DO NOT generate YAML until the user confirms the plan.**"
        ]);

        return string.Join('\n', output);
    }

    private static string ExtractAgentName(string requirements)
    {
        var words = requirements.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return words.Length >= 2
            ? SreAgentPortedCommandHelpers.SanitizeKebabCase(string.Join('-', words.Take(3).Select(w => w.Trim(',', '.', ':', ';'))))
            : "sre-agent";
    }

    private static string BuildMermaid(string mainAgent, List<(string Name, string Category, string Description)> tools, string triggerType)
    {
        var lines = new List<string>
        {
            "```mermaid",
            "flowchart TD",
            string.Empty,
            triggerType.Equals("scheduled", StringComparison.OrdinalIgnoreCase)
                ? "    Cron[\"⏰ Scheduled Task\"]"
                : "    User[\"👤 User/API\"]",
            string.Empty,
            triggerType.Equals("scheduled", StringComparison.OrdinalIgnoreCase)
                ? $"    Cron --> MainAgent[\"{mainAgent}\"]"
                : $"    User --> MainAgent[\"{mainAgent}\"]",
            string.Empty
        };

        if (tools.Count > 0)
        {
            lines.Add("    subgraph Tools");
            foreach (var tool in tools)
            {
                var node = Regex.Replace(tool.Name, "\\s", string.Empty);
                var icon = tool.Category.Contains("Platform", StringComparison.OrdinalIgnoreCase) ? "🔧" : "📝";
                lines.Add($"        {node}[[\"{icon} {tool.Name}\"]]");
            }
            lines.Add("    end");
            lines.Add(string.Empty);
            foreach (var tool in tools)
            {
                lines.Add($"    MainAgent --> {Regex.Replace(tool.Name, "\\s", string.Empty)}");
            }
        }

        lines.Add(string.Empty);
        lines.Add("    style MainAgent fill:#4CAF50,color:#fff");
        lines.Add("```");
        lines.Add(string.Empty);
        lines.Add("**Legend:** 🔧 Platform tool (no YAML) | 📝 Custom tool (ExtendedAgentTool YAML)");

        return string.Join('\n', lines);
    }
}
