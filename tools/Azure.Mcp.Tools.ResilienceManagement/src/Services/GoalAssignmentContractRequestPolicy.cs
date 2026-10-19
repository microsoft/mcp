// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

internal sealed class GoalAssignmentContractRequestPolicy(BinaryData? content = null)
    : HttpPipelineSynchronousPolicy
{
    internal const string ApiVersion = "2026-10-31-preview";

    public override void OnSendingRequest(HttpMessage message)
    {
        string path = message.Request.Uri.ToUri().AbsolutePath;
        if (!path.Contains(
                "/providers/Microsoft.AzureResilienceManagement/goalAssignments/",
                StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/goalResources", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        message.Request.Uri.Query = $"api-version={ApiVersion}";
        if (content is not null && message.Request.Method == RequestMethod.Put)
        {
            message.Request.Content = RequestContent.Create(content);
        }
    }
}
