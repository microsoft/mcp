// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Tests.Commands;

[CommandMetadata(
    Id = "76dc8348-d2a3-4fbd-874b-f44eae4b22f2",
    Name = "test-json-context",
    Title = "Test JSON Context",
    Description = "A command for testing source-generated serialization context resolution.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
internal sealed class JsonContextTestCommand<TResult>() : BaseCommand<EmptyOptions, TResult>
{
    public override JsonSerializerContext JsonContext => ResponseFieldSelectionJsonContext.Default;

    public override Task<CommandResponse> ExecuteAsync(
        CommandContext context, EmptyOptions options, CancellationToken cancellationToken) =>
        Task.FromResult(context.Response);

    internal void SetCommandResult(CommandContext context, TResult result) => SetResult(context, result);
}
