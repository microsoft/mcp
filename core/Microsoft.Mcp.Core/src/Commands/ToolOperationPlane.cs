// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Identifies the Azure API plane used by a tool.
/// </summary>
[System.Text.Json.Serialization.JsonConverter(typeof(ToolOperationPlaneJsonConverter))]
public enum ToolOperationPlane
{
    /// <summary>The tool has not yet been classified.</summary>
    Unspecified,

    /// <summary>The tool operates against an Azure service data-plane API.</summary>
    Data,

    /// <summary>The tool operates against an Azure management or control-plane API.</summary>
    Control,

    /// <summary>The tool operates against both Azure data-plane and control-plane APIs.</summary>
    Both,

    /// <summary>The Azure API plane distinction does not apply to the tool.</summary>
    NotApplicable
}
