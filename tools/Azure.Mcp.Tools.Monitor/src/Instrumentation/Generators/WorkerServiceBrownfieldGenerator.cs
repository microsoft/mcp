using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Generator for Worker Service brownfield projects migrating from Application Insights SDK 2.x to 3.x.
/// </summary>
public class WorkerServiceBrownfieldGenerator : BrownfieldGeneratorBase
{
    protected override AppType TargetAppType => AppType.Worker;
    protected override string PackageName => Packages.WorkerService;
    protected override string PackageVersion => Packages.WorkerService3x;
    protected override string MigrationCodeResource => LearningResources.MigrationWorkerService2xTo3xCode;
    protected override string MigrationNoCodeChangeResource => LearningResources.MigrationWorkerService2xTo3xNoCodeChange;
    protected override string EntryPointMethodName => "AddApplicationInsightsTelemetryWorkerService";

    public override bool CanHandle(Analysis analysis)
    {
        // Match Worker SDK projects, or Console/Library projects that reference the WorkerService package
        // (Console apps were commonly told to use AddApplicationInsightsTelemetryWorkerService in 2.x)
        var isWorkerProject = analysis.Projects.Any(p => p.AppType == AppType.Worker);
        var isConsoleWithWorkerServicePackage = !isWorkerProject
            && analysis.Projects.Any(p => p.AppType is AppType.Console or AppType.Library)
            && HasWorkerServicePackage(analysis);

        return analysis.Language == Language.DotNet
            && (isWorkerProject || isConsoleWithWorkerServicePackage)
            && analysis.State == InstrumentationState.Brownfield
            && analysis.ExistingInstrumentation?.Type == InstrumentationType.ApplicationInsightsSdk
            && analysis.ExistingInstrumentation?.IsTargetVersion != true
            && analysis.BrownfieldFindings is not null;
    }

    protected override ProjectInfo FindProject(Analysis analysis)
    {
        // Prefer Worker app type, fall back to Console/Library
        return analysis.Projects.FirstOrDefault(p => p.AppType == AppType.Worker)
            ?? analysis.Projects.First(p => p.AppType is AppType.Console or AppType.Library);
    }

    private static bool HasWorkerServicePackage(Analysis analysis)
    {
        return analysis.ExistingInstrumentation?.Evidence
            .Any(e => e.Indicator.Contains("Microsoft.ApplicationInsights.WorkerService", StringComparison.OrdinalIgnoreCase)) == true;
    }

    protected override bool HasFrameworkSpecificCodeChanges(ServiceOptionsFindings opts)
    {
        // Worker Service-only removed properties
        if (opts.EnableEventCounterCollectionModule != null)
            return true;
        if (opts.EnableAppServicesHeartbeatTelemetryModule != null)
            return true;
        if (opts.EnableAzureInstanceMetadataTelemetryModule != null)
            return true;
        if (opts.EnableDiagnosticsTelemetryModule != null)
            return true;
        return false;
    }

    protected override string AddServiceOptionsActions(
        OnboardingSpecBuilder builder, ServiceOptionsFindings opts,
        string entryPoint, string lastDependency)
    {
        var dep = AddSharedServiceOptionsActions(builder, opts, entryPoint, EntryPointMethodName, lastDependency);

        // Worker Service removed properties (shared + Worker-specific)
        var removedProperties = new List<(string name, object? value)>
        {
            ("EnableAdaptiveSampling", opts.EnableAdaptiveSampling),
            ("DeveloperMode", opts.DeveloperMode),
            ("EndpointAddress", opts.EndpointAddress),
            ("EnableHeartbeat", opts.EnableHeartbeat),
            ("EnableDebugLogger", opts.EnableDebugLogger),
            ("DependencyCollectionOptions", opts.DependencyCollectionOptions),
            ("EnableEventCounterCollectionModule", opts.EnableEventCounterCollectionModule),
            ("EnableAppServicesHeartbeatTelemetryModule", opts.EnableAppServicesHeartbeatTelemetryModule),
            ("EnableAzureInstanceMetadataTelemetryModule", opts.EnableAzureInstanceMetadataTelemetryModule),
            ("EnableDiagnosticsTelemetryModule", opts.EnableDiagnosticsTelemetryModule),
        };

        var removedFound = removedProperties.Where(p => p.value != null).Select(p => p.name).ToList();
        if (removedFound.Count > 0)
        {
            var actionId = "remove-deprecated-options";
            builder.AddManualStepAction(
                actionId,
                "Remove deprecated ApplicationInsightsServiceOptions properties",
                $"In {entryPoint}, remove these properties from the {EntryPointMethodName} options block — they are removed in 3.x: {string.Join(", ", removedFound)}",
                dependsOn: dep);
            dep = actionId;
        }

        return dep;
    }

    protected override string AddRemovedMethodActions(
        OnboardingSpecBuilder builder, ServiceOptionsFindings opts,
        string entryPoint, string lastDependency)
    {
        // Worker Service has no UseApplicationInsights() — go straight to shared methods
        return AddSharedRemovedMethodActions(builder, opts, entryPoint, EntryPointMethodName, lastDependency);
    }
}
