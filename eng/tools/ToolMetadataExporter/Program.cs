// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ToolMetadataExporter.Models.Kusto;
using ToolMetadataExporter.Services;

namespace ToolMetadataExporter;

public class Program
{
    private readonly AzmcpProgram _azmcpExe;
    private readonly IAzureMcpDatastore _azureMcpDatastore;
    private readonly ILogger<Program> _logger;

    public Program(AzmcpProgram program, IAzureMcpDatastore azureMcpDatastore, ILogger<Program> logger)
    {
        _azmcpExe = program;
        _azureMcpDatastore = azureMcpDatastore;
        _logger = logger;
    }

    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        ConfigureServices(builder.Services);

        var host = builder.Build();

        var program = host.Services.GetRequiredService<Program>();

        await program.RunAsync(DateTimeOffset.UtcNow, isDryRun: true);
        await host.RunAsync();
    }

    private static void ConfigureServices(IServiceCollection services)
    {

        services.AddSingleton<IAzureMcpDatastore, AzureMcpKustoDatastore>()
            .AddSingleton<AzmcpProgram>();

        services.AddOptions<AppConfiguration>()
            .PostConfigure(existing =>
            {
                if (existing.WorkDirectory == null)
                {
                    string exeDir = AppContext.BaseDirectory;
                    var repoRoot = Utility.FindRepoRoot(exeDir);
                    existing.WorkDirectory = Path.Combine(repoRoot, ".work");
                }
            });

        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
    }

    private async Task RunAsync(DateTimeOffset analysisTime, bool isDryRun, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting analysis.");

        var currentTools = await _azmcpExe.LoadToolsDynamicallyAsync();

        if (currentTools == null)
        {
            _logger.LogError("LoadToolsDynamicallyAsync did not return a result.");
            return;
        }
        else if (currentTools.Tools == null || currentTools.Tools.Count == 0)
        {
            _logger.LogWarning("azmcp program did not return any tools.");
            return;
        }

        var existingTools = (await _azureMcpDatastore.GetAvailableToolsAsync(cancellationToken)).ToDictionary(x => x.ToolId);

        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Analysis was cancelled.");
            return;
        }

        var changes = new List<McpToolEvent>();

        foreach (var tool in currentTools.Tools)
        {
            if (string.IsNullOrEmpty(tool.Id))
            {
                throw new InvalidOperationException($"Tool without an id. Name: {tool.Name}. Command: {tool.Command}");
            }

            if (!existingTools.TryGetValue(tool.Id, out var knownValue))
            {
               
            }
            else
            {
                changes.Add(new McpToolEvent
                {
                    EventTime = analysisTime,
                    EventType = McpToolEventType.Created,
                });
            }
        }

        await _azureMcpDatastore.AddToolEventsAsync(changes, cancellationToken);
    }
}
