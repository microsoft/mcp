// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Models;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

public interface IAzureRMDocsService
{
    Task<AzureRMDocsResult> GetDocumentationAsync(
        string resourceTypeName,
        string docType = "resource",
        string? argumentName = null,
        string? attributeName = null,
        CancellationToken cancellationToken = default);
}
