// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tests.Client.Attributes;

/// <summary>
/// Marks a test as live-only, meaning it cannot be recorded or played back.
/// Tests decorated with this attribute will be skipped when the test mode is
/// <see cref="TestMode.Playback"/> or <see cref="TestMode.Record"/>.
///
/// Use this attribute for tests that rely on non-HTTP protocols (e.g., WebSockets, TCP)
/// or have client-side limitations that prevent recording.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class LiveTestOnlyAttribute : Attribute;
