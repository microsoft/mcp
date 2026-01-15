// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Policy.Options;

/// <summary>
/// Base options type for policy-related commands.
/// </summary>
/// <remarks>
/// This class currently does not add properties beyond <see cref="SubscriptionOptions"/>,
/// but it exists as a dedicated extension point for future policy commands to share
/// common option behavior without impacting non-policy tools.
/// </remarks>
public class BasePolicyOptions : SubscriptionOptions
{
}
