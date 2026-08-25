// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace TrimmedOptionContainerApp;

internal sealed class TrimmedOptions
{
    [OptionContainer<RetryPolicyOptions>(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
