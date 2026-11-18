// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Tests.Commands;

[CommandMetadata(
    Id = "76dc8348-d2a3-4fbd-874b-f44eae4b22f2",
    Name = "test-result-type-info",
    Title = "Test Result Type Info",
    Description = "A command for testing source-generated result metadata.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
internal sealed class ResultTypeInfoTestCommand() : BaseCommand<EmptyOptions, ExceptionResult>
{
    public override JsonTypeInfo<ExceptionResult>? ResultTypeInfo => CoreJsonContext.Default.ExceptionResult;

    public override Task<CommandResponse> ExecuteAsync(
        CommandContext context, EmptyOptions options, CancellationToken cancellationToken) =>
        Task.FromResult(context.Response);

    internal void SetCommandResult(CommandContext context, ExceptionResult result) => SetResult(context, result);
}
