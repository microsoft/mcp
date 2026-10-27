// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

internal sealed class DrillRunReprotectContractRequestPolicy(BinaryData content)
    : HttpPipelineSynchronousPolicy
{
    internal const string ApiVersion = ResilienceManagementApiVersions.Current;

    public override void OnSendingRequest(HttpMessage message)
    {
        string path = message.Request.Uri.ToUri().AbsolutePath;
        if (!path.Contains("/drillRuns/", StringComparison.OrdinalIgnoreCase) ||
            !path.EndsWith("/reprotect", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var query = System.Web.HttpUtility.ParseQueryString(message.Request.Uri.Query);
        query["api-version"] = ApiVersion;
        message.Request.Uri.Query = query.ToString() ?? string.Empty;
        message.Request.Content = RequestContent.Create(content);
        message.Request.Headers.SetValue("Content-Type", "application/json");
    }
}
