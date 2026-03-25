// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Specifies how template files should be returned.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<TemplateOutput>))]
public enum TemplateOutput
{
    /// <summary>
    /// Returns all files in a single 'files' list for creating complete new projects.
    /// </summary>
    New,

    /// <summary>
    /// Separates files into 'functionFiles' and 'projectFiles' with merge instructions
    /// for adding functions to existing projects.
    /// </summary>
    Add
}
