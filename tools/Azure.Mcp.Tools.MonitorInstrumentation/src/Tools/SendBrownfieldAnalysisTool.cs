using System.Text.Json;
using Azure.Mcp.Tools.MonitorInstrumentation.Generators;
using Azure.Mcp.Tools.MonitorInstrumentation.Models;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Tools;

/// <summary>
/// Receives brownfield analysis findings from the LLM and generates a targeted migration plan.
/// Called after orchestrator_start returns status "analysis_needed".
/// </summary>
public class SendBrownfieldAnalysisTool
{
    private readonly IEnumerable<IGenerator> _generators;

    public SendBrownfieldAnalysisTool(IEnumerable<IGenerator> generators)
    {
        _generators = generators;
    }

    public string Submit(
        string sessionId,
        ServiceOptionsFindings? serviceOptions,
        InitializerFindings? initializers,
        ProcessorFindings? processors,
        ClientUsageFindings? clientUsage,
        SamplingFindings? sampling,
        TelemetryPipelineFindings? telemetryPipeline)
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

        if (session.State != SessionState.AwaitingAnalysis)
        {
            return Respond(new OrchestratorResponse
            {
                Status = "error",
                SessionId = sessionId,
                Message = "Session is not awaiting analysis. This tool is only valid after orchestrator_start returns 'analysis_needed'.",
                Instruction = "Call orchestrator_next to continue with the current session."
            });
        }

        var parsedFindings = new BrownfieldFindings
        {
            ServiceOptions = serviceOptions,
            Initializers = initializers,
            Processors = processors,
            ClientUsage = clientUsage,
            Sampling = sampling,
            TelemetryPipeline = telemetryPipeline
        };

        // Store findings and attach to analysis for generator
        session.Findings = parsedFindings;
        var analysisWithFindings = session.Analysis with
        {
            BrownfieldFindings = parsedFindings
        };

        // Find matching brownfield generator
        var generator = _generators.FirstOrDefault(g => g.CanHandle(analysisWithFindings));
        if (generator == null)
        {
            return Respond(new OrchestratorResponse
            {
                Status = "unsupported",
                SessionId = sessionId,
                Message = $"No brownfield generator available for {session.Analysis.Language}/{session.Analysis.Projects.FirstOrDefault()?.AppType}/{session.Analysis.State}",
                Instruction = "Inform the user this brownfield scenario is not yet supported. Manual migration required."
            });
        }

        // Generate migration spec
        var spec = generator.Generate(analysisWithFindings);
        session.Spec = spec;
        session.State = SessionState.Executing;

        // Return first action
        if (spec.Actions.Count == 0)
        {
            OrchestratorTool.Sessions.TryRemove(sessionId, out _);
            return Respond(new OrchestratorResponse
            {
                Status = "complete",
                SessionId = sessionId,
                Message = "Analysis complete. No migration actions required.",
                Instruction = "Inform the user no code changes are needed for this migration."
            });
        }

        var firstAction = spec.Actions[0];
        var primaryProject = spec.Analysis.Projects.FirstOrDefault();
        var appTypeDescription = primaryProject?.AppType.ToString() ?? "unknown";

        return Respond(new OrchestratorResponse
        {
            Status = "in_progress",
            SessionId = sessionId,
            Message = $"Migration plan generated for {spec.Analysis.Language} {appTypeDescription} application.",
            Instruction = OrchestratorTool.BuildInstructionPublic(firstAction, spec.AgentMustExecuteFirst),
            CurrentAction = firstAction,
            Progress = $"Step 1 of {spec.Actions.Count}",
            Warnings = spec.Warnings
        });
    }

    private static string Respond(OrchestratorResponse response)
    {
        return JsonSerializer.Serialize(response, OnboardingJsonContext.Default.OrchestratorResponse);
    }
}
