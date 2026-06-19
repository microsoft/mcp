// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Workflows;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Workflows;

[CommandMetadata(
    Id = "7217d724-f07f-4e56-81bc-5e6e182fe987",
    Name = "apply",
    Title = "Apply Workflow YAML",
    Description = "Apply and deploy a YAML workflow to an SRE Agent. Uploads and activates ExtendedAgent or ExtendedAgentTool YAML configuration on the specified SRE Agent resource.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class WorkflowsApplyCommand(ILogger<WorkflowsApplyCommand> logger, ISreAgentService sreAgentService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<WorkflowsApplyOptions, SreAgentTextResult>(subscriptionResolver)
{
    private readonly ILogger<WorkflowsApplyCommand> _logger = logger;
    private readonly ISreAgentService _sreAgentService = sreAgentService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, WorkflowsApplyOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var yaml = options.YamlContent;
            if (string.IsNullOrWhiteSpace(yaml))
            {
                SreAgentPortedCommandHelpers.SetTextResult(context.Response, "Error: YAML content is empty");
                return context.Response;
            }
            var parsed = ParseMinimalYaml(yaml);
            var kind = parsed.Kind;
            var name = parsed.Name;
            if (string.IsNullOrWhiteSpace(kind))
            {
                SreAgentPortedCommandHelpers.SetTextResult(context.Response, "Error: YAML must contain a \"kind\" field (ExtendedAgent or ExtendedAgentTool)");
                return context.Response;
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                SreAgentPortedCommandHelpers.SetTextResult(context.Response, "Error: YAML must contain metadata.name");
                return context.Response;
            }
            if (kind != "ExtendedAgent" && kind != "ExtendedAgentTool")
            {
                SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"Error: Unsupported kind \"{kind}\". Expected ExtendedAgent or ExtendedAgentTool.");
                return context.Response;
            }

            var endpoint = await SreAgentCommandHelpers.ResolveAgentEndpointAsync(
                _sreAgentService,
                options,
                cancellationToken);
            var props = new Dictionary<string, JsonElement>();
            foreach (var kvp in parsed.Spec)
            {
                var raw = kvp.Value is null ? "null" : kvp.Value.ToJsonString();
                using var doc = JsonDocument.Parse(raw);
                props[kvp.Key] = doc.RootElement.Clone();
            }
            var tagList = parsed.Tags.Select(t => t?.ToString() ?? string.Empty).ToList();
            var payload = new ExtendedAgentResourceEnvelope
            {
                Name = name,
                Type = kind,
                Tags = tagList,
                Owner = parsed.Owner,
                Properties = props
            };
            await _sreAgentService.ApplyExtendedAgentResourceAsync(endpoint, kind, name!, payload, options.Tenant, cancellationToken);
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, $"✅ {(kind == "ExtendedAgentTool" ? "Tool" : "Agent")} '{name}' applied successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying workflow YAML");
            HandleException(context, ex);
        }
        return context.Response;
    }

    private static (string? Kind, string? Name, string? Owner, JsonArray Tags, JsonObject Spec) ParseMinimalYaml(string yaml)
    {
        string? kind = null;
        string? name = null;
        string? owner = null;
        var tags = new JsonArray();
        var spec = new JsonObject();
        string? section = null;
        string? currentKey = null;
        var block = new List<string>();
        var blockIsList = false;
        foreach (var rawLine in yaml.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.TrimEnd();
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;
            if (!char.IsWhiteSpace(line[0]))
            {
                FlushBlock(spec, ref currentKey, block, ref blockIsList);
                section = trimmed.TrimEnd(':');
                if (trimmed.StartsWith("kind:", StringComparison.OrdinalIgnoreCase))
                    kind = Clean(trimmed[5..]);
                continue;
            }
            if (section == "metadata")
            {
                if (trimmed.StartsWith("name:", StringComparison.OrdinalIgnoreCase))
                    name = Clean(trimmed[5..]);
                else if (trimmed.StartsWith("owner:", StringComparison.OrdinalIgnoreCase))
                    owner = Clean(trimmed[6..]);
                else if (trimmed.StartsWith('-'))
                    tags.Add((JsonNode?)JsonValue.Create(Clean(trimmed[1..])));
            }
            else if (section == "spec")
            {
                if (trimmed.StartsWith('-'))
                {
                    if (currentKey is not null)
                    {
                        blockIsList = true;
                        block.Add(Clean(trimmed[1..]));
                    }
                    continue;
                }
                var colon = trimmed.IndexOf(':');
                if (colon > 0)
                {
                    FlushBlock(spec, ref currentKey, block, ref blockIsList);
                    var key = trimmed[..colon].Trim();
                    var value = trimmed[(colon + 1)..].Trim();
                    if (value is "|" or "|-" or ">" or ">-")
                    {
                        currentKey = key;
                        blockIsList = false;
                    }
                    else if (value.Length == 0)
                    {
                        currentKey = key;
                        blockIsList = false;
                    }
                    else
                    {
                        spec[key] = Clean(value);
                    }
                }
                else if (currentKey is not null)
                    block.Add(trimmed);
            }
        }
        FlushBlock(spec, ref currentKey, block, ref blockIsList);
        return (kind, name, owner, tags, spec);
    }

    private static void FlushBlock(JsonObject spec, ref string? currentKey, List<string> block, ref bool blockIsList)
    {
        if (currentKey is null)
            return;
        if (blockIsList)
        {
            var arr = new JsonArray();
            foreach (var item in block)
                arr.Add((JsonNode?)JsonValue.Create(item));
            spec[currentKey] = arr;
        }
        else
        {
            spec[currentKey] = block.Count == 0 ? string.Empty : string.Join('\n', block);
        }
        currentKey = null;
        block.Clear();
        blockIsList = false;
    }

    private static string Clean(string value) => value.Trim().Trim('"', '\'');
}
