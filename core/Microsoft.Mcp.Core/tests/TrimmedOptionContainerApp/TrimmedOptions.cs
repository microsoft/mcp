// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace TrimmedOptionContainerApp;

internal class TrimmedOptions
{
    [OptionContainer<RetryPolicyOptions>(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
