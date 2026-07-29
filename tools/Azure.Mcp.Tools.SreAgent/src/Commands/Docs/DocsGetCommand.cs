// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Docs;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Docs;

[CommandMetadata(
    Id = "3e7977a8-786b-4db9-a3be-2194af3cdafb",
    Name = "get",
    Title = "Get SRE Agent Documentation",
    Description = "Return reference documentation for SRE Agent concepts.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DocsGetCommand(ILogger<DocsGetCommand> logger)
    : BaseCommand<DocsGetOptions, SreAgentTextResult>
{
    private readonly ILogger<DocsGetCommand> _logger = logger;

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, DocsGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, Resolve(options.Topic.ToLowerInvariant()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documentation");
            HandleException(context, ex);
        }
        return Task.FromResult(context.Response);
    }
    private static string Resolve(string topic) => topic switch
    {
        "overview" => Overview,
        "architecture" or "trigger-flow" or "flow" => Architecture,
        "agents" or "agent" or "subagents" or "handoffs" or "subagent" => Agents,
        "tools" or "tool" or "tool-types" => Tools,
        "triggers" or "trigger" => Triggers,
        "scheduled-tasks" or "scheduled" => ScheduledTasks,
        "yaml-schema" or "schema" => YamlSchema,
        "connectors" or "connector" => Connectors,
        "quickstart" => Quickstart,
        "workflows" or "workflow" => Workflows,
        "all" => string.Join("\n\n---\n\n", [Overview, Architecture, Agents, Tools, YamlSchema, Connectors, Quickstart, Workflows]),
        _ => $"Unknown topic: {topic}. Available: overview, architecture, agents, tools, tool-types, triggers, subagents, connectors, yaml-schema, quickstart, workflows, scheduled-tasks, all"
    };
    private const string Overview = """
# SRE Agent Platform Overview

AI-powered platform for Site Reliability Engineering. Automates incident response, monitoring, and operational tasks.

## Core Concepts

**Runtime Flow**: Trigger -> Agent -> Tools -> Connectors

**Deployment Order**: Tools -> Agents -> Triggers (always deploy dependencies first)

| Concept | Description |
|---------|-------------|
| **Connectors** | Pre-authenticated endpoints (Kusto, Outlook, Teams, MCP) |
| **Tools** | Query/action definitions - System tools (platform) or Custom (KustoTool, LinkTool) |
| **Agents** | AI orchestration with instructions, tools, and optional handoffs |
| **Triggers** | Events that invoke agents (scheduled tasks, manual, API) |
| **Handoffs** | Agents delegating to other agents (use sparingly) |
""";
    private const string Architecture = """
# SRE Agent Architecture

```
TRIGGER -> AGENT -> TOOLS -> CONNECTORS
Examples: Scheduled Task -> Agent instructions -> KustoTool/LinkTool/SystemTools -> Kusto/Outlook/Teams/MCP
```

## Component Hierarchy

### Connectors
Pre-authenticated endpoints: Kusto, Outlook, Teams, MCP.

### Tools
System tools are platform-provided. Custom tools are ExtendedAgentTool YAML: KustoTool and LinkTool.

### Agents
LLM orchestration with tools and instructions. Model is fixed by the platform. Agents may hand off to other agents sparingly.

### Triggers
CronScheduledTask or manual/API invocation.
""";
    private const string Agents = """
# Agent YAML Schema

```yaml
api_version: azuresre.ai/v2
kind: ExtendedAgent
metadata:
  name: my-agent
spec:
  instructions: |-
    Clear, specific instructions for the agent.
  handoffDescription: 'One-line description for routing'
  handoffs: []
  tools: []
  maxReflectionCount: 0
  customReflectionNote: ''
  commonPrompts: []
  enableVanillaMode: false
```

Required: api_version, kind, metadata.name, spec.instructions, spec.handoffDescription, spec.handoffs.
""";
    private const string Tools = """
# Custom Tool Types (ExtendedAgentTool)

CRITICAL: Use `##paramName##` for parameter placeholders. Include `target: dictionary:args:string` and `type: string` for every parameter.

## KustoTool
Requires connector, database, description, query, and optional parameters.

## LinkTool
Requires template and description.
""";
    private const string Connectors = """
# Connectors

Connectors are pre-authenticated endpoints managed by the platform: Kusto, Outlook, Teams, and MCP.
Use `connectors` with `action: list` to see available connectors.
""";
    private const string YamlSchema = """
# YAML Schema Reference (V2)

All YAML uses `api_version: azuresre.ai/v2`.

Agent kind: ExtendedAgent. Tool kind: ExtendedAgentTool. KustoTool parameters must use `##param##`, `type: string`, and `target: dictionary:args:string`.
""";
    private const string Triggers = """
# Triggers

Triggers invoke agents automatically.

Cron format: `* * * * *` (minute, hour, day of month, month, day of week). Examples: `0 * * * *`, `0 8 * * *`, `0 */6 * * *`.
""";
    private const string ScheduledTasks = """
# Scheduled Tasks

Run agents on a schedule. Deploy custom tools first, agents second, scheduled tasks last.
""";
    private const string Quickstart = """
# Quick Start

1. Plan architecture and get user confirmation.
2. Check/create connectors.
3. Generate YAML and get user confirmation.
4. Deploy and test.

Always deploy tools before agents.
""";
    private const string Workflows = """
# Deployment Workflow

## Design First, Deploy Second

1. Understand requirements.
2. List system and custom tools.
3. Define agent instructions.
4. Get confirmation.
5. Generate YAML using `##param##` placeholders.
6. Deploy tools first, agents second, scheduled tasks last.
""";
}


