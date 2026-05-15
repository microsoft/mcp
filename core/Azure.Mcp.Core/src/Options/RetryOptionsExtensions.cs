// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Azure.Core;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Core.Options;

public static class RetryOptionsExtensions
{
    public static void ConfigureRetryOptions(this ClientOptions clientOptions, RetryPolicyOptions? retryPolicy)
    {
        if (retryPolicy == null)
        {
            return;
        }

        if (retryPolicy.MaxRetries.HasValue)
        {
            clientOptions.Retry.MaxRetries = retryPolicy.MaxRetries.Value;
        }

        if (retryPolicy.Mode.HasValue)
        {
            clientOptions.Retry.Mode = retryPolicy.Mode.Value;
        }

        if (retryPolicy.DelaySeconds.HasValue)
        {
            clientOptions.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.DelaySeconds.Value);
        }

        if (retryPolicy.MaxDelaySeconds.HasValue)
        {
            clientOptions.Retry.MaxDelay = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds.Value);
        }

        if (retryPolicy.NetworkTimeoutSeconds.HasValue)
        {
            clientOptions.Retry.NetworkTimeout = TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds.Value);
        }
    }
}
