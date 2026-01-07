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

        var commandLineOptions = builder.Configuration.Get<CommandLineOptions>();

        if (commandLineOptions == null)
        {
            throw new InvalidOperationException("Expected to be able to get command line options from IConfiguration.");
        }

        var analyzer = host.Services.GetRequiredService<ToolAnalyzer>();

        await analyzer.RunAsync(DateTimeOffset.UtcNow, commandLineOptions.IsDryRun);
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });

        services.AddSingleton<IAzureMcpDatastore, AzureMcpKustoDatastore>()
            .AddSingleton<AzmcpProgram>()
            .AddSingleton<ToolAnalyzer>();

        services.Configure<AppConfiguration>(configuration.GetSection("AppConfig"))
            .PostConfigure<AppConfiguration>(existing =>
            {
                if (existing.WorkDirectory == null)
                {
                    string exeDir = AppContext.BaseDirectory;
                    var repoRoot = Utility.FindRepoRoot(exeDir);
                    existing.WorkDirectory = Path.Combine(repoRoot, ".work");
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
