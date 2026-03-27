// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands.Descriptors;

/// <summary>
/// Determines which inherited options a command receives automatically.
/// </summary>
public enum CommandKind
{
    /// <summary>
    /// No inherited options beyond what the command explicitly declares.
    /// </summary>
    Basic,

    /// <summary>
    /// Inherits global options: tenant, auth-method, retry policy.
    /// </summary>
    Global,

    /// <summary>
    /// Inherits global options plus subscription.
    /// </summary>
    Subscription
}
