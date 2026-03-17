using System.Collections.Concurrent;
using System.Text.Json;
using Azure.Mcp.Tools.MonitorInstrumentation.Models;
using Azure.Mcp.Tools.MonitorInstrumentation.Pipeline;
using static Azure.Mcp.Tools.MonitorInstrumentation.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Tools;

/// <summary>
/// Single entry point for Azure Monitor instrumentation.
/// Controls the entire workflow server-side, eliminating LLM decision randomness.
///
/// Flow:
/// 1. LLM calls orchestrator_start → gets first action with explicit instructions
/// 2. LLM executes EXACTLY what's returned
/// 3. LLM calls orchestrator_next → gets next action (or completion)
/// 4. Repeat until complete
/// </summary>
public class OrchestratorTool
{
    private readonly WorkspaceAnalyzer _analyzer;
    internal static readonly ConcurrentDictionary<string, ExecutionSession> Sessions = new();
    private static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(30);

    public OrchestratorTool(WorkspaceAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    /// <summary>
    /// Removes expired sessions to prevent unbounded memory growth.
    /// Called opportunistically on Start and Next operations.
    /// </summary>
    private static void CleanupExpiredSessions()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = Sessions
            .Where(kvp => now - kvp.Value.CreatedAt > SessionTimeout)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            Sessions.TryRemove(key, out _);
        }
    }

    public string Start(string workspacePath)
    {
        var spec = _analyzer.Analyze(workspacePath);

        // Handle error cases
        if (spec.Decision.Intent == Intents.Error)
        {
            return Respond(new OrchestratorResponse
            {
                Status = "error",
                Message = spec.Decision.Rationale,
                Instruction = "Tell the user about this error. Do not proceed.",
                Warnings = spec.Warnings
            });
        }

        // Handle brownfield — intercept before unsupported fallback
        if (spec.Decision.Intent == Intents.Unsupported
            && spec.Analysis.State == InstrumentationState.Brownfield
            && spec.Analysis.ExistingInstrumentation?.Type == InstrumentationType.ApplicationInsightsSdk)
        {
            return HandleBrownfieldAnalysis(workspacePath, spec.Analysis);
        }

        // Handle unsupported cases
        if (spec.Decision.Intent == Intents.Unsupported)
        {
            return Respond(new OrchestratorResponse
            {
                Status = "unsupported",
                Message = spec.Decision.Rationale,
                Instruction = "Inform the user this scenario is not yet supported. Manual instrumentation required.",
                Warnings = spec.Warnings
            });
        }

        // Handle clarification needed
        if (spec.Decision.Intent == Intents.ClarificationNeeded)
        {
            return Respond(new OrchestratorResponse
            {
                Status = "clarification_needed",
                Message = spec.Decision.Rationale,
                Instruction = "Ask the user to clarify which project to instrument, then call orchestrator_start again.",
                Warnings = spec.Warnings
            });
        }

        // No actions to execute
        if (spec.Actions.Count == 0)
        {
            return Respond(new OrchestratorResponse
            {
                Status = "complete",
                Message = "No actions required.",
                Instruction = "Inform the user no instrumentation changes are needed."
            });
        }

        // Create session
        CleanupExpiredSessions();
        var session = new ExecutionSession
        {
            WorkspacePath = workspacePath,
            Analysis = spec.Analysis,
            Spec = spec,
            CreatedAt = DateTime.UtcNow
        };
        Sessions[workspacePath] = session;

        // Return first action
        var firstAction = spec.Actions[0];
        var primaryProject = spec.Analysis.Projects.FirstOrDefault();
        var appTypeDescription = primaryProject?.AppType.ToString() ?? "unknown";

        return Respond(new OrchestratorResponse
        {
            Status = "in_progress",
            SessionId = workspacePath,
            Message = $"Instrumentation started for {spec.Analysis.Language} {appTypeDescription} application.",

            // Tell LLM exactly what to do
            Instruction = BuildInstruction(firstAction, spec.AgentMustExecuteFirst),

            // Provide action details for execution
            CurrentAction = firstAction,

            // Progress info
            Progress = $"Step 1 of {spec.Actions.Count}",

            Warnings = spec.Warnings
        });
    }

    public string Next(string sessionId, string completionNote)
    {
        CleanupExpiredSessions();

        if (!Sessions.TryGetValue(sessionId, out var session))
        {
            return Respond(new OrchestratorResponse
            {
                Status = "error",
                Message = "No active session. Call orchestrator_start first.",
                Instruction = "Call orchestrator_start with the workspace path to begin."
            });
        }

        // Guard: cannot call next while still awaiting brownfield analysis
        if (session.State == SessionState.AwaitingAnalysis)
        {
            return Respond(new OrchestratorResponse
            {
                Status = "error",
                SessionId = sessionId,
                Message = "Brownfield analysis is pending. Submit findings first.",
                Instruction = "Call send_brownfield_analysis with the filled analysis template before calling orchestrator_next."
            });
        }

        var spec = session.Spec!;

        // Record completion and advance atomically
        var completedIndex = session.AdvanceIndex();
        if (completedIndex >= spec.Actions.Count)
        {
            Sessions.TryRemove(sessionId, out _);

            return Respond(new OrchestratorResponse
            {
                Status = "complete",
                SessionId = sessionId,
                Message = "All instrumentation actions completed successfully!",
                Instruction = BuildCompletionInstruction(spec),
                CompletedActions = session.CompletedActions.ToList()
            });
        }

        var completedAction = spec.Actions[completedIndex];
        session.CompletedActions.Add(completedAction.Id);
        var nextIndex = completedIndex + 1;

        // Check if all done
        if (nextIndex >= spec.Actions.Count)
        {
            Sessions.TryRemove(sessionId, out _);

            return Respond(new OrchestratorResponse
            {
                Status = "complete",
                SessionId = sessionId,
                Message = "All instrumentation actions completed successfully!",
                Instruction = BuildCompletionInstruction(spec),
                CompletedActions = session.CompletedActions.ToList()
            });
        }

        // Return next action
        var nextAction = spec.Actions[nextIndex];

        return Respond(new OrchestratorResponse
        {
            Status = "in_progress",
            SessionId = sessionId,
            Message = $"Step {completedIndex + 1} complete.",
            Instruction = BuildInstruction(nextAction, null),
            CurrentAction = nextAction,
            Progress = $"Step {nextIndex + 1} of {spec.Actions.Count}",
            CompletedActions = session.CompletedActions.ToList()
        });
    }

    /// <summary>
    /// Handle brownfield detection by creating an AwaitingAnalysis session
    /// and returning an analysis template for the LLM to fill.
    /// </summary>
    private string HandleBrownfieldAnalysis(string workspacePath, Analysis analysis)
    {
        CleanupExpiredSessions();
        var session = new ExecutionSession
        {
            WorkspacePath = workspacePath,
            Analysis = analysis,
            State = SessionState.AwaitingAnalysis,
            Spec = null,
            CreatedAt = DateTime.UtcNow
        };
        Sessions[workspacePath] = session;

        var primaryProject = analysis.Projects.FirstOrDefault();
        var appTypeDescription = primaryProject?.AppType.ToString() ?? "unknown";
        var existingType = analysis.ExistingInstrumentation?.Type.ToString() ?? "unknown";

        return Respond(new OrchestratorResponse
        {
            Status = "analysis_needed",
            SessionId = workspacePath,
            Message = $"Brownfield {existingType} detected in {analysis.Language} {appTypeDescription} application. Code analysis required before migration plan can be generated.",
            Instruction = BuildAnalysisInstruction(),
            AnalysisTemplate = BuildAnalysisTemplate()
        });
    }

    private static string BuildAnalysisInstruction()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("BROWNFIELD ANALYSIS REQUIRED");
        sb.AppendLine();
        sb.AppendLine("Scan the workspace source files and fill in the analysis template provided in the 'analysisTemplate' field.");
        sb.AppendLine("The template has 5 sections. Set any section to null if the concern does not exist in the codebase.");
        sb.AppendLine();
        sb.AppendLine("Sections to analyze:");
        sb.AppendLine("1. serviceOptions — Find the AddApplicationInsightsTelemetry() call and report which options are configured");
        sb.AppendLine("2. initializers — Find all classes implementing ITelemetryInitializer and describe each one");
        sb.AppendLine("3. processors — Find all classes implementing ITelemetryProcessor and describe each one");
        sb.AppendLine("4. clientUsage — Find all files that use TelemetryClient directly (injection, instantiation, or method calls)");
        sb.AppendLine("5. sampling — Find any custom sampling configuration");
        sb.AppendLine();
        sb.AppendLine("When done, call send_brownfield_analysis with the sessionId and your filled findings JSON.");
        return sb.ToString();
    }

    private static AnalysisTemplate BuildAnalysisTemplate()
    {
        return AnalysisTemplate.CreateDefault();
    }

    /// <summary>
    /// Build explicit, unambiguous instructions for the LLM.
    /// This is the key to reducing hallucination - tell it EXACTLY what to do.
    /// </summary>
    private string BuildInstruction(OnboardingAction action, string? preInstruction)
    {
        return BuildInstructionPublic(action, preInstruction);
    }

    /// <summary>
    /// Public accessor for BuildInstruction, used by SendBrownfieldAnalysisTool.
    /// </summary>
    internal static string BuildInstructionPublic(OnboardingAction action, string? preInstruction)
    {
        var instruction = new System.Text.StringBuilder();

        // Pre-instruction (e.g., read docs first)
        if (!string.IsNullOrEmpty(preInstruction))
        {
            instruction.AppendLine($"FIRST: {preInstruction}");
            instruction.AppendLine();
        }

        // Action-specific instructions
        instruction.AppendLine($"ACTION: {action.Description}");
        instruction.AppendLine();

        switch (action.Type)
        {
            case ActionType.ReviewEducation:
                var resources = action.Details.TryGetValue("resources", out var res)
                    ? res as IEnumerable<object> ?? []
                    : [];
                instruction.AppendLine("EXECUTE: Call get_learning_resource for each of these paths:");
                foreach (var resource in resources)
                {
                    instruction.AppendLine($"  - {resource}");
                }
                instruction.AppendLine();
                instruction.AppendLine("Read and understand the content before proceeding.");
                break;

            case ActionType.AddPackage:
                var pkg = action.Details.GetValueOrDefault("package", "")?.ToString();
                var project = action.Details.GetValueOrDefault("targetProject", "")?.ToString();
                var version = action.Details.GetValueOrDefault("version", "")?.ToString();
                var packageManager = action.Details.GetValueOrDefault("packageManager", "")?.ToString();
                if (string.IsNullOrWhiteSpace(pkg) || string.IsNullOrWhiteSpace(project))
                {
                    instruction.AppendLine("ERROR: Missing package or project information. Cannot proceed with this action.");
                    break;
                }
                instruction.AppendLine($"EXECUTE: Run this exact command:");
                var installCommand = packageManager?.ToLowerInvariant() switch
                {
                    "pip" => !string.IsNullOrWhiteSpace(version) && version != "latest-stable"
                        ? $"  pip install {pkg}=={version}"
                        : $"  pip install {pkg}",
                    "npm" => !string.IsNullOrWhiteSpace(version) && version != "latest-stable"
                        ? $"  npm install {pkg}@{version}"
                        : $"  npm install {pkg}",
                    _ => !string.IsNullOrWhiteSpace(version) && version != "latest-stable"
                        ? $"  dotnet add \"{project}\" package {pkg} --version {version}"
                        : $"  dotnet add \"{project}\" package {pkg}"
                };
                instruction.AppendLine(installCommand);
                instruction.AppendLine();
                instruction.AppendLine("Wait for the command to complete successfully.");
                break;

            case ActionType.ModifyCode:
                var file = action.Details.GetValueOrDefault("file", "")?.ToString();
                var snippet = action.Details.GetValueOrDefault("codeSnippet", "")?.ToString();
                var insertAfter = action.Details.GetValueOrDefault("insertAfter", "")?.ToString();
                var usingStmt = action.Details.GetValueOrDefault("requiredUsing", "")?.ToString();
                if (string.IsNullOrWhiteSpace(file) || string.IsNullOrWhiteSpace(snippet))
                {
                    instruction.AppendLine("ERROR: Missing file path or code snippet. Cannot proceed with this action.");
                    break;
                }
                instruction.AppendLine($"EXECUTE: Modify file {file}");
                instruction.AppendLine();
                if (!string.IsNullOrWhiteSpace(usingStmt))
                {
                    instruction.AppendLine($"1. Add this using statement at the top:");
                    instruction.AppendLine($"   using {usingStmt};");
                    instruction.AppendLine();
                    instruction.AppendLine($"2. Add this code IMMEDIATELY after the line containing '{insertAfter}':");
                }
                else
                {
                    instruction.AppendLine($"1. Add this code IMMEDIATELY after the line containing '{insertAfter}':");
                }
                instruction.AppendLine($"   {snippet}");
                instruction.AppendLine();
                instruction.AppendLine("DO NOT add any other code. DO NOT modify anything else.");
                break;

            case ActionType.AddConfig:
                var configFile = action.Details.GetValueOrDefault("file", "")?.ToString();
                var jsonPath = action.Details.GetValueOrDefault("jsonPath", "")?.ToString();
                var value = action.Details.GetValueOrDefault("value", "")?.ToString();
                var envVar = action.Details.GetValueOrDefault("envVarAlternative", "")?.ToString();
                if (string.IsNullOrWhiteSpace(configFile) || string.IsNullOrWhiteSpace(jsonPath))
                {
                    instruction.AppendLine("ERROR: Missing configuration file or JSON path. Cannot proceed with this action.");
                    break;
                }
                instruction.AppendLine($"EXECUTE: Add configuration to {configFile}");
                instruction.AppendLine();
                instruction.AppendLine($"Add this JSON property: \"{jsonPath}\": \"{value}\"");
                if (!string.IsNullOrWhiteSpace(envVar))
                {
                    instruction.AppendLine();
                    instruction.AppendLine($"Tell user they can alternatively set environment variable: {envVar}");
                }
                break;

            case ActionType.ManualStep:
                var manualInstructions = action.Details.GetValueOrDefault("instructions", "")?.ToString();
                if (string.IsNullOrWhiteSpace(manualInstructions))
                {
                    instruction.AppendLine("ERROR: Missing manual step instructions. Cannot proceed with this action.");
                    break;
                }
                instruction.AppendLine($"EXECUTE: {manualInstructions}");
                break;

            default:
                instruction.AppendLine("Execute the action as described.");
                break;
        }

        instruction.AppendLine();
        instruction.AppendLine("When done, call orchestrator_next with the sessionId to continue.");

        return instruction.ToString();
    }

    /// <summary>
    /// Build a dynamic completion instruction based on what was actually done,
    /// instead of hardcoding .NET-specific text.
    /// </summary>
    private static string BuildCompletionInstruction(OnboardingSpec spec)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Tell the user instrumentation is complete. Summarize what was done:");

        foreach (var action in spec.Actions)
        {
            if (action.Type != ActionType.ReviewEducation)
            {
                sb.AppendLine($"- {action.Description}");
            }
        }

        sb.AppendLine();

        switch (spec.Analysis.Language)
        {
            case Language.DotNet:
                sb.AppendLine("Remind them to set the APPLICATIONINSIGHTS_CONNECTION_STRING environment variable or configure it in appsettings.json.");
                break;
            case Language.NodeJs:
                sb.AppendLine("Remind them to set the APPLICATIONINSIGHTS_CONNECTION_STRING environment variable in their .env file or hosting environment.");
                break;
            case Language.Python:
                sb.AppendLine("Remind them to set the APPLICATIONINSIGHTS_CONNECTION_STRING environment variable in their .env file or hosting environment.");
                break;
            default:
                sb.AppendLine("Remind them to configure the APPLICATIONINSIGHTS_CONNECTION_STRING for their environment.");
                break;
        }

        return sb.ToString();
    }

    private static string Respond(OrchestratorResponse response)
    {
        return JsonSerializer.Serialize(response, OnboardingJsonContext.Default.OrchestratorResponse);
    }
}

