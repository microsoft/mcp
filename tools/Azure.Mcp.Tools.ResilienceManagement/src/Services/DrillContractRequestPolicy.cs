// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

// The beta.1 SDK omits recoveryPlanId from Drill PUT requests even though its
// persistable model contains the value. Preserve the validated wire payload.
internal sealed class DrillContractRequestPolicy(BinaryData? content = null)
    : HttpPipelineSynchronousPolicy
{
    public override void OnSendingRequest(HttpMessage message)
    {
        string path = message.Request.Uri.ToUri().AbsolutePath;
        if (!path.Contains(
                "/providers/Microsoft.AzureResilienceManagement/drills/",
                StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/drillResources", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/drillRuns", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var query = System.Web.HttpUtility.ParseQueryString(message.Request.Uri.Query);
        query["api-version"] = ResilienceManagementApiVersions.Current;
        message.Request.Uri.Query = query.ToString() ?? string.Empty;
        if (content is not null &&
            (message.Request.Method == RequestMethod.Put ||
             message.Request.Method == RequestMethod.Patch))
        {
            message.Request.Content = RequestContent.Create(content);
            message.Request.Headers.SetValue("Content-Type", "application/json");
        }
    }
}
