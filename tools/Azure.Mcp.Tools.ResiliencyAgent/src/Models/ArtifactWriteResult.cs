// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Models;

/// <summary>
/// The workspace-materialization result for one backend artifact.
/// </summary>
/// <param name="Artifact">The authoritative in-memory backend artifact.</param>
/// <param name="Name">The sanitized file name used to identify the artifact.</param>
/// <param name="WorkspaceArtifact">
/// The workspace file when materialization succeeded; otherwise <see langword="null"/>.
/// </param>
/// <param name="IntendedRelativePath">
/// The workspace-relative path assigned to this artifact set, including when materialization fails.
/// </param>
public sealed record ArtifactWriteResult(
    AgentArtifact Artifact,
    string Name,
    WrittenArtifact? WorkspaceArtifact,
    string? IntendedRelativePath = null);
