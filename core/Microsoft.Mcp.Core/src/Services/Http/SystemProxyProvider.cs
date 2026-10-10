// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Microsoft.Mcp.Core.Services.Http;

/// <summary>
/// Provides the runtime system proxy separately from configuration-bound HTTP options.
/// </summary>
/// <param name="proxy">
/// The process proxy whose operating-system, PAC, and bypass rules are evaluated per destination.
/// </param>
/// <remarks>
/// This is a host service rather than an <see cref="HttpClientOptions"/> property because
/// <see cref="IWebProxy"/> is runtime infrastructure, not appsettings configuration data.
/// Tests can register an isolated instance without mutating <see cref="HttpClient.DefaultProxy"/>.
/// </remarks>
internal sealed class SystemProxyProvider(IWebProxy proxy)
{
    internal IWebProxy Proxy { get; } = proxy ?? throw new ArgumentNullException(nameof(proxy));
}
