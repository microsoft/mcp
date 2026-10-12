// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Server;
using Azure.Mcp.Server.Perf.Dispatch;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Server.Perf.Benchmarks;

/// <summary>
/// Measures the Azure MCP Server's own per-call dispatch overhead in isolation (issue #3119):
/// the cost of routing a tool name to a command, parsing MCP arguments, binding and validating
/// options, running the execution frame, and serializing the response — all with a deterministic
/// <see cref="NoOpCommand"/> so no Azure or network latency is included.
///
/// Each layer is a separate benchmark so a regression can be attributed to the responsible layer.
/// <c>[MemoryDiagnoser]</c> also records allocations per layer.
///
/// Run with:
///   dotnet run -c Release --project servers/Azure.Mcp.Server/perf/Azure.Mcp.Server.PerformanceBenchmarks `
///     -- --filter '*CommandDispatch*' --buildTimeout 600
/// </summary>
[MemoryDiagnoser]
public class CommandDispatchBenchmarks
{
    // A real, registered, Azure-free-to-look-up command name used to measure routing cost.
    private const string RealToolName = "subscription_list";

    private ServiceProvider _serviceProvider = null!;
    private ICommandFactory _commandFactory = null!;
    private NoOpCommand _noop = null!;
    private IBaseCommand _noopBase = null!;
    private Command _noopSysCommand = null!;
    private Dictionary<string, JsonElement> _args = null!;
    private ParseResult _parseResult = null!;
    private NoOpCommand.NoOpOptions _boundOptions = null!;
    private CommandResponse _response = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        // Build the full server so routing measures a realistic command map (~500+ commands).
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        _serviceProvider = services.BuildServiceProvider();
        await Program.InitializeServicesAsync(_serviceProvider);
        _commandFactory = _serviceProvider.GetRequiredService<ICommandFactory>();

        _noop = new NoOpCommand();
        _noopBase = _noop;
        _noopSysCommand = _noop.GetCommand();

        _args = new Dictionary<string, JsonElement>
        {
            ["value"] = JsonSerializer.SerializeToElement("hello"),
            ["count"] = JsonSerializer.SerializeToElement(3),
        };

        _noopSysCommand.TryParseFromDictionary(_args, out var parseResult, out _);
        _parseResult = parseResult!;
        _boundOptions = _noop.BindOptions(_parseResult);

        // A populated response for the serialization benchmark.
        _response = await _noopBase.ExecuteAsync(new CommandContext(), _parseResult, CancellationToken.None);
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        // The full DI container holds services that only implement IAsyncDisposable
        // (e.g. CosmosService), so it must be disposed asynchronously.
        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
        }
    }

    [Benchmark(Description = "Routing – CommandFactory tool-name lookup")]
    public IBaseCommand? Routing() => _commandFactory.FindCommandByName(RealToolName);

    [Benchmark(Description = "Parse – MCP arguments dictionary to ParseResult")]
    public bool Parse() => _noopSysCommand.TryParseFromDictionary(_args, out _, out _);

    [Benchmark(Description = "Bind – ParseResult to options object")]
    public NoOpCommand.NoOpOptions Bind() => _noop.BindOptions(_parseResult);

    [Benchmark(Description = "Validate – option validation")]
    public bool Validate()
    {
        var validationResult = new ValidationResult();
        _noop.ValidateOptions(_boundOptions, validationResult);
        return validationResult.IsValid;
    }

    [Benchmark(Description = "SerializeResponse – CommandResponse to MCP JSON")]
    public string SerializeResponse()
        => JsonSerializer.Serialize(_response, ModelsJsonContext.Default.CommandResponse);

    [Benchmark(Description = "ExecuteFrame – bind + validate + execute + capture")]
    public async Task<CommandResponse> ExecuteFrame()
        => await _noopBase.ExecuteAsync(new CommandContext(), _parseResult, CancellationToken.None);

    [Benchmark(Description = "FullDispatch – parse + bind + validate + execute + serialize")]
    public async Task<string> FullDispatch()
    {
        _noopSysCommand.TryParseFromDictionary(_args, out var parseResult, out _);
        var response = await _noopBase.ExecuteAsync(new CommandContext(), parseResult!, CancellationToken.None);
        return JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse);
    }
}
