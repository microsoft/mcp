// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Telemetry-safe reasons recorded in <see cref="TagName.ToolFailureMessage"/> when a tool call is rejected
/// before the command produces a response.
/// </summary>
public static class ToolFailureReason
{
    public const string ToolNotEnabled = "ToolNotEnabled";
    public const string ToolNotFound = "ToolNotFound";
    public const string ReadOnlyMode = "ReadOnlyMode";
    public const string LocalRequiredInHttpMode = "LocalRequiredInHttpMode";
    public const string InvalidArguments = "InvalidArguments";
    public const string ElicitationNotSupported = "ElicitationNotSupported";
    public const string ElicitationRejected = "ElicitationRejected";
    public const string ElicitationFailed = "ElicitationFailed";
}
