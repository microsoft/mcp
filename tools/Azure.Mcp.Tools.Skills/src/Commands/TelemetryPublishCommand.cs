// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Skills.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Skills.Commands;

public sealed class TelemetryPublishCommand(ILogger<TelemetryPublishCommand> logger) : BaseCommand<TelemetryPublishOptions>
{
    private const string CommandTitle = "Publish Skills Telemetry";
    private readonly ILogger<TelemetryPublishCommand> _logger = logger;

    public override string Id => "b3e7c1a2-4f85-4d9e-a6c3-8f2b1e0d7a94";

    public override string Name => "publish";

    public override string Description =>
        "Publish skills-related telemetry events from agent hooks. " +
        "Accepts JSONL (JSON Lines) format - one JSON object per line - with fields such as 'timestamp', 'event_type', " +
        "'tool_name', and 'session_id'. Use this command from agent hooks in clients like VS Code, " +
        "Claude, or Copilot CLI to emit usage metrics.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        command.Options.Add(SkillsOptionDefinitions.Events);
    }

    protected override TelemetryPublishOptions BindOptions(ParseResult parseResult)
    {
        return new TelemetryPublishOptions
        {
            Events = parseResult.GetValueOrDefault<string>(SkillsOptionDefinitions.Events.Name)
        };
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        var options = BindOptions(parseResult);

        try
        {
            if (string.IsNullOrWhiteSpace(options.Events))
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = "The --events parameter is required and cannot be empty.";
                return Task.FromResult(context.Response);
            }

            // Parse JSONL format (one JSON object per line)
            var events = new List<JsonElement>();
            var lines = options.Events.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    continue;
                }

                try
                {
                    var evt = JsonSerializer.Deserialize(trimmedLine, SkillsJsonContext.Default.JsonElement);
                    events.Add(evt);
                }
                catch (JsonException ex)
                {
                    var preview = trimmedLine.Length > 50 ? trimmedLine[..50] + "..." : trimmedLine;
                    context.Response.Status = HttpStatusCode.BadRequest;
                    context.Response.Message = $"Invalid JSON in line: {preview} Error: {ex.Message}";
                    return Task.FromResult(context.Response);
                }
            }

            foreach (var evt in events)
            {
                _logger.LogInformation("SkillsTelemetry {Event}", evt.ToString());
            }

            var result = new TelemetryPublishResult(events.Count);
            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, SkillsJsonContext.Default.TelemetryPublishResult);
            context.Response.Message = string.Empty;

            context.Activity?.AddTag("Skills_EventCount", events.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing skills telemetry.");
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    internal record TelemetryPublishResult(int EventCount);
}
