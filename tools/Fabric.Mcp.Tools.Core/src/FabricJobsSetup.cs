// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core;

/// <summary>Exposes Jobs independently while reusing the Fabric services configured by the host's Core area.</summary>
public sealed class FabricJobsSetup() : IAreaSetup
{
    public string Name => "jobs";
    public string Title => "Microsoft Fabric Jobs";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ItemJobInstanceGetCommand>();
        services.AddSingleton<ItemJobRunCommand>(static serviceProvider =>
        {
            var command = new ItemJobRunCommand(
                serviceProvider.GetRequiredService<ILogger<ItemJobRunCommand>>(),
                serviceProvider.GetRequiredService<IFabricCoreService>());
            var definition = command.GetCommand();
            definition.Validators.Add(result =>
            {
                if (definition.Options.Any(option => result.GetResult(option) is { IdentifierTokenCount: > 1 }))
                {
                    result.AddError("Specify each job submission option at most once.");
                }
            });
            return command;
        });
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var jobs = new CommandGroup(Name,
            "Microsoft Fabric Jobs - Submit one on-demand item job or read one existing job instance using explicit IDs.\n" +
            "Run returns acceptance, not completion; it may consume compute or change data and never automatically retries.\n" +
            "Use its trustworthy job-instance ID in a separate Get call after waiting at least the returned Retry-After guidance.\n" +
            "Get returns status, timing, safe failure metadata, and retry guidance. Neither tool polls, waits, or retrieves results.\n" +
            "Job-instance IDs are not operation IDs. Cancellation and scheduling are not implemented.");
        jobs.AddCommand<ItemJobInstanceGetCommand>(serviceProvider);
        jobs.AddCommand<ItemJobRunCommand>(serviceProvider);
        return jobs;
    }
}
