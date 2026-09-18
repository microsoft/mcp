// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Gallery;

/// <summary>
/// Options for the GalleryCreate command.
/// </summary>
public sealed class GalleryCreateOptions : ISubscriptionOption
{
    [Option(Description = ComputeOptionDescriptions.GalleryName, Aliases = ["name"])]
    public required string Gallery { get; set; }

    [Option(Description = ComputeOptionDescriptions.Location, Aliases = ["l"])]
    public string? Location { get; set; }

    [Option(Description = ComputeOptionDescriptions.GalleryDescription)]
    public string? Description { get; set; }

    [Option(Description = ComputeOptionDescriptions.Tags)]
    public string? Tags { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
