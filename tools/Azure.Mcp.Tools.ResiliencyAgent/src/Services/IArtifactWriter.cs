// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Models;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

/// <summary>
/// Writes the artifacts a conversation produced into the caller's working directory.
/// </summary>
/// <remarks>
/// Generated templates and reports are far more useful as files the user can open, edit and deploy
/// than as text inside a chat reply, and returning paths instead of content keeps a large report out
/// of the model's context. This mirrors the pattern the Azure Migrate platform landing zone tool
/// already uses: write to the current directory and tell the caller where the file is.
/// </remarks>
public interface IArtifactWriter
{
    /// <summary>
    /// Writes each artifact and returns where it landed. Artifacts with no content are skipped.
    /// </summary>
    IReadOnlyList<WrittenArtifact> Write(IEnumerable<AgentArtifact> artifacts, string conversationId);
}
