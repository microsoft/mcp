// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.OneLake.Options;

public class BlobListOptions : BaseItemOptions
{
    public string? Path { get; set; }
    public bool Recursive { get; set; }
    public string? Format { get; set; }
}
