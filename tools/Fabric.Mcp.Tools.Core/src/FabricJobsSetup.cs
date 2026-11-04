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
        services.AddSingleton<ItemJobInstanceCancelCommand>(static serviceProvider =>
        {
            var command = new ItemJobInstanceCancelCommand(
                serviceProvider.GetRequiredService<ILogger<ItemJobInstanceCancelCommand>>(),
                serviceProvider.GetRequiredService<IFabricCoreService>());
            var definition = command.GetCommand();
            definition.Validators.Add(result =>
            {
                if (definition.Options.Any(option => result.GetResult(option) is { IdentifierTokenCount: > 1 }))
                {
                    result.AddError("Specify each job cancellation option at most once.");
                }
            });
            return command;
        });
        services.AddSingleton<ItemJobInstanceGetCommand>();
        services.AddSingleton<ItemJobInstanceListCommand>(static serviceProvider =>
        {
            var command = new ItemJobInstanceListCommand(
                serviceProvider.GetRequiredService<ILogger<ItemJobInstanceListCommand>>(),
                serviceProvider.GetRequiredService<IFabricCoreService>());
            var definition = command.GetCommand();
            definition.Validators.Add(result =>
            {
                if (definition.Options.Any(option => result.GetResult(option) is { IdentifierTokenCount: > 1 }))
                {
                    result.AddError("Specify each job listing option at most once.");
                }
            });
            return command;
        });
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
            "Microsoft Fabric Jobs - Submit, request cancellation of, read, or list item jobs using explicit IDs.\n" +
            "Run and Cancel require execution permission and consent. Run may consume compute or change data; Cancel does not promise rollback.\n" +
            "Run returns submission acceptance; Cancel returns cancellation requested, not a Cancelled job state. Neither automatically retries.\n" +
            "Use the known job-instance ID in a separate Get call after waiting at least the returned Retry-After guidance.\n" +
            "Get returns status, timing, safe failure metadata, and retry guidance. No tool polls, waits, or retrieves results.\n" +
            "List reads one page only; call again explicitly with the same workspace/item IDs and unchanged token for another page.\n" +
            "For most items the API reports up to 100 recently completed instances, not a 100-row page cap; active instances have no corresponding limit.\n" +
            "Listing does not identify an earlier ambiguous Run submission or authorize cancellation. HTTP success does not mean jobs succeeded.\n" +
            "Job-instance IDs are not operation IDs. Scheduling is not implemented.");
        jobs.AddCommand<ItemJobInstanceCancelCommand>(serviceProvider);
        jobs.AddCommand<ItemJobInstanceGetCommand>(serviceProvider);
        jobs.AddCommand<ItemJobInstanceListCommand>(serviceProvider);
        jobs.AddCommand<ItemJobRunCommand>(serviceProvider);
        return jobs;
    }
}
