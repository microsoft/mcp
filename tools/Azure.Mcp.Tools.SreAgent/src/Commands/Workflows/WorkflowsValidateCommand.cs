// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options.Workflows;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands.Workflows;

[CommandMetadata(
    Id = "a22bbea7-e805-4039-891c-ac570bb29c9f",
    Name = "validate",
    Title = "Validate Workflow YAML",
    Description = "Validate SRE Agent YAML content for common issues.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class WorkflowsValidateCommand(ILogger<WorkflowsValidateCommand> logger)
    : BaseCommand<WorkflowsValidateOptions, SreAgentTextResult>
{
    private readonly ILogger<WorkflowsValidateCommand> _logger = logger;

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, WorkflowsValidateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            SreAgentPortedCommandHelpers.SetTextResult(context.Response, ValidateYaml(options.YamlContent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating workflow YAML");
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
    internal static string ValidateYaml(string yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return "❌ Validation failed: YAML content is empty";
        }

        var errors = new List<string>();
        var warnings = new List<string>();

        if (!Regex.IsMatch(yaml, "^\\s*name\\s*:", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            && !Regex.IsMatch(yaml, "^\\s*metadata\\s*:\\s*\\r?\\n\\s*name\\s*:", RegexOptions.IgnoreCase | RegexOptions.Multiline))
        {
            errors.Add("Missing required name");
        }

        if (!yaml.Contains("instructions:", StringComparison.OrdinalIgnoreCase)
            && !yaml.Contains("systemPrompt:", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Missing instructions");
        }

        if (!yaml.Contains("model:", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("No model specified - server will use default");
        }

        if (!yaml.Contains("api_version:", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("No api_version - use 'azuresre.ai/v2' for new agents");
        }

        if (!yaml.Contains("tools:", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add("No tools defined - agent will only respond with text");
        }

        var connectorMatches = Regex.Matches(yaml, "^\\s*connector\\s*:\\s*(\\S+)", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            .Select(m => m.Groups[1].Value.Trim())
            .Distinct()
            .ToList();
        if (connectorMatches.Count > 0)
        {
            warnings.Add($"Connectors referenced: [{string.Join(", ", connectorMatches)}]. Run `connectors -> list` to verify access.");
        }

        var lines = errors.Count == 0
            ? new List<string> { "✅ YAML validation passed" }
            : ["❌ YAML validation failed", string.Empty, "## Errors", .. errors.Select(e => $"- {e}")];

        if (warnings.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("## Warnings");
            lines.AddRange(warnings.Select(w => $"- {w}"));
        }

        return string.Join('\n', lines);
    }
}
