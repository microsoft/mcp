
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
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
        }
        
        public void RegisterCommands(CommandGroup root, ILoggerFactory loggerFactory)
        {
            var startupsGroup = new CommandGroup("startups", "Commands for Microsoft for Startups");
            root.AddSubGroup(startupsGroup);

            // Guidance commands
            startupsGroup.AddCommand("get", new StartupsGuidanceCommand());
        }
    }
}
