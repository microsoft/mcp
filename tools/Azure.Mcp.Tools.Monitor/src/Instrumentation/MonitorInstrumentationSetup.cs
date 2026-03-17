// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.MonitorInstrumentation.Commands;
using Azure.Mcp.Tools.MonitorInstrumentation.Detectors;
using Azure.Mcp.Tools.MonitorInstrumentation.Generators;
using Azure.Mcp.Tools.MonitorInstrumentation.Pipeline;
using Azure.Mcp.Tools.MonitorInstrumentation.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.MonitorInstrumentation;

public sealed class MonitorInstrumentationSetup : IAreaSetup
{
    public string Name => "monitorinstrumentation";

    public string Title => "Azure Monitor Instrumentation";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILanguageDetector, DotNetLanguageDetector>();
        services.AddSingleton<IAppTypeDetector, DotNetAppTypeDetector>();
        services.AddSingleton<IInstrumentationDetector, DotNetInstrumentationDetector>();

        services.AddSingleton<IGenerator, AspNetCoreGreenfieldGenerator>();
        services.AddSingleton<IGenerator, AspNetCoreBrownfieldGenerator>();

        services.AddSingleton<WorkspaceAnalyzer>();
        services.AddSingleton<OrchestratorTool>();
        services.AddSingleton<SendBrownfieldAnalysisTool>();

        services.AddSingleton<ListLearningResourcesCommand>();
        services.AddSingleton<GetLearningResourceCommand>();
        services.AddSingleton<OrchestratorStartCommand>();
        services.AddSingleton<OrchestratorNextCommand>();
        services.AddSingleton<SendBrownfieldAnalysisCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var group = new CommandGroup(
            Name,
            "Azure Monitor instrumentation operations for orchestrated onboarding and migration steps.",
            Title);

        group.AddCommand("list_learning_resources", serviceProvider.GetRequiredService<ListLearningResourcesCommand>());
        group.AddCommand("get_learning_resource", serviceProvider.GetRequiredService<GetLearningResourceCommand>());
        group.AddCommand("orchestrator_start", serviceProvider.GetRequiredService<OrchestratorStartCommand>());
        group.AddCommand("orchestrator_next", serviceProvider.GetRequiredService<OrchestratorNextCommand>());
        group.AddCommand("send_brownfield_analysis", serviceProvider.GetRequiredService<SendBrownfieldAnalysisCommand>());

        return group;
    }
}
