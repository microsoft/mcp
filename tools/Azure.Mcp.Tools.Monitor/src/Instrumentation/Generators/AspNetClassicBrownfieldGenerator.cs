using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Generator for classic ASP.NET brownfield projects migrating from Application Insights 2.x to 3.x.
/// Supports ASP.NET MVC, WebForms, and generic ASP.NET app types.
/// Uses Package Manager Console (nuget-vs) for package operations and ConfigureOpenTelemetryBuilder for non-DI extensibility.
/// </summary>
public class AspNetClassicBrownfieldGenerator : BrownfieldGeneratorBase
{
    // TargetAppType is used by base for default FindProject — we override FindProject instead
    protected override AppType TargetAppType => AppType.AspNetMvc;
    protected override string PackageName => Packages.ApplicationInsightsWeb;
    protected override string PackageVersion => Packages.ApplicationInsightsWeb3x;
    protected override string MigrationCodeResource => LearningResources.MigrationAspNetClassic2xTo3xCode;
    protected override string MigrationNoCodeChangeResource => LearningResources.MigrationAspNetClassic2xTo3xCode; // No no-code-change path for classic — always has config changes
    protected override string EntryPointMethodName => "TelemetryConfiguration.CreateDefault";
    protected override string PackageManagerType => Packages.PackageManagerNuGetVS;
    protected override string DefaultEntryPoint => "Global.asax.cs";

    protected override ProjectInfo FindProject(Analysis analysis)
        => analysis.Projects.First(p =>
            p.AppType is AppType.AspNetClassic or AppType.AspNetMvc or AppType.AspNetWebForms);

    public override bool CanHandle(Analysis analysis)
    {
        var classicProjectCount = analysis.Projects
            .Count(p => p.AppType is AppType.AspNetClassic
                     or AppType.AspNetMvc
                     or AppType.AspNetWebForms);

        return analysis.Language == Language.DotNet
            && classicProjectCount == 1
            && analysis.State == InstrumentationState.Brownfield
            && analysis.ExistingInstrumentation?.Type == InstrumentationType.ApplicationInsightsSdk
            && analysis.ExistingInstrumentation?.IsTargetVersion != true
            && analysis.BrownfieldFindings is not null;
    }

    protected override List<string> BuildLearnResources(BrownfieldFindings? findings)
    {
        var resources = new List<string> { MigrationCodeResource };

        // Always include non-DI extensibility doc for classic ASP.NET
        resources.Add(LearningResources.ApiConfigureOpenTelemetryBuilder);

        if (findings?.Initializers is { Found: true, Implementations.Count: > 0 }
            || findings?.Processors is { Found: true, Implementations.Count: > 0 })
        {
            resources.Add(LearningResources.ApiActivityProcessors);
            resources.Add(LearningResources.ApiLogProcessors);
            // No ConfigureOpenTelemetryProvider — classic uses ConfigureOpenTelemetryBuilder (already added above)
        }

        if (HasBreakingClientUsage(findings?.ClientUsage))
            resources.Add(LearningResources.ApiTelemetryClient);

        // Surface AAD migration doc when an initializer or processor related to
        // credential / AAD / token authentication is detected.
        if (HasAadRelatedCustomizations(findings))
            resources.Add(LearningResources.MigrationAadAuthentication);

        // No Sampling.md — classic ASP.NET sampling is via <TracesPerSecond>/<SamplingRatio> in applicationinsights.config,
        // already covered in the migration doc. Custom OTel samplers not supported with 3.x shim.

        return resources;
    }

    protected override bool HasFrameworkSpecificCodeChanges(ServiceOptionsFindings opts)
    {
        // Classic ASP.NET always has code changes — applicationinsights.config must be rewritten
        return true;
    }

