// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
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
        services.AddSingleton<ItemScheduleCreateCommand>(static serviceProvider =>
        {
            var command = new ItemScheduleCreateCommand(
                serviceProvider.GetRequiredService<ILogger<ItemScheduleCreateCommand>>(),
                serviceProvider.GetRequiredService<IFabricCoreService>());
            ConfigureScheduleMutationOptions(command.GetCommand(), "creation");
            return command;
        });
        services.AddSingleton<ItemScheduleGetCommand>(static serviceProvider =>
        {
            var command = new ItemScheduleGetCommand(
                serviceProvider.GetRequiredService<ILogger<ItemScheduleGetCommand>>(),
                serviceProvider.GetRequiredService<IFabricCoreService>());
            var definition = command.GetCommand();
            definition.Validators.Add(result =>
            {
                if (definition.Options.Any(option => result.GetResult(option) is { IdentifierTokenCount: > 1 }))
                {
                    result.AddError("Specify each schedule lookup option at most once.");
                }
            });
            return command;
        });
        services.AddSingleton<ItemScheduleListCommand>(static serviceProvider =>
        {
            var command = new ItemScheduleListCommand(
                serviceProvider.GetRequiredService<ILogger<ItemScheduleListCommand>>(),
                serviceProvider.GetRequiredService<IFabricCoreService>());
            var definition = command.GetCommand();
            definition.Validators.Add(result =>
            {
                if (definition.Options.Any(option => result.GetResult(option) is { IdentifierTokenCount: > 1 }))
                {
                    result.AddError("Specify each schedule listing option at most once.");
                }
            });
            return command;
        });
        services.AddSingleton<ItemScheduleUpdateCommand>(static serviceProvider =>
        {
            var command = new ItemScheduleUpdateCommand(
                serviceProvider.GetRequiredService<ILogger<ItemScheduleUpdateCommand>>(),
                serviceProvider.GetRequiredService<IFabricCoreService>());
            ConfigureScheduleMutationOptions(command.GetCommand(), "update");
            return command;
        });
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var jobs = new CommandGroup(Name,
            "Microsoft Fabric Jobs - Submit, request cancellation of, read, or list item jobs, and get, list, create, or update schedules using explicit context.\n" +
            "Run and Cancel require execution permission and consent. Run may consume compute or change data; Cancel does not promise rollback.\n" +
            "Run returns submission acceptance; Cancel returns cancellation requested, not a Cancelled job state. Neither automatically retries.\n" +
            "Use the known job-instance ID in a separate Get call after waiting at least the returned Retry-After guidance.\n" +
            "Get Job Instance returns status, timing, safe failure metadata, and retry guidance. No tool polls, waits, or retrieves results.\n" +
            "List Job Instances reads one page only; call again explicitly with the same workspace/item IDs and unchanged token for another page.\n" +
            "For most items the API reports up to 100 recently completed instances, not a 100-row page cap; active instances have no corresponding limit.\n" +
            "Listing does not identify an earlier ambiguous Run submission or authorize cancellation. HTTP success does not mean jobs succeeded.\n" +
            "List Schedules reads one page for workspace/item IDs and an explicit job type; continuation requires the same context and unchanged token.\n" +
            "Get Schedule reads one snapshot for explicit workspace/item/schedule IDs and job type, without listing or enrichment. Read permission and disclosure consent are required, even in read-only mode.\n" +
            "A schedule snapshot is not job execution status or proof of an ambiguous mutation outcome, and reading never authorizes a mutation.\n" +
            "Create Schedule sends one POST with explicit enabled and configuration, returning creation, not job execution or success.\n" +
            "Update Schedule sends one PATCH for an explicit schedule ID with enabled and full configuration; no hidden Get or merge.\n" +
            "Updating is not job execution; disabling is not cancellation. Omitted execution data has no documented preservation or clearing guarantee.\n" +
            "Before consenting, confirm timing: an enabled schedule with a past start may execute immediately, consuming compute or changing data.\n" +
            "Creation and updates require Fabric permissions and delegated Item.Execute.All plus Item.ReadWrite.All. No preflight or automatic retry is performed.\n" +
            "Schedule settings, owner, and execution data may be sensitive. Cron intervals are minutes, not cron expressions. No timezone conversion is performed.\n" +
            "A schedule's owner identifies its creator or last modifier, separately from item ownership. Confirm the calling identity before updates.\n" +
            "Schedule IDs, job-instance IDs, and operation IDs are distinct. Schedule deletion is not implemented.");
        jobs.AddCommand<ItemJobInstanceCancelCommand>(serviceProvider);
        jobs.AddCommand<ItemJobInstanceGetCommand>(serviceProvider);
        jobs.AddCommand<ItemJobInstanceListCommand>(serviceProvider);
        jobs.AddCommand<ItemScheduleCreateCommand>(serviceProvider);
        jobs.AddCommand<ItemScheduleGetCommand>(serviceProvider);
        jobs.AddCommand<ItemScheduleListCommand>(serviceProvider);
        jobs.AddCommand<ItemScheduleUpdateCommand>(serviceProvider);
        jobs.AddCommand<ItemJobRunCommand>(serviceProvider);
        return jobs;
    }

    private static void ConfigureScheduleMutationOptions(Command definition, string operation)
    {
        if (definition.Options.Single(static option => option.Name == "--enabled") is not Option<bool> enabled ||
            !enabled.Required)
        {
            throw new InvalidOperationException($"Schedule {operation} requires Boolean --enabled without a default.");
        }
        // A bool flag's implicit false default is not explicit scheduling intent.
        enabled.DefaultValueFactory = null;
        enabled.Arity = ArgumentArity.ExactlyOne;
        definition.Validators.Add(result =>
        {
            if (definition.Options.Any(option => result.GetResult(option) is { IdentifierTokenCount: > 1 }))
            {
                result.AddError($"Specify each schedule {operation} option at most once.");
            }
            var value = result.GetResult(enabled);
            if (value is null || value.Implicit || value.Tokens.Count != 1 || !bool.TryParse(value.Tokens[0].Value, out _))
            {
                result.AddError("Specify --enabled with an explicit true or false value.");
            }
        });
    }
}
