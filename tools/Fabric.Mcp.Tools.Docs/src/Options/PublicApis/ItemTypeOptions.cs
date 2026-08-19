// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Docs.Options.PublicApis;

public sealed class ItemTypeOptions
{
    [Option(Description = "The Microsoft Fabric item type (e.g. 'notebook', 'lakehouse', 'dataPipeline', 'report', 'warehouse'). Also accepts the non-item API areas 'platform', 'admin', 'spark' and 'realTimeIntelligence'. Call the 'list-item-types' tool for the full list of supported values.")]
    public required string ItemType { get; set; }
}
