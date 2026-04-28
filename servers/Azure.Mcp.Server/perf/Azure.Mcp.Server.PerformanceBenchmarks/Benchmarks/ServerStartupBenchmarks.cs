// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Server;
using Azure.Mcp.Server.Perf;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Server.Perf.Benchmarks;

/// <summary>
/// Measures each phase of the Azure MCP Server startup pipeline in isolation so regressions
/// can be pinpointed to the specific phase that changed.
///
/// Run with:
///   dotnet run -c Release --project servers/Azure.Mcp.Server/perf/Azure.Mcp.Server.PerformanceBenchmarks -- --filter *
///
/// Quick smoke-run (shorter iteration count, useful in CI):
///   dotnet run -c Release ... -- --filter * --job short
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 5)]
public class ServerStartupBenchmarks
{
    // -------------------------------------------------------------------------
    // Phase 1 – instantiate all IAreaSetup objects (the 60+ toolset registrations)
    // -------------------------------------------------------------------------

    [Benchmark(Description = "RegisterAreas – instantiate all IAreaSetup objects")]
    public object RegisterAreas() => Program.RegisterAreas();

    // -------------------------------------------------------------------------
    // Phase 2 – wire the DI container (call all area.ConfigureServices)
    // -------------------------------------------------------------------------

    [Benchmark(Description = "ConfigureServices – register services into the DI container")]
    public IServiceCollection ConfigureServices() => CreateSilentServices();

    // -------------------------------------------------------------------------
    // Phase 3 – compile the DI container (BuildServiceProvider)
    // -------------------------------------------------------------------------

    [Benchmark(Description = "BuildServiceProvider – compile the DI container")]
    public ServiceProvider BuildServiceProvider() => CreateSilentServices().BuildServiceProvider();

    // -------------------------------------------------------------------------
    // Phase 4 – async service initialization (telemetry, user-agent policy, etc.)
    // -------------------------------------------------------------------------

    [Benchmark(Description = "InitializeServicesAsync – async service warm-up")]
    public async Task InitializeServicesAsync()
    {
        var sp = CreateSilentServices().BuildServiceProvider();
        await Program.InitializeServicesAsync(sp);
    }

    // -------------------------------------------------------------------------
    // Phase 5 – build the full System.CommandLine command tree
    // -------------------------------------------------------------------------

    [Benchmark(Description = "CommandFactory.RootCommand – build the full command tree")]
    public System.CommandLine.RootCommand BuildCommandTree()
    {
        var sp = CreateSilentServices().BuildServiceProvider();
        Program.InitializeServicesAsync(sp).GetAwaiter().GetResult();
        return sp.GetRequiredService<ICommandFactory>().RootCommand;
    }

    // -------------------------------------------------------------------------
    // Full pipeline – all phases combined (closest to real startup cost for
    // the first DI container in Program.Main)
    // -------------------------------------------------------------------------

    [Benchmark(Description = "FullStartup – all phases combined (DI build → command tree)")]
    public async Task<System.CommandLine.RootCommand> FullStartup()
    {
        var sp = CreateSilentServices().BuildServiceProvider();
        await Program.InitializeServicesAsync(sp);
        return sp.GetRequiredService<ICommandFactory>().RootCommand;
    }

    // -------------------------------------------------------------------------
    // Shared setup – registers all services with logging suppressed
    // -------------------------------------------------------------------------

    private static IServiceCollection CreateSilentServices()
    {
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.None));
        return services;
    }
}
