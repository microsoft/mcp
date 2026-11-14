// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Models;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

/// <summary>
/// Writes one completed artifact set into a timestamped folder under the caller's working directory.
/// </summary>
/// <remarks>
/// Every set is staged and moved into place as a unit, so multi-file packages keep their canonical
/// filenames and relative references. Separate results receive separate UTC-stamped folders.
/// </remarks>
public interface IArtifactWriter
{
    /// <summary>
    /// Attempts to write all usable artifacts atomically and returns both the authoritative in-memory
    /// artifacts and their optional workspace files. If any write fails, no partial set is published.
    /// </summary>
    IReadOnlyList<ArtifactWriteResult> Write(IEnumerable<AgentArtifact> artifacts, string conversationId);
}
