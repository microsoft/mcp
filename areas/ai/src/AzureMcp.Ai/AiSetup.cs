using AzureMcp.Ai.Commands.Completions;
using AzureMcp.Ai.Services;
using AzureMcp.Core.Areas;
using AzureMcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Ai;

public class AiSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAiService, AiService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var ai = new CommandGroup("ai",
            "AI operations - Commands that interact with Azure OpenAI and related AI services.");
        rootGroup.AddSubGroup(ai);

        var completions = new CommandGroup("completions",
            "Text completions operations for Azure OpenAI deployments.");
        ai.AddSubGroup(completions);

        completions.AddCommand("create",
            new CompletionsCreateCommand(loggerFactory.CreateLogger<CompletionsCreateCommand>()));
    }
}