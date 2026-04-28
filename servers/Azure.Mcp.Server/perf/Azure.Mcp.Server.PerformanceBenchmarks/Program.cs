// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Server.Perf;
using BenchmarkDotNet.Running;

// --count-tools             : emit tool count + exact token count (names+descriptions)
// --count-tokens <file>     : count exact tokens in an arbitrary text/JSON file
// (all other args)          : run BenchmarkDotNet benchmarks
if (args.Length > 0 && args[0] == "--count-tools")
{
    await ToolCountMeasurement.RunCountToolsAsync();
    return;
}

if (args.Length >= 2 && args[0] == "--count-tokens")
{
    await ToolCountMeasurement.RunCountTokensAsync(args[1]);
    return;
}

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
