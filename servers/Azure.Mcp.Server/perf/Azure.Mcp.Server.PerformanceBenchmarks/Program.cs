// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Server.Perf;
using BenchmarkDotNet.Running;

// --count-tools             : emit tool count + exact token count (names+descriptions)
// --mcp-startup <exe> [serverArgs...] : time full MCP client initialize+tools/list via SDK
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

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
