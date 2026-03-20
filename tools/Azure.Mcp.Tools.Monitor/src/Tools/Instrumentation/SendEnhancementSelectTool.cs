// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Monitor.Generators;
using Azure.Mcp.Tools.Monitor.Models;

namespace Azure.Mcp.Tools.Monitor.Tools;

public class SendEnhancementSelectTool
{
    public string Send(string sessionId, string enhancementKeys)
    {
        if (!OrchestratorTool.Sessions.TryGetValue(sessionId, out var session))
        {
            return Respond(new OrchestratorResponse
            {
                Status = "error",
                Message = "No active session. Call orchestrator_start first.",
                Instruction = "Call orchestrator_start with the workspace path to begin."
            });
        }

        if (session.State != SessionState.AwaitingEnhancementSelection)
        {
            return Respond(new OrchestratorResponse
            {
                Status = "error",
                SessionId = sessionId,
                Message = "Session is not awaiting enhancement selection. This tool is only valid after orchestrator_start returns 'enhancement_available'.",
                Instruction = "Call orchestrator_next to continue with the current session."
            });
        }

        var keys = enhancementKeys
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var selections = new List<(string key, EnhancementOption option)>();
        foreach (var key in keys)
        {
            if (!DotNetEnhancementGenerator.SupportedEnhancements.TryGetValue(key, out var option))
            {
                var validKeys = string.Join(", ", DotNetEnhancementGenerator.SupportedEnhancements.Keys);
                return Respond(new OrchestratorResponse
                {
                    Status = "error",
                    SessionId = sessionId,
                    Message = $"Unknown enhancement key: '{key}'. This enhancement is not currently supported through MCP.",
                    Instruction = $"Valid options are: {validKeys}. Ask the user to choose from the supported list."
                });
            }

            selections.Add((key, option));
        }

        var spec = DotNetEnhancementGenerator.GenerateForSelections(session.Analysis, selections);
        session.Spec = spec;
        session.State = SessionState.Executing;

        if (spec.Actions.Count == 0)
        {
            var names = string.Join(", ", selections.Select(s => s.option.DisplayName));
            return Respond(new OrchestratorResponse
            {
                Status = "complete",
                SessionId = sessionId,
                Message = $"{names} - no actions needed.",
                Instruction = "Inform the user."
            });
        }

        var firstAction = spec.Actions[0];
        var displayNames = string.Join(" + ", selections.Select(s => s.option.DisplayName));

        return Respond(new OrchestratorResponse
        {
            Status = "in_progress",
            SessionId = sessionId,
            Message = $"Enhancement plan generated: {displayNames}.",
            Instruction = OrchestratorTool.BuildInstructionPublic(firstAction, spec.AgentMustExecuteFirst),
            CurrentAction = firstAction,
            Progress = $"Step 1 of {spec.Actions.Count}"
        });
    }

    private static string Respond(OrchestratorResponse response)
    {
        return JsonSerializer.Serialize(response, OnboardingJsonContext.Default.OrchestratorResponse);
    }
}
