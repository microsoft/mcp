// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands;

internal sealed class SelectOutputSchemaTestOptions()
    : ISelectOption
{
    [Option(Description = "Response fields to include.")]
    public string? Select { get; set; }
}
