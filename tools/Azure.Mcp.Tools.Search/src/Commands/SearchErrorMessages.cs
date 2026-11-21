// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Search.Commands;

internal static class SearchErrorMessages
{
    internal static string Authorization(RequestFailedException exception, string permissions) =>
        $"Azure AI Search returned {exception.Status}. Check the authenticated identity and use --tenant to select the Search service's tenant. " +
        "These data-plane commands access the service directly and do not require subscription-wide discovery permissions. " +
        $"{permissions} Check that role-based access is enabled and that the service's network rules allow this request. " +
        "For permission and authentication guidance, see https://learn.microsoft.com/azure/search/search-security-rbac " +
        $"and https://aka.ms/azmcp/auth. Details: {exception.Message}";

    internal const string Definitions =
        "This operation requires permission to read Search object definitions at the resource scope; Search Index Data Reader alone does not grant that permission.";

    internal const string Query =
        "This command reads the index definition before querying documents. It requires definition-read permission as well as document-query permission, such as Search Index Data Reader, at the appropriate resource scope.";

    internal const string Retrieval =
        "This command reads the knowledge-base definition before retrieval. It requires definition-read permission as well as content-retrieval permission, such as Search Index Data Reader, at the appropriate resource scope.";
}
