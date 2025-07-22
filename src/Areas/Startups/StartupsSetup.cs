// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using AzureMcp.Areas.Startups.Commands;
using AzureMcp.Areas.Startups.Commands.Guidance;
using AzureMcp.Areas.Startups.Services;
using AzureMcp.Areas.Storage.Services;
using AzureMcp.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Areas.Startups
{
    public class StartupsSetup : IAreaSetup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IStartupsService, StartupsService>();
            services.AddSingleton<IStorageService, StorageService>();
        }

        public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
        {
            var startupsGroup = new CommandGroup("startups", "Commands for Microsoft for Startups");
            rootGroup.AddSubGroup(startupsGroup);

            // Register guidance command
            startupsGroup.AddCommand("get", new StartupsGuidanceCommand(loggerFactory.CreateLogger<StartupsGuidanceCommand>()));

            // Register deploy command
            startupsGroup.AddCommand("deploy", new StartupsDeployCommand(loggerFactory.CreateLogger<StartupsDeployCommand>()));
        }
    }
}