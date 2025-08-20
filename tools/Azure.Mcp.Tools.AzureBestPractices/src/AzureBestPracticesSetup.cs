// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AzureBestPractices.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureBestPractices;

public class AzureBestPracticesSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Register Azure Best Practices command at the root level
        var bestPractices = new CommandGroup(
            "bestpractices",
            "Returns secure, production-grade Azure best practices. Call this before generating Azure SDK code."
        );
        rootGroup.AddSubGroup(bestPractices);

        bestPractices.AddCommand(
            "get",
            new BestPracticesCommand(loggerFactory.CreateLogger<BestPracticesCommand>())
        );
    }
}
