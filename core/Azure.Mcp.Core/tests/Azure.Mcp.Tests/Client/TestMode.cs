// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tests.Client;

/// <summary>
/// Execution mode for tests integrating with the test proxy.
/// </summary>
public enum TestMode
{
    Live,
    Record,
    Playback
}
