// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

// GoalMembersData inherits read-only ARM id serialization. The service requires that id in updateGoalResources,
// but the beta.1 SDK omits it in wire format. Preserve the validated payload for this POST only.
internal sealed class GoalResourceUpdateRequestPolicy(BinaryData content) : HttpPipelineSynchronousPolicy
{
    public override void OnSendingRequest(HttpMessage message)
    {
        if (message.Request.Method == RequestMethod.Post &&
            message.Request.Uri.ToUri().AbsolutePath.EndsWith("/updateGoalResources", StringComparison.OrdinalIgnoreCase))
        {
            message.Request.Content = RequestContent.Create(content);
        }
    }
}
