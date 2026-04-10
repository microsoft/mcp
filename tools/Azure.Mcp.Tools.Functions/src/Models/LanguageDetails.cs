// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents the complete language details returned by the get languages list command,
/// combining language info with runtime version details and global runtime metadata.
/// </summary>
public sealed class LanguageDetails
{
    public required string Language { get; init; }

    public required LanguageInfo Info { get; init; }

    public required RuntimeVersionInfo RuntimeVersions { get; init; }
}
