// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Server.Perf.Dispatch;

/// <summary>
/// A deterministic no-op command used to benchmark the server's own dispatch overhead
/// (issue #3119) — argument binding, validation, execution frame, and response
/// serialization — with zero Azure or network work. It is instantiated directly by the
/// dispatch benchmarks and is <b>not</b> registered with the production server, so it never
/// appears in the real tool surface.
/// </summary>
[CommandMetadata(
    Id = "0a5f5f5f-0000-0000-0000-0000000000ff",
    Name = "noop",
    Title = "No-op benchmark command",
    Description = "Deterministic no-op used to measure server dispatch overhead. Not registered in production.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class NoOpCommand : BaseCommand<NoOpCommand.NoOpOptions, NoOpCommand.NoOpResult>
{
    public override JsonTypeInfo<NoOpResult>? ResultTypeInfo => PerfDispatchJsonContext.Default.NoOpResult;

    public override Task<CommandResponse> ExecuteAsync(
        CommandContext context, NoOpOptions options, CancellationToken cancellationToken)
    {
        SetResult(context, new NoOpResult(options.Value ?? "ok", options.Count ?? 0));
        return Task.FromResult(context.Response);
    }

    public sealed class NoOpOptions
    {
        [Option(Name = "value", Description = "An echoed string value.")]
        public string? Value { get; set; }

        [Option(Name = "count", Description = "An echoed integer value.")]
        public int? Count { get; set; }
    }

    public sealed record NoOpResult(string Echo, int Count);
}
