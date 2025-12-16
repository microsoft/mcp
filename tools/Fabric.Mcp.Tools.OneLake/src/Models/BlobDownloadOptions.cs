// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace Fabric.Mcp.Tools.OneLake.Models;

/// <summary>
/// Options that control how a blob should be downloaded from OneLake.
/// </summary>
public sealed class BlobDownloadOptions
{
    /// <summary>
    /// Gets or sets the destination stream that receives the blob content.
    /// When specified, the content is copied directly to the stream.
    /// </summary>
    public Stream? DestinationStream { get; init; }

    /// <summary>
    /// Gets or sets the local file path associated with the destination stream, when provided.
    /// This is propagated to the command result so the caller knows where the file was written.
    /// </summary>
    public string? LocalFilePath { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether inline content (base64/text) should be included in the result.
    /// </summary>
    public bool IncludeInlineContent { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum number of bytes that can be safely inlined in the response.
    /// When specified, inline content larger than this limit is suppressed to avoid oversized payloads.
    /// </summary>
    public long? InlineContentLimit { get; init; }
}
