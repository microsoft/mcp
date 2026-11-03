// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
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
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var jobs = new CommandGroup(Name,
            "Microsoft Fabric Jobs - Read one existing item job instance by workspace, item, and job-instance UUIDs.\n" +
            "Returns its status, invocation type, available timing and trace fields, safe failure metadata, and retry guidance.\n" +
            "Makes one read without polling, waiting, retrieving results, or starting, canceling, or scheduling jobs.");
        jobs.AddCommand<ItemJobInstanceGetCommand>(serviceProvider);
        return jobs;
    }
}
