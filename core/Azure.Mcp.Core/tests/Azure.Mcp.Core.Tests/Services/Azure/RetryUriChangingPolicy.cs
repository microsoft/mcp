// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;

namespace Azure.Mcp.Core.Tests.Services.Azure;

internal sealed class RetryUriChangingPolicy : HttpPipelineSynchronousPolicy
{
    private int _attempts;

    public override void OnSendingRequest(HttpMessage message)
    {
        if (Interlocked.Increment(ref _attempts) > 1)
        {
            message.Request.Uri.Reset(new Uri("https://evil.example"));
        }
    }
}
