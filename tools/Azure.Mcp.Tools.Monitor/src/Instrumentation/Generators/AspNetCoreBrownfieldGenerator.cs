using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Generator for ASP.NET Core brownfield projects migrating from Application Insights SDK 2.x to 3.x.
/// </summary>
public class AspNetCoreBrownfieldGenerator : BrownfieldGeneratorBase
{
    protected override AppType TargetAppType => AppType.AspNetCore;
    protected override string PackageName => Packages.ApplicationInsightsAspNetCore;
    protected override string PackageVersion => Packages.ApplicationInsightsAspNetCore3x;
    protected override string MigrationCodeResource => LearningResources.MigrationAppInsights2xTo3xCode;
    protected override string MigrationNoCodeChangeResource => LearningResources.MigrationAppInsights2xTo3xNoCodeChange;
    protected override string EntryPointMethodName => "AddApplicationInsightsTelemetry";

    public override bool CanHandle(Analysis analysis)
    {
        var aspNetCoreProjectCount = analysis.Projects
            .Count(p => p.AppType == AppType.AspNetCore);

        return analysis.Language == Language.DotNet
            && aspNetCoreProjectCount == 1
            && analysis.State == InstrumentationState.Brownfield
            && analysis.ExistingInstrumentation?.Type == InstrumentationType.ApplicationInsightsSdk
            && analysis.ExistingInstrumentation?.IsTargetVersion != true
            && analysis.BrownfieldFindings is not null;
    }

    protected override bool HasFrameworkSpecificCodeChanges(ServiceOptionsFindings opts)
    {
        if (opts.RequestCollectionOptions != null)
            return true;
        if (opts.UseApplicationInsights == true)
            return true;
        return false;
    }

    protected override string AddServiceOptionsActions(
        OnboardingSpecBuilder builder,
        ServiceOptionsFindings opts,
        string entryPoint,
        string lastDependency)
    {
        var dep = AddSharedServiceOptionsActions(builder, opts, entryPoint, EntryPointMethodName, lastDependency);

        // ASP.NET Core removed properties
        var removedProperties = new List<(string name, object? value)>
        {
            ("EnableAdaptiveSampling", opts.EnableAdaptiveSampling),
            ("DeveloperMode", opts.DeveloperMode),
            ("EndpointAddress", opts.EndpointAddress),
            ("EnableHeartbeat", opts.EnableHeartbeat),
            ("EnableDebugLogger", opts.EnableDebugLogger),
            ("RequestCollectionOptions", opts.RequestCollectionOptions),
            ("DependencyCollectionOptions", opts.DependencyCollectionOptions),
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
        OnboardingSpecBuilder builder,
        ServiceOptionsFindings opts,
        string entryPoint,
        string lastDependency)
    {
        var dep = lastDependency;

        // ASP.NET Core-only: UseApplicationInsights()
        if (opts.UseApplicationInsights == true)
        {
            var actionId = "remove-use-appinsights";
            builder.AddManualStepAction(
                actionId,
                "Remove UseApplicationInsights() call",
                $"In {entryPoint}, remove the call to UseApplicationInsights() on IWebHostBuilder — it is removed in 3.x.",
                dependsOn: dep);
            dep = actionId;
        }

        // Shared removed methods
        dep = AddSharedRemovedMethodActions(builder, opts, entryPoint, EntryPointMethodName, dep);

        return dep;
    }
}
