// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands.Descriptors;

/// <summary>
/// Determines which inherited options a command receives automatically.
/// </summary>
public enum InheritOptions
{
    Basic,
    Global,
    Subscription
}