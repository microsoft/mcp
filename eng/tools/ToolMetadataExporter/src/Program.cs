// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Kusto.Ingest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToolMetadataExporter.Models;
using ToolMetadataExporter.Services;

namespace ToolMetadataExporter;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        ConfigureServices(builder.Services, builder.Configuration);
        ConfigureAzureServices(builder.Services);

        var host = builder.Build();
        var analyzer = host.Services.GetRequiredService<ToolAnalyzer>();

        await analyzer.RunAsync(DateTimeOffset.UtcNow);
    }

    private static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });

        services.AddSingleton<IAzureMcpDatastore, AzureMcpKustoDatastore>()
            .AddSingleton<Utility>()
            .AddSingleton<AzmcpProgram>()
            .AddSingleton<ToolAnalyzer>()
            .AddSingleton<RunInformation>();

        services.AddOptions<CommandLineOptions>()
            .Bind(configuration);

        services.AddOptions<AppConfiguration>()
            .Bind<AppConfiguration>(configuration.GetSection("AppConfig"))
            .Configure<IOptions<CommandLineOptions>>((existing, commandLineOptions) =>
            {
                // Command-line IsDryRun overrides appsettings.json file value.
                if (commandLineOptions.Value.IsDryRun.HasValue)
                {
                    existing.IsDryRun = commandLineOptions.Value.IsDryRun.Value;
                }

                var exeDir = AppContext.BaseDirectory;

                // If a path to azmcp.exe is not provided. Assume that this is running within the context of
                // the repository and try to find it.
                var isAzmcpExeSpecified = !string.IsNullOrEmpty(commandLineOptions.Value.AzmcpExe);
                if (isAzmcpExeSpecified)
                {
                    existing.AzmcpExe = commandLineOptions.Value.AzmcpExe!;

                    existing.WorkDirectory ??= exeDir;
                }
                else
                {
                    var repoRoot = Utility.FindRepoRoot(exeDir);
                    existing.WorkDirectory ??= Path.Combine(repoRoot, ".work");

                    existing.AzmcpExe = Path.Combine(repoRoot, "eng", "tools", "Azmcp", "azmcp.exe");
                }
            });
    }

    private static void ConfigureAzureServices(IServiceCollection services)
    {
        services.AddScoped<TokenCredential>(sp =>
        {
            var credential = new ChainedTokenCredential(
                new ManagedIdentityCredential(),
                new DefaultAzureCredential()
            );

            return credential;
        });
        services.AddSingleton<ICslQueryProvider>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<AppConfiguration>>();

            var connectionStringBuilder = new KustoConnectionStringBuilder(config.Value.QueryEndpoint)
                .WithAadUserPromptAuthentication()
                .WithAadAzCliAuthentication(interactive: true);

            return KustoClientFactory.CreateCslQueryProvider(connectionStringBuilder);
        });
        services.AddSingleton<IKustoIngestClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<AppConfiguration>>();

            var connectionStringBuilder = new KustoConnectionStringBuilder(config.Value.IngestionEndpoint)
                .WithAadUserPromptAuthentication()
                .WithAadAzCliAuthentication(interactive: true);

            return KustoIngestFactory.CreateDirectIngestClient(connectionStringBuilder);
        });
    }
}
