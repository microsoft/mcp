// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Skills.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Skills;

public class SkillsSetup : IAreaSetup
{
    public string Name => "skill";
    public string Title => "Azure Skills Telemetry";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<TelemetryPublishCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var skill = new CommandGroup(
            Name,
            "Azure Skills - Commands for publishing skills-related telemetry events from agent hooks. " +
            "Use these commands to emit usage metrics from clients like VS Code, Claude, or Copilot CLI.",
            Title
        );

        var telemetry = new CommandGroup(
            "telemetry",
            "Skills telemetry operations - Commands for publishing telemetry events from agent hooks."
        );
        skill.AddSubGroup(telemetry);

        var publishCommand = serviceProvider.GetRequiredService<TelemetryPublishCommand>();
        telemetry.AddCommand(publishCommand.Name, publishCommand);

        return skill;
    }
}
