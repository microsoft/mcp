// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Models;

/// <summary>
/// An artifact after it has been written to the caller's working directory.
/// </summary>
/// <param name="Name">Safe materialized file name with whitespace normalized to hyphens.</param>
/// <param name="Path">Exact local filesystem path.</param>
/// <param name="MarkdownLink">
/// Safe clickable file link. Render this value verbatim rather than formatting <paramref name="Path"/>.
/// </param>
/// <param name="Format">Backend artifact format, when supplied.</param>
/// <param name="Description">Backend artifact description, when supplied.</param>
public sealed record WrittenArtifact(
    string Name,
    string Path,
    string MarkdownLink,
    string? Format,
    string? Description);
