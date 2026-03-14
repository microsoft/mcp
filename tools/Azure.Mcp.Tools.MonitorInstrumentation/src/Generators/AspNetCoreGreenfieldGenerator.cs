using Azure.Mcp.Tools.MonitorInstrumentation.Models;
using static Azure.Mcp.Tools.MonitorInstrumentation.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Generators;

/// <summary>
/// Generator for ASP.NET Core greenfield projects (no existing telemetry)
/// </summary>
public class AspNetCoreGreenfieldGenerator : IGenerator
{
    private readonly GeneratorConfig _config;

    public AspNetCoreGreenfieldGenerator()
    {
        _config = GeneratorConfigLoader.LoadConfig("aspnetcore-greenfield");
    }

    public bool CanHandle(Analysis analysis)
    {
        // Single ASP.NET Core project, greenfield
        var aspNetCoreProjects = analysis.Projects
            .Where(p => p.AppType == AppType.AspNetCore)
            .ToList();

        return analysis.Language == Language.DotNet
            && aspNetCoreProjects.Count == 1
            && analysis.State == InstrumentationState.Greenfield;
    }

    public OnboardingSpec Generate(Analysis analysis)
    {
        var project = analysis.Projects.First(p => p.AppType == AppType.AspNetCore);
        var projectFile = project.ProjectFile;
        var entryPoint = project.EntryPoint ?? "Program.cs";
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
