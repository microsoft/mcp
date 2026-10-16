// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.GalleryApplication;

public class GalleryApplicationCreateOptions : BaseComputeOptions
{
    public string? Gallery { get; set; }

    public string? GalleryApplication { get; set; }

    public string? Location { get; set; }

    public string? Tags { get; set; }
}
