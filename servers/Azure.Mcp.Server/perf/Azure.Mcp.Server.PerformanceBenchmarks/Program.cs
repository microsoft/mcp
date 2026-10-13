// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Server.Perf;
using BenchmarkDotNet.Running;

// --count-tools             : emit tool count + exact token count (names+descriptions)
// --mcp-startup <exe> [serverArgs...]      : time full MCP client initialize+tools/list via SDK (stdio)
// --mcp-startup-http <exe> [serverArgs...] : time full MCP client initialize+tools/list via SDK (HTTP transport)
// --mcp-resource <exe> [serverArgs...]     : sample CPU/memory/handles across idle/startup/discovery/steady/soak phases (HTTP transport)
// --mcp-dispatch-stdio <exe> [serverArgs...] : time ping + tools/list request overhead over stdio
// --mcp-dispatch-http <exe> [serverArgs...]  : time ping + tools/list request overhead over HTTP transport
// --mcp-concurrency <exe> [serverArgs...]     : sweep concurrency levels; throughput, latency, error rate, scaling efficiency (HTTP transport)
// (all other args)          : run BenchmarkDotNet benchmarks
if (args.Length > 0 && args[0] == "--count-tools")
{
    await ToolCountMeasurement.RunCountToolsAsync();
    return;
}

if (args.Length >= 2 && args[0] == "--mcp-startup")
{
    // args[1] = path to azmcp exe; args[2..] = server arguments (e.g. "server" "start" "--mode" "namespace")
    await McpClientStartupMeasurement.RunAsync(args[1], args.Length > 2 ? args[2..] : []);
    return;
}

if (args.Length >= 2 && args[0] == "--mcp-startup-http")
{
    // args[1] = path to azmcp exe; args[2..] = server arguments. The harness appends the
    // HTTP transport flags and measures startup over HttpClientTransport.
    await McpClientStartupMeasurement.RunHttpAsync(args[1], args.Length > 2 ? args[2..] : []);
    return;
}

if (args.Length >= 2 && args[0] == "--mcp-resource")
{
    // args[1] = path to azmcp exe; args[2..] = server arguments. The harness launches the
    // server over HTTP and samples process resource counters across each phase. Duration
    // and concurrency are controlled via PERF_* environment variables.
    await ResourceUsageMeasurement.RunAsync(args[1], args.Length > 2 ? args[2..] : []);
    return;
}

if (args.Length >= 2 && args[0] == "--mcp-dispatch-stdio")
{
    // args[1] = path to azmcp exe; args[2..] = server arguments. Times ping + tools/list
    // request overhead over stdio (issue #3119).
    await DispatchOverheadMeasurement.RunStdioAsync(args[1], args.Length > 2 ? args[2..] : []);
    return;
}

if (args.Length >= 2 && args[0] == "--mcp-dispatch-http")
{
    // args[1] = path to azmcp exe; args[2..] = server arguments. Times ping + tools/list
    // request overhead over the HTTP transport (issue #3119).
    await DispatchOverheadMeasurement.RunHttpAsync(args[1], args.Length > 2 ? args[2..] : []);
    return;
}

if (args.Length >= 2 && args[0] == "--mcp-concurrency")
{
    // args[1] = path to azmcp exe; args[2..] = server arguments. Sweeps concurrency levels
    // over HTTP and reports throughput, latency, error rate, and scaling efficiency (issue #3122).
    // Levels and durations are controlled via PERF_CONC_* environment variables.
    await ConcurrencyScalingMeasurement.RunAsync(args[1], args.Length > 2 ? args[2..] : []);
    return;
}

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
