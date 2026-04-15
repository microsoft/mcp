// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using Azure.Mcp.Tools.Monitor.Generators;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Pipeline;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Tools;

public class OrchestratorTool
{
    private readonly WorkspaceAnalyzer _analyzer;
    internal static readonly ConcurrentDictionary<string, ExecutionSession> Sessions = new();
    private static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(30);

    public OrchestratorTool(WorkspaceAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

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
        OnboardingSpec spec;
        try
        {
            spec = _analyzer.Analyze(workspacePath);
        }
        catch (Exception ex)
        {
            return Respond(new OrchestratorResponse
            {
                Status = "error",
                Message = $"Analysis failed: {ex.GetType().Name}: {ex.Message}",
                Instruction = "Tell the user about this error. Do not proceed.",
                Warnings = [ex.StackTrace ?? "No stack trace available"]
            });
        }

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

        if (spec.Decision.Intent == Intents.Enhance)
        {
            return HandleEnhancementOffer(workspacePath, spec);
        }

        if (spec.Decision.Intent == Intents.Unsupported
            && spec.Analysis.State == InstrumentationState.Brownfield
            && spec.Analysis.ExistingInstrumentation?.Type == InstrumentationType.ApplicationInsightsSdk
            && spec.Analysis.ExistingInstrumentation?.IsTargetVersion != true)
        {
            return HandleBrownfieldAnalysis(workspacePath, spec.Analysis);
        }

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

        if (spec.Decision.Intent == Intents.ClarificationNeeded)
        {
            return Respond(new OrchestratorResponse
            {
                Status = "clarification_needed",
                Message = spec.Decision.Rationale,
                Instruction = "Ask the user to clarify which project to instrument, then call orchestrator-start again.",
                Warnings = spec.Warnings
            });
        }

        if (spec.Actions.Count == 0)
        {
            return Respond(new OrchestratorResponse
            {
                Status = "complete",
                Message = "No actions required.",
                Instruction = "Inform the user no instrumentation changes are needed."
            });
        }

        CleanupExpiredSessions();
        var session = new ExecutionSession
        {
            WorkspacePath = workspacePath,
            Analysis = spec.Analysis,
            Spec = spec,
            CreatedAt = DateTime.UtcNow
        };
        Sessions[workspacePath] = session;

        var firstAction = spec.Actions[0];
        var primaryProject = spec.Analysis.Projects.FirstOrDefault();
        var appTypeDescription = primaryProject?.AppType.ToString() ?? "unknown";

        return Respond(new OrchestratorResponse
        {
            Status = "in_progress",
            SessionId = workspacePath,
            Message = $"Instrumentation started for {spec.Analysis.Language} {appTypeDescription} application.",
            Instruction = BuildInstruction(firstAction, spec.AgentMustExecuteFirst),
            CurrentAction = firstAction,
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
                Message = "No active session. Call orchestrator-start first.",
                Instruction = "Call orchestrator-start with the workspace path to begin."
            });
        }

        if (session.State == SessionState.AwaitingAnalysis)
        {
            return Respond(new OrchestratorResponse
            {
                Status = "error",
                SessionId = sessionId,
                Message = "Brownfield analysis is pending. Submit findings first.",
                Instruction = "Call send-brownfield-analysis with the filled analysis template before calling orchestrator-next."
            });
        }

        if (session.State == SessionState.AwaitingEnhancementSelection)
        {
            return Respond(new OrchestratorResponse
            {
                Status = "error",
                SessionId = sessionId,
                Message = "Enhancement selection is pending. Send selection first.",
                Instruction = "Call send-enhancement-select with the chosen enhancement keys before calling orchestrator-next."
            });
        }

        var spec = session.Spec!;

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

