// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.GalleryApplication;

public class GalleryApplicationGetOptions : BaseComputeOptions
{
    public string? Gallery { get; set; }

    public string? GalleryApplication { get; set; }
}
