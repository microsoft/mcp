// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Microsoft.Mcp.Core.Tests.Options;

public sealed class CursorTestOptions() : ICursorOption
{
    [Option(Description = ICursorOption.CursorDescription)]
    public string? Cursor { get; set; }
}