    private string HandleEnhancementOffer(string workspacePath, OnboardingSpec spec)
    {
        CleanupExpiredSessions();
        var session = new ExecutionSession
        {
            WorkspacePath = workspacePath,
            Analysis = spec.Analysis,
            State = SessionState.AwaitingEnhancementSelection,
            Spec = spec,
            CreatedAt = DateTime.UtcNow
        };
        Sessions[workspacePath] = session;

        var sdkLabel = spec.Analysis.ExistingInstrumentation?.Type == InstrumentationType.AzureMonitorDistro
            ? "Azure Monitor Distro"
            : "Application Insights 3.x";

        var version = spec.Analysis.ExistingInstrumentation?.Version;
        var versionSuffix = version != null ? $" (v{version})" : string.Empty;

        var options = DotNetEnhancementGenerator.SupportedEnhancements
            .Select(kv => new EnhancementOptionInfo { Key = kv.Key, DisplayName = kv.Value.DisplayName })
            .ToList();

        return Respond(new OrchestratorResponse
        {
            Status = "enhancement_available",
            SessionId = workspacePath,
            Message = $"Already on {sdkLabel}{versionSuffix}. No migration needed. Choose an enhancement to add:",
            Instruction = BuildEnhancementInstruction(),
            EnhancementOptions = options
        });
    }

