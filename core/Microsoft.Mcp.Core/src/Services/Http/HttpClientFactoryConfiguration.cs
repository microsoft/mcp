// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Http;

/// <summary>
/// Preserves the last shared HTTP defaults registration for startup posture reporting.
/// </summary>
/// <param name="RecordingProxyResolver">
/// The debug-only callback used during handler creation. Startup reporting must not invoke
/// it: recording fixtures can supply request-scoped or not-yet-initialized proxy state.
/// </param>
/// <remarks>
/// This is registration metadata, not a cached transport decision. Effective HTTP options
/// and recording environment settings are evaluated by the same logic as handler creation.
/// Named custom transports and usage of the explicit unprotected client are not represented.
/// </remarks>
internal sealed record HttpClientFactoryConfiguration(Func<Uri?>? RecordingProxyResolver);
