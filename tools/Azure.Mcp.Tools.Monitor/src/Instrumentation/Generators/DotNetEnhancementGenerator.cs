using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Generator for apps already on Application Insights 3.x or Azure Monitor Distro.
/// Confirms no migration needed and produces enhancement actions based on user selection.
/// </summary>
public class DotNetEnhancementGenerator : IGenerator
{
    /// <summary>
    /// Supported enhancement options — each maps to a learn resource, package, and setup instruction.
    /// </summary>
    public static readonly Dictionary<string, EnhancementOption> SupportedEnhancements = new(StringComparer.OrdinalIgnoreCase)
    {
        ["entityframework"] = new("Entity Framework Core Instrumentation",
            Packages.EntityFrameworkInstrumentation,
            [LearningResources.ApiEntityFrameworkInstrumentation, LearningResources.ApiConfigureOpenTelemetryProvider],
            "Add .AddEntityFrameworkCoreInstrumentation() to the tracing pipeline."),

        ["redis"] = new("Redis Instrumentation (StackExchange.Redis)",
            Packages.RedisInstrumentation,
            [LearningResources.ApiRedisInstrumentation, LearningResources.ApiConfigureOpenTelemetryProvider],
            "Add .AddRedisInstrumentation() to the tracing pipeline. Ensure IConnectionMultiplexer is registered in DI."),

        ["sqlclient"] = new("SQL Client Instrumentation",
            Packages.SqlClientInstrumentation,
            [LearningResources.ApiSqlClientInstrumentation, LearningResources.ApiConfigureOpenTelemetryProvider],
            "Add .AddSqlClientInstrumentation() to the tracing pipeline."),

        ["http"] = new("HTTP Client/Server Enrichment",
            Packages.HttpInstrumentation,
            [LearningResources.ApiHttpInstrumentation, LearningResources.ApiConfigureOpenTelemetryProvider],
            "Customize HTTP client/server instrumentation with enrichment callbacks, filters, and RecordException."),

        ["otlp"] = new("OTLP Exporter",
            Packages.OtlpExporter,
            [LearningResources.ApiOtlpExporter, LearningResources.ApiConfigureOpenTelemetryProvider],
            "Add .AddOtlpExporter() to each signal pipeline (traces, metrics, logs) for dual export."),

        ["console"] = new("Console Exporter (dev only)",
            Packages.ConsoleExporter,
            [LearningResources.ApiConsoleExporter, LearningResources.ApiConfigureOpenTelemetryProvider],
            "Add .AddConsoleExporter() to each signal pipeline for local debugging output."),

        ["sampling"] = new("Sampling Configuration",
            null,
            [LearningResources.ApiSampling],
            "Configure TracesPerSecond or SamplingRatio in service options, or use the OTel pipeline."),

        ["processors"] = new("Custom Processors",
            null,
            [LearningResources.ApiActivityProcessors, LearningResources.ApiLogProcessors, LearningResources.ApiConfigureOpenTelemetryProvider],
            "Create BaseProcessor<Activity> for trace enrichment/filtering and BaseProcessor<LogRecord> for log enrichment."),
    };

    public bool CanHandle(Analysis analysis)
    {
        if (analysis.Language != Language.DotNet)
            return false;

        var hasDotNetProject = analysis.Projects.Any(p =>
            p.AppType == AppType.AspNetCore || p.AppType == AppType.Worker);
        if (!hasDotNetProject)
            return false;

        if (analysis.State != InstrumentationState.Brownfield)
            return false;

        // Match: AI SDK 3.x (IsTargetVersion) or Azure Monitor Distro
        var instr = analysis.ExistingInstrumentation;
        if (instr == null)
            return false;

        return (instr.Type == InstrumentationType.ApplicationInsightsSdk && instr.IsTargetVersion)
            || instr.Type == InstrumentationType.AzureMonitorDistro;
    }

    public OnboardingSpec Generate(Analysis analysis)
    {
        var project = analysis.Projects.First(p =>
            p.AppType == AppType.AspNetCore || p.AppType == AppType.Worker);

        var sdkLabel = analysis.ExistingInstrumentation?.Type == InstrumentationType.AzureMonitorDistro
            ? "Azure Monitor Distro"
            : $"Application Insights 3.x ({analysis.ExistingInstrumentation?.Version})";

        var builder = new OnboardingSpecBuilder(analysis)
            .WithAgentPreExecuteInstruction(AgentPreExecuteInstruction)
            .WithDecision(
                Intents.Enhance,
                Approaches.ApplicationInsights3x,
                $"Already on {sdkLabel}. No migration needed. Enhancement options available.");

        return builder.Build();
    }

    /// <summary>
    /// Generate actions for one or more enhancement selections.
    /// Called by the SendEnhancementSelectTool after user picks.
    /// </summary>
    public static OnboardingSpec GenerateForSelections(
        Analysis analysis,
        List<(string key, EnhancementOption option)> selections)
    {
        var project = analysis.Projects.First(p =>
            p.AppType == AppType.AspNetCore || p.AppType == AppType.Worker);
        var entryPoint = project.EntryPoint ?? "Program.cs";

        var displayNames = string.Join(" + ", selections.Select(s => s.option.DisplayName));

        var builder = new OnboardingSpecBuilder(analysis)
            .WithAgentPreExecuteInstruction(AgentPreExecuteInstruction)
            .WithDecision(
                Intents.Enhance,
                Approaches.ApplicationInsights3x,
                $"Adding {displayNames}.");

        // Collect all learn resources (deduplicated)
        var allResources = selections
            .SelectMany(s => s.option.LearnResources)
            .Distinct()
            .ToList();

        builder.AddReviewEducationAction(
            "review-enhancement",
            $"Review the guides for: {displayNames}",
            allResources);

        var lastDep = "review-enhancement";

        // Package actions for each enhancement that needs one
        foreach (var (key, option) in selections)
        {
            if (option.PackageName != null)
            {
                var actionId = $"add-package-{key}";
                builder.AddPackageAction(
                    actionId,
                    $"Install {option.PackageName}",
                    Packages.PackageManagerNuGet,
                    option.PackageName,
                    Packages.LatestStableVersion,
                    project.ProjectFile,
                    lastDep);
                lastDep = actionId;
            }
        }

        // Setup instruction for each enhancement
        foreach (var (key, option) in selections)
        {
            var actionId = $"configure-{key}";
            builder.AddManualStepAction(
                actionId,
                $"Configure {option.DisplayName}",
                $"In {entryPoint}: {option.SetupInstruction} " +
                $"Refer to the {option.DisplayName} guide for options and examples.",
                dependsOn: lastDep);
            lastDep = actionId;
        }

        return builder.Build();
    }
}

public record EnhancementOption(
    string DisplayName,
    string? PackageName,
    string[] LearnResources,
    string SetupInstruction);
