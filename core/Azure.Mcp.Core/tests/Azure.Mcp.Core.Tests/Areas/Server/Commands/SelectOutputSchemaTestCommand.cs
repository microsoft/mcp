// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands;

[CommandMetadata(
    Id = "38011acd-a746-4766-a397-02ba6078d516",
    Name = "select-schema-get",
    Title = "Test Selectable Output Schema",
    Description = "A command for testing selectable structured results.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
internal sealed class SelectOutputSchemaTestCommand()
    : BaseCommand<SelectOutputSchemaTestOptions, SelectOutputSchemaCollection>
{
    public override JsonTypeInfo<SelectOutputSchemaCollection> ResultTypeInfo
        => SelectOutputSchemaJsonContext.Default.SelectOutputSchemaCollection;

    public override Task<CommandResponse> ExecuteAsync(
        CommandContext context, SelectOutputSchemaTestOptions options, CancellationToken cancellationToken)
    {
        SetResult(context, new([new("outer", 1, new("inner", 2))]));
        return Task.FromResult(context.Response);
    }
}