    private static string BuildEnhancementInstruction()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("ENHANCEMENT SELECTION");
        sb.AppendLine();
        sb.AppendLine("The application is already on the latest SDK version. No migration is needed.");
        sb.AppendLine("Present the enhancement options to the user and ask what they'd like to add.");
        sb.AppendLine("The user may select one or more options. They can describe what they want in natural language.");
        sb.AppendLine();
        sb.AppendLine("When the user has chosen, call send-enhancement-select with the sessionId and the selected option key(s) as a comma-separated string (e.g. 'redis,processors').");
        sb.AppendLine("If the user asks for something not in the list, inform them it is not currently supported through MCP.");
        return sb.ToString();
    }

    private static string BuildAnalysisInstruction()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("BROWNFIELD ANALYSIS REQUIRED");
        sb.AppendLine();
        sb.AppendLine("Scan the workspace source files and fill in the analysis template provided in the 'analysisTemplate' field.");
        sb.AppendLine("The template has 7 sections. For sections that do not exist in the codebase, pass an empty/default object (e.g. found: false) rather than null.");
        sb.AppendLine();
        sb.AppendLine("Sections to analyze:");
        sb.AppendLine("1. serviceOptions — Find the AddApplicationInsightsTelemetry() or AddApplicationInsightsTelemetryWorkerService() call, or for Console apps find TelemetryConfiguration.CreateDefault() / TelemetryConfiguration.Active usage, and report which options are configured");
        sb.AppendLine("2. initializers — Find all classes implementing ITelemetryInitializer AND any classes implementing IConfigureOptions<TelemetryConfiguration> (e.g. classes that call SetAzureTokenCredential for AAD auth). Also report any direct config.SetAzureTokenCredential() calls found in entry points (e.g. Program.cs, Global.asax.cs) as an initializer entry with purpose mentioning 'SetAzureTokenCredential for AAD auth'. Report all types here — describe each one with its purpose");
        sb.AppendLine("3. processors — Find all classes implementing ITelemetryProcessor and describe each one");
        sb.AppendLine("4. clientUsage — Find all files that use TelemetryClient directly OR access Application Insights telemetry types (RequestTelemetry, DependencyTelemetry, TraceTelemetry, EventTelemetry, MetricTelemetry, ExceptionTelemetry, AvailabilityTelemetry) from HttpContext.Features, HttpContext.GetRequestTelemetry(), or via Microsoft.ApplicationInsights.DataContracts. For each file, list every Track*/GetMetric method name called (e.g. TrackEvent, TrackException, TrackPageView, TrackAvailability, GetMetric) — several have removed overloads in 3.x. Also note: (a) Features.Get<RequestTelemetry>() or GetRequestTelemetry() usage — in 3.x use Activity.Current with SetTag(); (b) manual construction of telemetry objects (new DependencyTelemetry(), new RequestTelemetry(), etc.) — these types still exist in 3.x but some properties changed; (c) type checks like 'telemetry is RequestTelemetry' in custom code outside initializers/processors");
        sb.AppendLine("5. sampling — Find any custom sampling configuration (e.g. custom ISamplingProcessor, .SetSampler<T>(), or explicit TelemetryProcessorChainBuilder sampling setup). Do NOT report EnableAdaptiveSampling here — that is a service option handled in section 1");
        sb.AppendLine("6. telemetryPipeline — Find any custom ITelemetryChannel implementations, TelemetryConfiguration.TelemetryChannel assignments, or TelemetrySinks/DefaultTelemetrySink usage — all removed in 3.x");
        sb.AppendLine("7. logging — Find any explicit loggingBuilder.AddApplicationInsights() or services.AddLogging(b => b.AddApplicationInsights(...)) calls, and any AddFilter<ApplicationInsightsLoggerProvider>(...) log filter registrations — ApplicationInsightsLoggerProvider is removed in 3.x and logging is now automatic");
        sb.AppendLine();
        sb.AppendLine("When done, call send-brownfield-analysis with the sessionId and your filled findings JSON.");
        return sb.ToString();
    }

    private static AnalysisTemplate BuildAnalysisTemplate()
    {
        return AnalysisTemplate.CreateDefault();
    }

    private string BuildInstruction(OnboardingAction action, string? preInstruction)
    {
        return BuildInstructionPublic(action, preInstruction);
    }

    internal static string BuildInstructionPublic(OnboardingAction action, string? preInstruction)
    {
        var instruction = new System.Text.StringBuilder();

        if (!string.IsNullOrEmpty(preInstruction))
        {
            instruction.AppendLine($"FIRST: {preInstruction}");
            instruction.AppendLine();
        }

        instruction.AppendLine($"ACTION: {action.Description}");
        instruction.AppendLine();

        switch (action.Type)
        {
            case ActionType.ReviewEducation:
                var resources = action.Details.TryGetValue("resources", out var res)
                    ? res as IEnumerable<object> ?? []
                    : [];
                instruction.AppendLine("EXECUTE: Call get-learning-resource for each of these paths:");
                foreach (var resource in resources)
                {
                    instruction.AppendLine($"  - {resource}");
                }
                instruction.AppendLine();
                instruction.AppendLine("Read and understand the content before proceeding.");
                break;

            case ActionType.AddPackage:
                var pkg = action.Details.GetValueOrDefault("package", string.Empty)?.ToString();
                var project = action.Details.GetValueOrDefault("targetProject", string.Empty)?.ToString();
                var version = action.Details.GetValueOrDefault("version", string.Empty)?.ToString();
                var packageManager = action.Details.GetValueOrDefault("packageManager", string.Empty)?.ToString();
                if (string.IsNullOrWhiteSpace(pkg) || string.IsNullOrWhiteSpace(project))
                {
                    instruction.AppendLine("ERROR: Missing package or project information. Cannot proceed with this action.");
                    break;
                }
                instruction.AppendLine("EXECUTE: Run this exact command:");
                var installCommand = packageManager?.ToLowerInvariant() switch
                {
                    "pip" => !string.IsNullOrWhiteSpace(version) && version != "latest-stable"
                        ? $"  pip install {pkg}=={version}"
                        : $"  pip install {pkg}",
                    "npm" => !string.IsNullOrWhiteSpace(version) && version != "latest-stable"
                        ? $"  npm install {pkg}@{version}"
                        : $"  npm install {pkg}",
                    "nuget-vs" => $"  Install-Package {pkg}",
                    _ => !string.IsNullOrWhiteSpace(version) && version != "latest-stable"
                        ? $"  dotnet add \"{project}\" package {pkg} --version {version}"
                        : $"  dotnet add \"{project}\" package {pkg}"
                };
                instruction.AppendLine(installCommand);
                if (packageManager?.ToLowerInvariant() == "nuget-vs")
                {
                    instruction.AppendLine();
                    instruction.AppendLine("ASK THE USER to run this command in the Package Manager Console (View → Other Windows → Package Manager Console) or install via the NuGet Package Manager UI (right-click project → Manage NuGet Packages).");
                    instruction.AppendLine("The agent cannot run this command — it requires the Package Manager Console which is separate from the developer terminal.");
                    instruction.AppendLine("Wait for the user to confirm the package is installed, then call orchestrator-next to continue.");
                }
                instruction.AppendLine();
                instruction.AppendLine("Wait for the command to complete successfully.");
                break;

            case ActionType.ModifyCode:
                var file = action.Details.GetValueOrDefault("file", string.Empty)?.ToString();
                var snippet = action.Details.GetValueOrDefault("codeSnippet", string.Empty)?.ToString();
                var insertAfter = action.Details.GetValueOrDefault("insertAfter", string.Empty)?.ToString();
                var usingStmt = action.Details.GetValueOrDefault("requiredUsing", string.Empty)?.ToString();
                if (string.IsNullOrWhiteSpace(file) || string.IsNullOrWhiteSpace(snippet))
                {
                    instruction.AppendLine("ERROR: Missing file path or code snippet. Cannot proceed with this action.");
                    break;
                }
                instruction.AppendLine($"EXECUTE: Modify file {file}");
                instruction.AppendLine();
                if (!string.IsNullOrWhiteSpace(usingStmt))
                {
                    instruction.AppendLine("1. Add this using statement at the top:");
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
                var configFile = action.Details.GetValueOrDefault("file", string.Empty)?.ToString();
                var jsonPath = action.Details.GetValueOrDefault("jsonPath", string.Empty)?.ToString();
                var value = action.Details.GetValueOrDefault("value", string.Empty)?.ToString();
                var envVar = action.Details.GetValueOrDefault("envVarAlternative", string.Empty)?.ToString();
                if (string.IsNullOrWhiteSpace(configFile) || string.IsNullOrWhiteSpace(jsonPath))
                {
                    instruction.AppendLine("ERROR: Missing configuration file or config path. Cannot proceed with this action.");
                    break;
                }
                instruction.AppendLine($"EXECUTE: Add configuration to {configFile}");
                instruction.AppendLine();
                if (configFile.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
                {
                    // XML config (ApplicationInsights.config, Web.config)
                    instruction.AppendLine($"Set the <{jsonPath}> element value to \"{value}\" in the XML file.");
                    instruction.AppendLine($"Example: <{jsonPath}>{value}</{jsonPath}>");
                }
                else
                {
                    // JSON config (appsettings.json)
                    instruction.AppendLine($"Add this JSON property: \"{jsonPath}\": \"{value}\"");
                }
                if (!string.IsNullOrWhiteSpace(envVar))
                {
                    instruction.AppendLine();
                    instruction.AppendLine($"Tell user they can alternatively set environment variable: {envVar}");
                }
                break;

            case ActionType.ManualStep:
                var manualInstructions = action.Details.GetValueOrDefault("instructions", string.Empty)?.ToString();
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
        instruction.AppendLine("When done, call orchestrator-next with the sessionId to continue.");

        return instruction.ToString();
    }

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
    AwaitingAnalysis,
    AwaitingEnhancementSelection,
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

    public int AdvanceIndex() => Interlocked.Increment(ref _currentActionIndex) - 1;
}

internal sealed record EnhancementOptionInfo
{
    public required string Key { get; init; }
    public required string DisplayName { get; init; }
}

internal record OrchestratorResponse
{
    public required string Status { get; init; }
    public string? SessionId { get; init; }
    public required string Message { get; init; }
    public required string Instruction { get; init; }
    public OnboardingAction? CurrentAction { get; init; }
    public string? Progress { get; init; }
    public List<string>? CompletedActions { get; init; }
    public List<string>? Warnings { get; init; }
    public AnalysisTemplate? AnalysisTemplate { get; init; }
    public List<EnhancementOptionInfo>? EnhancementOptions { get; init; }
}

#endregion
