// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Models;

/// <summary>
/// One artifact the agent produced during a conversation, already decoded into text.
/// The A2A response carries artifacts inline, so nothing is fetched separately.
/// </summary>
/// <param name="Name">File name as the agent named it.</param>
/// <param name="Description">The agent's own description of the artifact, when it supplied one.</param>
/// <param name="MimeType">Reported media type. Unreliable - the agent labels Bicep as JSON - so it is
/// carried for information only and never used to decide how content is handled.</param>
/// <param name="Content">Decoded text content.</param>
/// <param name="Format">Artifact family from the agent's metadata, such as arm, bicep or terraform.</param>
/// <param name="Family">Materialization family used to group one completed artifact set.</param>
/// <param name="SourceKind">Original A2A part kind: file, text, or data.</param>
/// <param name="SourceDataJson">Exact structured DataPart JSON when the source kind is data.</param>
/// <param name="MetadataJson">Exact artifact metadata JSON when supplied by the backend.</param>
public sealed record AgentArtifact(
    string Name,
    string? Description,
    string? MimeType,
    string? Content,
    string? Format = null,
    string? Family = null,
    string? SourceKind = null,
    string? SourceDataJson = null,
    string? MetadataJson = null);