    protected override string AddServiceOptionsActions(
        OnboardingSpecBuilder builder, ServiceOptionsFindings opts,
        string entryPoint, string lastDependency)
    {
        var dep = AddSharedServiceOptionsActions(builder, opts, entryPoint, EntryPointMethodName, lastDependency);

        // Classic ASP.NET: rewrite applicationinsights.config
        var actionId = "rewrite-appinsights-config";
        builder.AddManualStepAction(
            actionId,
            "Rewrite applicationinsights.config to 3.x format",
            "Rewrite applicationinsights.config to the 3.x format: " +
            "remove the entire <TelemetryInitializers> section, <TelemetryModules> section, " +
            "<TelemetryProcessors> section, and <TelemetryChannel> section. " +
            "Replace <InstrumentationKey> with <ConnectionString>. " +
            "Add 3.x elements: <TracesPerSecond>, <EnableTraceBasedLogsSampler>, " +
            "<EnableQuickPulseMetricStream>, <EnablePerformanceCounterCollectionModule>, " +
            "<EnableDependencyTrackingTelemetryModule>, <EnableRequestTrackingTelemetryModule>, " +
            "<AddAutoCollectedMetricExtractor>. " +
            "See the migration guide for the full 3.x template.",
            dependsOn: dep);
        dep = actionId;

        // Update Web.config HTTP modules
        actionId = "update-webconfig-modules";
        builder.AddManualStepAction(
            actionId,
            "Update Web.config HTTP modules",
            "In Web.config: " +
            "remove TelemetryCorrelationHttpModule entries (from both <system.web><httpModules> and <system.webServer><modules>). " +
            "Verify ApplicationInsightsWebTracking (ApplicationInsightsHttpModule) is present. " +
            "Verify TelemetryHttpModule (OpenTelemetry.Instrumentation.AspNet.TelemetryHttpModule) is present — " +
            "this should have been added by the package upgrade.",
            dependsOn: dep);
        dep = actionId;

        // Remove satellite packages
        actionId = "remove-satellite-packages";
        builder.AddManualStepAction(
            actionId,
            "Remove satellite packages via Package Manager Console",
            "ASK THE USER to remove these satellite packages in this exact order via Package Manager Console " +
            "(dependents must be removed before their dependencies): " +
            "1. Uninstall-Package Microsoft.ApplicationInsights.WindowsServer; " +
            "2. Uninstall-Package Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel; " +
            "3. Uninstall-Package Microsoft.ApplicationInsights.DependencyCollector; " +
            "4. Uninstall-Package Microsoft.ApplicationInsights.PerfCounterCollector; " +
            "5. Uninstall-Package Microsoft.ApplicationInsights.Agent.Intercept; " +
            "6. Uninstall-Package Microsoft.AspNet.TelemetryCorrelation. " +
            "Only uninstall packages that exist in packages.config — skip any that are not present.",
            dependsOn: dep);
        dep = actionId;

        // Fix TelemetryConfiguration.Active usage
        actionId = "fix-telemetry-config-active";
        builder.AddManualStepAction(
            actionId,
            "Replace TelemetryConfiguration.Active with CreateDefault()",
            $"In {entryPoint} and any other files, replace TelemetryConfiguration.Active with " +
            "TelemetryConfiguration.CreateDefault(). Note: CreateDefault() returns a static singleton in 3.x. " +
            "Also replace any config.InstrumentationKey assignments with config.ConnectionString.",
            dependsOn: dep);
        dep = actionId;

        return dep;
    }

    protected override string AddRemovedMethodActions(
        OnboardingSpecBuilder builder, ServiceOptionsFindings opts,
        string entryPoint, string lastDependency)
    {
        // Classic ASP.NET doesn't have UseApplicationInsights() — go straight to shared methods
        return AddSharedRemovedMethodActions(builder, opts, entryPoint, EntryPointMethodName, lastDependency);
    }

    protected override void AddConnectionStringAction(OnboardingSpecBuilder builder, BrownfieldFindings? findings, string lastDependency)
    {
        // Classic ASP.NET uses <ConnectionString> in applicationinsights.config, not appsettings.json.
        // This is already handled by the "rewrite-appinsights-config" step — no additional action needed.
        // The user can also set APPLICATIONINSIGHTS_CONNECTION_STRING as env var.
    }
}
