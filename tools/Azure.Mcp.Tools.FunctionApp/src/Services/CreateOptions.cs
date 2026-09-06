// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FunctionApp.Services;

/// <summary>
/// Resolved settings used to provision a Function App and its dependencies.
/// </summary>
public readonly record struct CreateOptions(
    string Runtime,
    string? RuntimeVersion,
    HostingKind HostingKind,
    bool RequiresLinux,
    string? ExplicitSku,
    string? ExplicitOs,
    bool UseManagedIdentityStorage = true);
