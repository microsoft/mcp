using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Generator for classic ASP.NET greenfield projects (.NET Framework).
/// Supports ASP.NET MVC, WebForms, and generic ASP.NET app types.
/// Uses Microsoft.ApplicationInsights.Web — zero code change onboarding via NuGet.
/// </summary>
public class AspNetClassicGreenfieldGenerator : IGenerator
{
    private readonly GeneratorConfig _config;

    public AspNetClassicGreenfieldGenerator()
    {
        _config = GeneratorConfigLoader.LoadConfig("aspnet-classic-greenfield");
    }

    public bool CanHandle(Analysis analysis)
    {
        var classicProjectCount = analysis.Projects
            .Count(p => p.AppType is AppType.AspNetClassic
                     or AppType.AspNetMvc
                     or AppType.AspNetWebForms);

        return analysis.Language == Language.DotNet
            && classicProjectCount == 1
            && analysis.State == InstrumentationState.Greenfield;
    }

    public OnboardingSpec Generate(Analysis analysis)
    {
        var project = analysis.Projects.First(p =>
            p.AppType is AppType.AspNetClassic or AppType.AspNetMvc or AppType.AspNetWebForms);
        var projectFile = project.ProjectFile;
        var entryPoint = project.EntryPoint ?? "Global.asax.cs";
        var projectDir = Path.GetDirectoryName(projectFile) ?? "";

        return new OnboardingSpecBuilder(analysis)
            .WithAgentPreExecuteInstruction(AgentPreExecuteInstruction)
            .WithDecision(
                Intents.Onboard,
                _config.Decision.Solution,
                _config.Decision.Rationale)
            .AddActionsFromConfig(_config, projectFile, entryPoint, projectDir)
            .Build();
    }
}
