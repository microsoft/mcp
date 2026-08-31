// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace TrimmedOptionContainerApp;

internal class InheritedTrimmedOptions : TrimmedOptions
{
    [OptionContainer<RetryPolicyOptions>(Prefix = "another-retry")]
    public RetryPolicyOptions? AnotherRetryPolicy { get; set; }
}