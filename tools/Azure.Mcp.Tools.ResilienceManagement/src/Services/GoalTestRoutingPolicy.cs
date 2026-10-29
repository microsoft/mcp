// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if DEBUG
using Azure.Core;
using Azure.Core.Pipeline;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

// Keep canary routing out of release builds and leave the recording transport intact.
internal sealed class GoalTestRoutingPolicy : HttpPipelineSynchronousPolicy
{
    public override void OnSendingRequest(HttpMessage message)
    {
        Uri uri = message.Request.Uri.ToUri();
        if (uri.Scheme != Uri.UriSchemeHttps ||
            (uri.Host != "management.azure.com" && uri.Host != "eastus2euap.management.azure.com"))
        {
            throw new InvalidOperationException("Goals canary tests require the public Azure ARM endpoint.");
        }

        message.Request.Uri.Host = "eastus2euap.management.azure.com";
        message.Request.Headers.SetValue("x-ms-rhub-routing-location", "eastus2euap");
    }
}
#endif
