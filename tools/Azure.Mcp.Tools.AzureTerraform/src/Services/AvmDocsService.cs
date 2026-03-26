// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureTerraform.Models;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

public sealed class AvmDocsService(IHttpClientFactory httpClientFactory) : IAvmDocsService
{
    private const string AvailableModulesUrl =
        "https://raw.githubusercontent.com/Azure/Azure-Verified-Modules/main/docs/static/module-indexes/TerraformResourceModules.csv";

    private const string ModuleNameColumn = "ModuleName";
    private const string DescriptionColumn = "Description";
    private const string ModuleStatusColumn = "ModuleStatus";
    private const string ModuleRepoUrlColumn = "RepoURL";
    private const string ModuleStatusProposed = "Proposed";

    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24);

    private static readonly Lock CacheLock = new();
    private static List<AvmModule>? s_cachedModules;
    private static DateTime s_cacheTimestamp;

    public async Task<List<AvmModule>> ListModulesAsync(CancellationToken cancellationToken = default)
    {
        return await GetModuleCollectionAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<AvmVersion>> GetVersionsAsync(string moduleName, CancellationToken cancellationToken = default)
    {
        var modules = await GetModuleCollectionAsync(cancellationToken).ConfigureAwait(false);
        var module = modules.Find(m => string.Equals(m.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Module '{moduleName}' not found in available modules.");

        var apiUrl = module.RepoUrl
            .Replace("github.com", "api.github.com/repos", StringComparison.OrdinalIgnoreCase) + "/releases";

        var client = httpClientFactory.CreateClient();
        ConfigureGitHubHeaders(client);

        var response = await client.GetAsync(new Uri(apiUrl), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var releases = JsonSerializer.Deserialize(json, AvmJsonContext.Default.ListGitHubRelease) ?? [];

        var versions = new List<AvmVersion>(releases.Count);
        foreach (var release in releases)
        {
            var tagName = release.TagName.TrimStart('v');
            versions.Add(new AvmVersion
            {
                TagName = tagName,
                CreatedAt = release.CreatedAt,
                TarballUrl = release.TarballUrl
            });
        }

        versions.Sort((a, b) => string.Compare(b.CreatedAt, a.CreatedAt, StringComparison.Ordinal));
        return versions;
    }

    public async Task<string> GetDocumentationAsync(string moduleName, string moduleVersion, CancellationToken cancellationToken = default)
    {
        var modules = await GetModuleCollectionAsync(cancellationToken).ConfigureAwait(false);
        var module = modules.Find(m => string.Equals(m.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Module '{moduleName}' not found in available modules.");

        var cleanVersion = moduleVersion.TrimStart('v');

        // Fetch README.md directly from GitHub raw content
        var repoPath = ExtractRepoPath(module.RepoUrl);
        var readmeUrl = $"https://raw.githubusercontent.com/{repoPath}/v{cleanVersion}/README.md";

        var client = httpClientFactory.CreateClient();
        ConfigureGitHubHeaders(client);

        var response = await client.GetAsync(new Uri(readmeUrl), cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            // Try without 'v' prefix
            readmeUrl = $"https://raw.githubusercontent.com/{repoPath}/{cleanVersion}/README.md";
            response = await client.GetAsync(new Uri(readmeUrl), cancellationToken).ConfigureAwait(false);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"No README.md found for module '{moduleName}' version '{moduleVersion}'. HTTP {(int)response.StatusCode}");
        }

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<AvmModule>> GetModuleCollectionAsync(CancellationToken cancellationToken)
    {
        lock (CacheLock)
        {
            if (s_cachedModules is not null && DateTime.UtcNow - s_cacheTimestamp < CacheExpiration)
            {
                return s_cachedModules;
            }
        }

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Azure-Terraform-MCP-Server");

        var response = await client.GetAsync(new Uri(AvailableModulesUrl), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var csvContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var modules = ParseModuleCsv(csvContent);

        lock (CacheLock)
        {
            s_cachedModules = modules;
            s_cacheTimestamp = DateTime.UtcNow;
        }

        return modules;
    }

    internal static List<AvmModule> ParseModuleCsv(string csvContent)
    {
        var lines = csvContent.Split('\n');
        if (lines.Length == 0)
        {
            return [];
        }

        var headerLine = lines[0].Trim();
        if (string.IsNullOrEmpty(headerLine))
        {
            return [];
        }

        var headers = headerLine.Split(',');
        for (int i = 0; i < headers.Length; i++)
        {
            headers[i] = headers[i].Trim().Trim('"');
        }

        var nameIdx = Array.IndexOf(headers, ModuleNameColumn);
        var descIdx = Array.IndexOf(headers, DescriptionColumn);
        var statusIdx = Array.IndexOf(headers, ModuleStatusColumn);
        var repoUrlIdx = Array.IndexOf(headers, ModuleRepoUrlColumn);

        if (nameIdx < 0 || repoUrlIdx < 0)
        {
            return [];
        }

        var modules = new List<AvmModule>();

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var values = ParseCsvLine(line);

            if (statusIdx >= 0 && statusIdx < values.Count &&
                string.Equals(values[statusIdx], ModuleStatusProposed, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var moduleName = nameIdx < values.Count ? values[nameIdx] : string.Empty;
            var repoUrl = repoUrlIdx < values.Count ? values[repoUrlIdx] : string.Empty;

            if (string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(repoUrl))
            {
                continue;
            }

            modules.Add(new AvmModule
            {
                ModuleName = moduleName,
                Description = descIdx >= 0 && descIdx < values.Count ? values[descIdx] : string.Empty,
                RepoUrl = repoUrl,
                Source = SourceFromRepoUrl(repoUrl)
            });
        }

        return modules;
    }

    internal static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString().Trim());
        return values;
    }

    internal static string SourceFromRepoUrl(string repoUrl)
    {
        // Input: https://github.com/Azure/terraform-azurerm-avm-res-apimanagement-service
        // Output: Azure/avm-res-apimanagement-service/azurerm
        var parts = repoUrl.Split('/');
        var githubOrg = parts.Length >= 2 ? parts[^2] : "Azure";
        var moduleRepoName = parts.Length >= 1 ? parts[^1] : string.Empty;

        var nameParts = moduleRepoName.Split('-');
        var moduleOrg = nameParts.Length >= 2 ? nameParts[1] : "azurerm";
        var moduleName = nameParts.Length >= 3 ? string.Join('-', nameParts[2..]) : moduleRepoName;

        return $"{githubOrg}/{moduleName}/{moduleOrg}";
    }

    private static string ExtractRepoPath(string repoUrl)
    {
        // Extract "Azure/terraform-azurerm-avm-res-storage-storageaccount" from
        // "https://github.com/Azure/terraform-azurerm-avm-res-storage-storageaccount"
        var uri = new Uri(repoUrl);
        return uri.AbsolutePath.TrimStart('/');
    }

    private static void ConfigureGitHubHeaders(HttpClient client)
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Azure-Terraform-MCP-Server");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github.v3+json");
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrEmpty(githubToken))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", githubToken);
        }
    }
}

internal sealed class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("tarball_url")]
    public string TarballUrl { get; set; } = string.Empty;
}

[JsonSerializable(typeof(List<GitHubRelease>))]
[JsonSerializable(typeof(GitHubRelease))]
internal partial class AvmJsonContext : JsonSerializerContext;
