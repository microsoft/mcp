// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Models.Instrumentation;
using static Azure.Mcp.Tools.Monitor.Models.Instrumentation.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Instrumentation.Generators;

/// <summary>
/// Generator for ASP.NET Core greenfield projects (no existing telemetry)
/// </summary>
public class AspNetCoreGreenfieldGenerator : IGenerator
{
    private readonly GeneratorConfig _config = GeneratorConfigLoader.LoadConfig("aspnetcore-greenfield");

    public bool CanHandle(Analysis analysis)
    {
        // Single ASP.NET Core project, greenfield
        var aspNetCoreProjects = analysis.Projects
            .Count(p => p.AppType == AppType.AspNetCore);

        return analysis.Language == Language.DotNet
            && aspNetCoreProjects == 1
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
            .WithDecision(Intents.Onboard, _config.Decision.Solution, _config.Decision.Rationale)
            .AddActionsFromConfig(_config, projectFile, entryPoint, projectDir)
            .Build();
    }
}
