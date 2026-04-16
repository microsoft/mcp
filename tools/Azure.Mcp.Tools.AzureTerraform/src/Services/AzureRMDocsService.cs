// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Models;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

public sealed class AzureRMDocsService(IHttpClientFactory httpClientFactory) : IAzureRMDocsService
{
    private const string BaseResourcesUrl =
        "https://raw.githubusercontent.com/hashicorp/terraform-provider-azurerm/main/website/docs/r";
    private const string BaseDataSourcesUrl =
        "https://raw.githubusercontent.com/hashicorp/terraform-provider-azurerm/main/website/docs/d";

    public async Task<AzureRMDocsResult> GetDocumentationAsync(
        string resourceTypeName,
        string docType = "resource",
        string? argumentName = null,
        string? attributeName = null,
        CancellationToken cancellationToken = default)
    {
        // Normalize resource type — remove azurerm_ prefix if present
        string normalizedType = resourceTypeName.ToLowerInvariant().Replace("azurerm_", "", StringComparison.Ordinal);

        bool isDataSource = docType.Equals("data-source", StringComparison.OrdinalIgnoreCase)
            || docType.Equals("datasource", StringComparison.OrdinalIgnoreCase)
            || docType.Equals("data_source", StringComparison.OrdinalIgnoreCase);

        string docUrl = isDataSource
            ? $"{BaseDataSourcesUrl}/{normalizedType}.html.markdown"
            : $"{BaseResourcesUrl}/{normalizedType}.html.markdown";

        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept", "text/plain");
        client.DefaultRequestHeaders.Add("User-Agent", "Azure-Terraform-MCP-Server");

        string? markdownContent = null;
        bool isDataSourceUrl = isDataSource;

        var response = await client.GetAsync(new Uri(docUrl), cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            // Try the other type as fallback
            string fallbackUrl = isDataSource
                ? $"{BaseResourcesUrl}/{normalizedType}.html.markdown"
                : $"{BaseDataSourcesUrl}/{normalizedType}.html.markdown";

            var fallbackResponse = await client.GetAsync(new Uri(fallbackUrl), cancellationToken).ConfigureAwait(false);
            if (fallbackResponse.IsSuccessStatusCode)
            {
                markdownContent = await fallbackResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                docUrl = fallbackUrl;
                isDataSourceUrl = fallbackUrl.Contains("docs/d/", StringComparison.Ordinal);
            }
            else
            {
                return new AzureRMDocsResult
                {
                    ResourceType = resourceTypeName,
                    DocumentationUrl = docUrl,
                    Summary = $"Documentation not found for {resourceTypeName} (HTTP {(int)response.StatusCode}). " +
                        "Please double-check the resource type name is correct. " +
                        "If this resource is not available in the AzureRM provider, consider using the AzAPI provider instead."
                };
            }
        }

        markdownContent ??= await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var result = new AzureRMDocsResult
        {
            ResourceType = resourceTypeName,
            DocumentationUrl = docUrl,
            Summary = AzureRMDocsParser.ExtractSummary(markdownContent, resourceTypeName, isDataSourceUrl),
            Arguments = AzureRMDocsParser.ExtractArguments(markdownContent, isDataSourceUrl),
            Attributes = AzureRMDocsParser.ExtractAttributes(markdownContent),
            Examples = AzureRMDocsParser.ExtractExamples(markdownContent, normalizedType, isDataSourceUrl),
            Notes = AzureRMDocsParser.ExtractNotes(markdownContent)
        };

        // Populate block argument definitions
        var blockDefinitions = AzureRMDocsParser.ExtractBlockDefinitions(markdownContent);
        foreach (var arg in result.Arguments)
        {
            if (arg.Type == "Block" && blockDefinitions.TryGetValue(arg.Name, out var blockArgs))
            {
                arg.BlockArguments = blockArgs;
            }
        }

        if (!string.IsNullOrEmpty(argumentName))
        {
            result = FilterToArgument(result, argumentName);
        }

        if (!string.IsNullOrEmpty(attributeName))
        {
            result = FilterToAttribute(result, attributeName);
        }

        return result;
    }

    private static AzureRMDocsResult FilterToArgument(AzureRMDocsResult result, string argumentName)
    {
        var matching = result.Arguments
            .Where(a => a.Name.Equals(argumentName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        result.Arguments = matching;
        result.Summary = matching.Count > 0
            ? $"Argument details for '{argumentName}' in {result.ResourceType}"
            : $"Argument '{argumentName}' not found in {result.ResourceType}";

        return result;
    }

    private static AzureRMDocsResult FilterToAttribute(AzureRMDocsResult result, string attributeName)
    {
        var matching = result.Attributes
            .Where(a => a.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        result.Attributes = matching;
        result.Summary = matching.Count > 0
            ? $"Attribute details for '{attributeName}' in {result.ResourceType}"
            : $"Attribute '{attributeName}' not found in {result.ResourceType}";

        return result;
    }
}
