// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.Mcp.Tools.Monitor.Services.Traces;

/// <summary>
/// Lets <c>AddDefaultPolicies</c> (azmcp User-Agent) and <c>ConfigureRetryPolicy</c> apply to a
/// hand-built pipeline — Trace Store has no generated SDK client.
/// </summary>
public class MonitorTracesClientOptions : ClientOptions;
