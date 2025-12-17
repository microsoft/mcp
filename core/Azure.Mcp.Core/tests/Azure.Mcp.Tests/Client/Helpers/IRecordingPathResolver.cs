// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Azure.Mcp.Tests.Client.Helpers;

/// <summary>
/// Abstraction for resolving recording asset paths and session directories.
/// Enables tests to substitute custom paths when exercising record/playback infrastructure.
/// </summary>
public interface IRecordingPathResolver
{
    string RepositoryRoot { get; }

    string GetSessionDirectory(Type testType, string? variantSuffix = null);

    string GetAssetsJson(Type testType);
}
