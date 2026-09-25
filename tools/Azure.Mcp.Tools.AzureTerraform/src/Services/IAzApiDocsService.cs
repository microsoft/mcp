// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Models;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

public interface IAzApiDocsService
{
    AzApiDocsResult GetDocumentation(string resourceTypeName, string? apiVersion = null);
}
