// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Microsoft.Mcp.Core.Tests.Options;

public sealed class CursorTestOptions() : ICursorOption
{
    [Option(Description = "Opaque continuation token.", AllowEmptyOrWhiteSpaceString = true)]
    public string? Cursor { get; set; }
}
