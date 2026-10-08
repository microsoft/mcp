// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Server;
using Azure.Mcp.Server.Perf;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Server.Perf.Benchmarks;

/// <summary>
/// Measures each phase of the Azure MCP Server startup pipeline in isolation so regressions
/// can be pinpointed to the specific phase that changed.
///
/// Each phase's prerequisites are built in a per-target <see cref="IterationSetupAttribute"/>
/// hook so they are excluded from the timed body — e.g. the BuildServiceProvider benchmark
/// times only <c>BuildServiceProvider()</c>, not the ConfigureServices work that precedes it.
/// To keep setup/measurement one-to-one, the job runs a single invocation per iteration
/// (InvocationCount = 1, UnrollFactor = 1), which is the pattern BenchmarkDotNet requires
/// when IterationSetup establishes non-idempotent state.
///
/// Run with:
///   dotnet run -c Release --project servers/Azure.Mcp.Server/perf/Azure.Mcp.Server.PerformanceBenchmarks -- --filter *
/// </summary>
[Config(typeof(IsolatedPhaseConfig))]
public class ServerStartupBenchmarks
{
    // Per-phase prerequisites, prepared in IterationSetup so they are not part of the
    // timed body. Each is disposed in DisposeProviders after the iteration is recorded.
    private IServiceCollection _servicesForConfigure = null!;
    private IServiceCollection _servicesForBuild = null!;
    private ServiceProvider? _builtProvider;
    private ServiceProvider? _providerForInitialize;
    private ServiceProvider? _providerForCommandTree;

    private sealed class IsolatedPhaseConfig : ManualConfig
    {
        public IsolatedPhaseConfig()
        {
            AddJob(Job.Default
                .WithLaunchCount(1)
                .WithWarmupCount(2)
                .WithIterationCount(10)
                .WithInvocationCount(1)
                .WithUnrollFactor(1));
            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }

    [IterationCleanup]
    public async Task DisposeProviders()
    {
        // Providers may resolve async-only-disposable services (e.g. CosmosService implements
        // IAsyncDisposable but not IDisposable), for which a synchronous Dispose() throws.
        // Dispose asynchronously in the cleanup hook, outside the timed benchmark body.
        await (_builtProvider?.DisposeAsync() ?? ValueTask.CompletedTask);
        await (_providerForInitialize?.DisposeAsync() ?? ValueTask.CompletedTask);
        await (_providerForCommandTree?.DisposeAsync() ?? ValueTask.CompletedTask);
        _builtProvider = null;
        _providerForInitialize = null;
        _providerForCommandTree = null;
    }

    // -------------------------------------------------------------------------
    // Phase 1 – instantiate all IAreaSetup objects (the 60+ toolset registrations)
    // -------------------------------------------------------------------------

    [Benchmark(Description = "RegisterAreas – instantiate all IAreaSetup objects")]
    public object RegisterAreas() => Program.RegisterAreas();

    // -------------------------------------------------------------------------
    // Phase 2 – wire the DI container (call all area.ConfigureServices)
    // -------------------------------------------------------------------------

    [IterationSetup(Target = nameof(ConfigureServices))]
    public void SetupConfigureServices() => _servicesForConfigure = new ServiceCollection();

    [Benchmark(Description = "ConfigureServices – register services into the DI container")]
    public IServiceCollection ConfigureServices()
    {
        Program.ConfigureServices(_servicesForConfigure);
        return _servicesForConfigure;
    }

    // -------------------------------------------------------------------------
    // Phase 3 – compile the DI container (BuildServiceProvider)
    // -------------------------------------------------------------------------

    [IterationSetup(Target = nameof(BuildServiceProvider))]
    public void SetupBuildServiceProvider() => _servicesForBuild = CreateSilentServices();

    [Benchmark(Description = "BuildServiceProvider – compile the DI container")]
    public ServiceProvider BuildServiceProvider()
    {
        _builtProvider = _servicesForBuild.BuildServiceProvider();
        return _builtProvider;
    }

    // -------------------------------------------------------------------------
    // Phase 4 – async service initialization (telemetry, user-agent policy, etc.)
    // -------------------------------------------------------------------------

    [IterationSetup(Target = nameof(InitializeServicesAsync))]
    public void SetupInitializeServices() => _providerForInitialize = CreateSilentServices().BuildServiceProvider();

    [Benchmark(Description = "InitializeServicesAsync – async service warm-up")]
    public async Task InitializeServicesAsync() => await Program.InitializeServicesAsync(_providerForInitialize!);

    // -------------------------------------------------------------------------
    // Phase 5 – build the full System.CommandLine command tree
    // -------------------------------------------------------------------------

    [IterationSetup(Target = nameof(BuildCommandTree))]
    public void SetupBuildCommandTree()
    {
        _providerForCommandTree = CreateSilentServices().BuildServiceProvider();
        Program.InitializeServicesAsync(_providerForCommandTree).GetAwaiter().GetResult();
    }

    [Benchmark(Description = "CommandFactory.RootCommand – build the full command tree")]
    public System.CommandLine.RootCommand BuildCommandTree() =>
        _providerForCommandTree!.GetRequiredService<ICommandFactory>().RootCommand;

    // -------------------------------------------------------------------------
    // Full pipeline – DI build + service init + command tree.
    // This is the cumulative end-to-end measurement, so ConfigureServices/BuildServiceProvider
    // remain inside the timed body by design. RegisterAreas runs once as a static initializer
    // and is NOT included here (it has its own dedicated benchmark above).
    // -------------------------------------------------------------------------

    [Benchmark(Description = "FullStartup – DI build + service init + command tree (excludes RegisterAreas static init)")]
    public async Task<System.CommandLine.RootCommand> FullStartup()
    {
        await using var sp = CreateSilentServices().BuildServiceProvider();
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

