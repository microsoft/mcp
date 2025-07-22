// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using AzureMcp.Areas.Startups.Commands;
using AzureMcp.Areas.Startups.Commands.Guidance;
using AzureMcp.Areas.Startups.Services;
using AzureMcp.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Areas.Startups
{
    public class StartupsSetup : IAreaSetup
    {

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IStartupsServices, StartupsServices>();
            services.AddTransient<StartupsGuidanceCommand>();
            services.AddTransient<StartupsDeployCommand>();
        }

        public void RegisterCommands(CommandGroup root, IServiceProvider serviceProvider)
        {
            var startupsGroup = new CommandGroup("startups", "Commands for Microsoft for Startups");
            root.AddSubGroup(startupsGroup);

            // Guidance command
            startupsGroup.AddCommand("get", ActivatorUtilities.CreateInstance<StartupsGuidanceCommand>(
            serviceProvider));

            // Deploy command
            startupsGroup.AddCommand("deploy", ActivatorUtilities.CreateInstance<StartupsDeployCommand>(
            serviceProvider));
        }

        public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
        {}
    }
}