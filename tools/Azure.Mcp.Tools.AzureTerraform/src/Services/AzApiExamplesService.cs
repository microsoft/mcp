// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureTerraform.Models;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

public sealed class AzApiExamplesService(IHttpClientFactory httpClientFactory) : IAzApiExamplesService
{
    private const string RawContentBase =
        "https://raw.githubusercontent.com/Azure/template-reference-generator/main/settings/remarks";

    public async Task<List<AzApiExample>> GetExamplesAsync(
        string resourceTypeName,
        CancellationToken cancellationToken = default)
    {
        // Extract namespace from resource type (e.g., "microsoft.compute" from "Microsoft.Compute/virtualMachines")
        string[] parts = resourceTypeName.Split('/');
        if (parts.Length < 2 || string.IsNullOrEmpty(parts[0]))
        {
            return [];
        }

        string @namespace = parts[0].ToLowerInvariant();

        // Fetch the remarks.json index for this namespace
        RemarksJson? remarksIndex = await FetchRemarksIndexAsync(@namespace, cancellationToken).ConfigureAwait(false);
        if (remarksIndex?.TerraformSamples is null || remarksIndex.TerraformSamples.Count == 0)
        {
            return [];
        }

        // Find samples matching this resource type, deduplicate by path
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var matchingSamples = new List<TerraformSampleEntry>();

        foreach (var sample in remarksIndex.TerraformSamples)
        {
            if (sample.ResourceType.Equals(resourceTypeName, StringComparison.OrdinalIgnoreCase)
                && seenPaths.Add(sample.Path))
            {
                matchingSamples.Add(sample);
            }
        }

        if (matchingSamples.Count == 0)
        {
            return [];
        }

        // Fetch content for each matching sample
        var examples = new List<AzApiExample>();
        foreach (var sample in matchingSamples)
        {
            string? content = await FetchExampleContentAsync(@namespace, sample.Path, cancellationToken).ConfigureAwait(false);
            if (content != null)
            {
                examples.Add(new AzApiExample
                {
                    Description = sample.Description,
                    Content = content,
                    SourcePath = $"settings/remarks/{@namespace}/{sample.Path}"
                });
            }
        }

        return examples;
    }

    private async Task<RemarksJson?> FetchRemarksIndexAsync(
        string @namespace,
        CancellationToken cancellationToken)
    {
        string remarksUrl = $"{RawContentBase}/{@namespace}/remarks.json";

        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Azure-Terraform-MCP-Server");

        try
        {
            var response = await client.GetAsync(new Uri(remarksUrl), cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize(json, AzApiExamplesJsonContext.Default.RemarksJson);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<string?> FetchExampleContentAsync(
        string @namespace,
        string samplePath,
        CancellationToken cancellationToken)
    {
        string contentUrl = $"{RawContentBase}/{@namespace}/{samplePath}";

        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Azure-Terraform-MCP-Server");
        client.DefaultRequestHeaders.Add("Accept", "text/plain");

        try
        {
            var response = await client.GetAsync(new Uri(contentUrl), cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return null;
        }
    }
}

internal sealed class RemarksJson
{
    [JsonPropertyName("TerraformSamples")]
    public List<TerraformSampleEntry>? TerraformSamples { get; set; }
}

internal sealed class TerraformSampleEntry
{
    [JsonPropertyName("ResourceType")]
    public string ResourceType { get; set; } = string.Empty;

    [JsonPropertyName("Path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;
}

[JsonSerializable(typeof(RemarksJson))]
[JsonSerializable(typeof(List<TerraformSampleEntry>))]
internal sealed partial class AzApiExamplesJsonContext : JsonSerializerContext;