#region Internal Types

internal enum SessionState
{
    /// <summary>Brownfield analysis template sent, waiting for LLM to submit findings</summary>
    AwaitingAnalysis,
    /// <summary>Normal step-through of actions (greenfield or post-analysis brownfield)</summary>
    Executing
}

internal class ExecutionSession
{
    public required string WorkspacePath { get; init; }
    public required Analysis Analysis { get; init; }
    public OnboardingSpec? Spec { get; set; }
    public SessionState State { get; set; } = SessionState.Executing;
    public BrownfieldFindings? Findings { get; set; }
    private int _currentActionIndex;
    public ConcurrentBag<string> CompletedActions { get; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public int CurrentActionIndex => _currentActionIndex;

    /// <summary>
    /// Atomically advances the action index. Returns the previous index.
    /// </summary>
    public int AdvanceIndex() => Interlocked.Increment(ref _currentActionIndex) - 1;
}

internal record OrchestratorResponse
{
    /// <summary>
    /// Status: "in_progress", "complete", "error", "unsupported", "clarification_needed", "analysis_needed"
    /// </summary>
    public required string Status { get; init; }

    public string? SessionId { get; init; }

    /// <summary>
    /// Human-readable message about current state
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// EXPLICIT instruction for what the LLM must do next.
    /// This is the key to reducing hallucination - tell it exactly what to do.
    /// </summary>
    public required string Instruction { get; init; }

    /// <summary>
    /// The current action details (if in_progress)
    /// </summary>
    public OnboardingAction? CurrentAction { get; init; }

    /// <summary>
    /// Progress indicator: "Step 2 of 4"
    /// </summary>
    public string? Progress { get; init; }

    /// <summary>
    /// Actions already completed
    /// </summary>
    public List<string>? CompletedActions { get; init; }

    /// <summary>
    /// Any warnings to relay to user
    /// </summary>
    public List<string>? Warnings { get; init; }

    /// <summary>
    /// Brownfield analysis template (only present when status is "analysis_needed")
    /// </summary>
    public AnalysisTemplate? AnalysisTemplate { get; init; }
}

#endregion
