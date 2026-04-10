// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.Functions.Commands;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Services.Helpers;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Functions.Services;

/// <summary>
/// Service for Azure Functions template operations.
/// </summary>
public sealed class FunctionsService(
    IHttpClientFactory httpClientFactory,
    ILanguageMetadataProvider languageMetadata,
    IManifestService manifestService,
    ICacheService cacheService,
    ILogger<FunctionsService> logger) : IFunctionsService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILanguageMetadataProvider _languageMetadata = languageMetadata ?? throw new ArgumentNullException(nameof(languageMetadata));
    private readonly IManifestService _manifestService = manifestService ?? throw new ArgumentNullException(nameof(manifestService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<FunctionsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string CacheGroup = "functions-templates";

    private const string DefaultBranch = "main";
    private const long MaxFileSizeBytes = 1_048_576; // 1 MB
    private const long MaxTreeSizeBytes = 5_242_880; // 5 MB for tree API response

    private const string FunctionTemplateMergeInstructions =
        """
        ## Merging Template Files with Existing Project

        **Project files** (host.json, local.settings.json, etc.) may already exist if you used `functions project get`.
        - **local.settings.json**: Merge new "Values" entries with existing ones. Do not overwrite existing connection strings.
        - **host.json**: Keep existing extensionBundle settings. Merge other configuration sections.
        - **requirements.txt / package.json / pom.xml**: Add new dependencies, avoid duplicates.
        - **.funcignore**: Merge ignore patterns, avoid duplicates.

        **Function files** are new files to add to the project:
        - Python: Add/merge function code into `function_app.py`
        - TypeScript: Place files in `src/functions/`
        - Java: Place files in `src/main/java/com/function/`
        - C#: Place files in the project root alongside the .csproj
        """;

    public async Task<LanguageListResult> GetLanguageListAsync(CancellationToken cancellationToken = default)
    {
        // Fetch manifest to get runtime versions
        var manifest = await _manifestService.FetchManifestAsync(cancellationToken);
        var runtimeVersions = manifest.RuntimeVersions;

        var languages = new List<LanguageDetails>();

        foreach (var kvp in _languageMetadata.GetAllLanguages(runtimeVersions))
        {
            languages.Add(new LanguageDetails
            {
                Language = kvp.Key,
                Info = kvp.Value,
                RuntimeVersions = kvp.Value.RuntimeVersions
            });
        }

        var result = new LanguageListResult
        {
            FunctionsRuntimeVersion = _languageMetadata.FunctionsRuntimeVersion,
            ExtensionBundleVersion = _languageMetadata.ExtensionBundleVersion,
            Languages = languages
        };

        return result;
    }

    public async Task<ProjectTemplateResult> GetProjectTemplateAsync(
        string language,
        CancellationToken cancellationToken = default)
    {
        var normalizedLanguage = language.ToLowerInvariant();

        if (!_languageMetadata.IsValidLanguage(normalizedLanguage))
        {
            throw new ArgumentException(
                $"Invalid language: \"{language}\". Valid languages are: {string.Join(", ", _languageMetadata.SupportedLanguages)}.");
        }

        // Fetch manifest to get runtime versions
        var manifest = await _manifestService.FetchManifestAsync(cancellationToken);
        var languageInfo = _languageMetadata.GetLanguageInfo(normalizedLanguage, manifest.RuntimeVersions)!;

        var result = new ProjectTemplateResult
        {
            Language = normalizedLanguage,
            InitInstructions = languageInfo.InitInstructions,
            ProjectStructure = languageInfo.ProjectStructure
        };

        return result;
    }

    public async Task<TemplateListResult> GetTemplateListAsync(
        string language,
        CancellationToken cancellationToken = default)
    {
        var normalizedLanguage = language.ToLowerInvariant();

        if (!_languageMetadata.IsValidLanguage(normalizedLanguage))
        {
            throw new ArgumentException(
                $"Invalid language: \"{language}\". Valid languages are: {string.Join(", ", _languageMetadata.SupportedLanguages)}.");
        }

        var manifest = await _manifestService.FetchManifestAsync(cancellationToken);

        var matchingEntries = manifest.Templates
            .Where(t => t.Language.Equals(normalizedLanguage, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(t.FolderPath))
            .OrderBy(t => t.Priority)
            .ToList();

        static TemplateSummary ToSummary(TemplateManifestEntry entry) => new()
        {
            TemplateName = ExtractTemplateName(entry),
            DisplayName = entry.DisplayName,
            Description = entry.LongDescription,
            Resource = entry.Resource,
            Infrastructure = ParseInfrastructureType(entry.Iac)
        };

        static InfrastructureType ParseInfrastructureType(string? iac) => iac?.ToLowerInvariant() switch
        {
            "bicep" => InfrastructureType.Bicep,
            "terraform" => InfrastructureType.Terraform,
            "arm" => InfrastructureType.Arm,
            _ => InfrastructureType.None
        };

        return new TemplateListResult
        {
            Language = normalizedLanguage,
            Triggers = matchingEntries
                .Where(t => string.Equals(t.BindingType, "trigger", StringComparison.OrdinalIgnoreCase))
                .Select(ToSummary)
                .ToList(),
            InputBindings = matchingEntries
                .Where(t => string.Equals(t.BindingType, "input", StringComparison.OrdinalIgnoreCase))
                .Select(ToSummary)
                .ToList(),
            OutputBindings = matchingEntries
                .Where(t => string.Equals(t.BindingType, "output", StringComparison.OrdinalIgnoreCase))
                .Select(ToSummary)
                .ToList()
        };
    }

    public async Task<FunctionTemplateResult> GetFunctionTemplateAsync(
        string language,
        string template,
        string? runtimeVersion,
        CancellationToken cancellationToken = default)
    {
        var normalizedLanguage = language.ToLowerInvariant();

        if (!_languageMetadata.IsValidLanguage(normalizedLanguage))
        {
            throw new ArgumentException(
                $"Invalid language: \"{language}\". Valid languages are: {string.Join(", ", _languageMetadata.SupportedLanguages)}.");
        }

        var manifest = await _manifestService.FetchManifestAsync(cancellationToken);

        if (runtimeVersion is not null)
        {
            _languageMetadata.ValidateRuntimeVersion(normalizedLanguage, runtimeVersion, manifest.RuntimeVersions);
        }

        var entry = manifest.Templates
            .Where(t => t.Language.Equals(normalizedLanguage, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(t.FolderPath)
                && !string.IsNullOrWhiteSpace(t.RepositoryUrl)
                && ExtractTemplateName(t).Equals(template, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t.Priority)
            .FirstOrDefault();

        if (entry is null)
        {
            var availableNames = manifest.Templates
                .Where(t => t.Language.Equals(normalizedLanguage, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(t.FolderPath))
                .Select(t => ExtractTemplateName(t))
                .OrderBy(n => n)
                .ToList();

            throw new ArgumentException(
                $"Template \"{template}\" not found for language \"{normalizedLanguage}\". " +
                $"Available templates: {string.Join(", ", availableNames)}. " +
                "Use this tool without --template to list all templates with details.");
        }

        // Validate repository URL is from allowed GitHub org (SSRF prevention)
        if (!GitHubUrlValidator.IsValidRepositoryUrl(entry.RepositoryUrl))
        {
            throw new InvalidOperationException(
                $"Invalid repository URL in manifest. Only Azure and Azure-Samples organizations are allowed.");
        }

        var allFiles = await FetchTemplateFilesAsync(entry, normalizedLanguage, runtimeVersion, cancellationToken);

        var functionFiles = allFiles.Where(f => !_languageMetadata.KnownProjectFiles.Contains(GitHubUrlValidator.GetFileName(f.FileName))).ToList();
        var projectFiles = allFiles.Where(f => _languageMetadata.KnownProjectFiles.Contains(GitHubUrlValidator.GetFileName(f.FileName))).ToList();

        return new FunctionTemplateResult
        {
            Language = normalizedLanguage,
            TemplateName = ExtractTemplateName(entry),
            DisplayName = entry.DisplayName,
            Description = entry.LongDescription ?? entry.ShortDescription,
            BindingType = entry.BindingType,
            Resource = entry.Resource,
            FunctionFiles = functionFiles,
            ProjectFiles = projectFiles,
            MergeInstructions = FunctionTemplateMergeInstructions
        };
    }

    /// <summary>
    /// Builds a raw.githubusercontent.com URL from pre-validated repo path and file path.
    /// Raw URLs have higher rate limits (~5000/hour) compared to API (60/hour unauthenticated).
    /// </summary>
    internal static string BuildRawGitHubUrl(string repoPath, string filePath)
    {
        return $"https://raw.githubusercontent.com/{repoPath}/{DefaultBranch}/{filePath}";
    }

    /// <summary>
    /// Fetches all files from a template directory. Results are cached.
    /// </summary>
    private async Task<IReadOnlyList<ProjectTemplateFile>> FetchTemplateFilesAsync(
        TemplateManifestEntry template,
        string language,
        string? runtimeVersion,
        CancellationToken cancellationToken)
    {
        var normalizedPath = template.FolderPath.Trim().TrimStart('/');
        var isRootOrLarge = string.IsNullOrEmpty(normalizedPath) || normalizedPath == "." || normalizedPath == "..";
        var cacheKey = CacheKeyBuilder.Build(template.RepositoryUrl, normalizedPath);

        var cachedFiles = await _cacheService.GetAsync<List<ProjectTemplateFile>>(CacheGroup, cacheKey, FunctionsCacheDurations.TemplateCacheDuration, cancellationToken);
        IReadOnlyList<ProjectTemplateFile> files;

        if (cachedFiles is not null && cachedFiles.Count > 0)
        {
            _logger.LogDebug("Using cached template files for {Language}/{Path}", language, normalizedPath);
            files = cachedFiles;
        }
        else
        {
            _logger.LogDebug("Fetching template files from GitHub for {Language}/{Path}", language, normalizedPath);
            files = isRootOrLarge
                ? await FetchTemplateFilesViaArchiveAsync(template.RepositoryUrl, normalizedPath, cancellationToken)
                : await FetchTemplateFilesViaTreeApiAsync(template.RepositoryUrl, template.FolderPath, cancellationToken);

            if (files.Count > 0)
            {
                await _cacheService.SetAsync(CacheGroup, cacheKey, files.ToList(), FunctionsCacheDurations.TemplateCacheDuration, cancellationToken);
            }
        }

        var langInfo = _languageMetadata.GetLanguageInfo(language);
        var shouldReplace = runtimeVersion is not null && langInfo?.TemplateParameters is not null;

        if (!shouldReplace)
        {
            return files;
        }

        var result = new List<ProjectTemplateFile>(files.Count);
        foreach (var file in files)
        {
            var content = _languageMetadata.ReplaceRuntimeVersion(file.Content, language, runtimeVersion!);
            result.Add(new ProjectTemplateFile { FileName = file.FileName, Content = content });
        }

        return result;
    }

    /// <summary>
    /// Fetches files using GitHub's Tree API + raw URLs. Cached.
    /// </summary>
    private async Task<IReadOnlyList<ProjectTemplateFile>> FetchTemplateFilesViaTreeApiAsync(
        string repositoryUrl,
        string folderPath,
        CancellationToken cancellationToken)
    {
        var repoPath = GitHubUrlValidator.ExtractGitHubRepoPath(repositoryUrl)
            ?? throw new ArgumentException("Invalid repository URL format.", nameof(repositoryUrl));

        var normalizedFolder = GitHubUrlValidator.NormalizeFolderPath(folderPath)
            ?? throw new ArgumentException("Folder path must specify a valid subdirectory.", nameof(folderPath));

        var treeCacheKey = CacheKeyBuilder.Build("tree", repoPath, DefaultBranch);
        var cachedTree = await _cacheService.GetAsync<GitHubTreeResponse>(CacheGroup, treeCacheKey, FunctionsCacheDurations.TemplateCacheDuration, cancellationToken);

        GitHubTreeResponse treeResponse;
        if (cachedTree is not null)
        {
            _logger.LogDebug("Using cached tree for {Repo}", repoPath);
            treeResponse = cachedTree;
        }
        else
        {
            _logger.LogDebug("Fetching tree from GitHub for {Repo}", repoPath);
            var treeUrl = $"https://api.github.com/repos/{repoPath}/git/trees/{DefaultBranch}?recursive=1";
            treeResponse = await FetchTreeFromGitHubAsync(treeUrl, repoPath, cancellationToken);
            await _cacheService.SetAsync(CacheGroup, treeCacheKey, treeResponse, FunctionsCacheDurations.TemplateCacheDuration, cancellationToken);
        }

        var filePaths = FilterTreeToFolder(treeResponse, normalizedFolder);
        if (filePaths.Count == 0)
        {
            throw new InvalidOperationException(
                $"No template files found in folder '{normalizedFolder}'. The template may not exist or the repository structure has changed.");
        }

        var files = new List<ProjectTemplateFile>();
        using var client = _httpClientFactory.CreateClient();

        foreach (var (fullPath, relativePath, size) in filePaths)
        {
            if (size > MaxFileSizeBytes)
            {
                _logger.LogWarning("Skipping file {Name} ({Size} bytes) - exceeds max size", relativePath, size);
                continue;
            }

            var rawUrl = BuildRawGitHubUrl(repoPath, fullPath);

            try
            {
                using var response = await client.GetAsync(new Uri(rawUrl), cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch {Name} from raw URL (HTTP {Status})", relativePath, response.StatusCode);
                    continue;
                }

                string content;
                try
                {
                    content = await GitHubUrlValidator.ReadSizeLimitedStringAsync(response.Content, MaxFileSizeBytes, cancellationToken);
                }
                catch (InvalidOperationException)
                {
                    _logger.LogWarning("Skipping file {Name} - size exceeds limit", relativePath);
                    continue;
                }

                files.Add(new ProjectTemplateFile
                {
                    FileName = relativePath,
                    Content = content
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Error fetching template file {Name}", relativePath);
            }
        }

        return files;
    }

    /// <summary>
    /// Fetches tree response from GitHub API.
    /// </summary>
    private async Task<GitHubTreeResponse> FetchTreeFromGitHubAsync(string treeUrl, string repoPath, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient();

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(new Uri(treeUrl), cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch tree from {Url}", treeUrl);
            throw new InvalidOperationException(
                $"Failed to fetch template files from GitHub. Check your network connection. Details: {ex.Message}", ex);
        }

        using (response)
        {
            // Check for rate limiting - verify via X-RateLimit-Remaining header
            // HTTP 403 can also mean permission denied, so we check the header
            if (response.StatusCode == HttpStatusCode.TooManyRequests ||
                (response.StatusCode == HttpStatusCode.Forbidden && IsRateLimited(response)))
            {
                var resetHeader = response.Headers.TryGetValues("X-RateLimit-Reset", out var values)
                    ? values.FirstOrDefault() : null;
                var resetInfo = resetHeader != null && long.TryParse(resetHeader, out var resetTime)
                    ? $" Rate limit resets at {DateTimeOffset.FromUnixTimeSeconds(resetTime):HH:mm:ss UTC}."
                    : "";

                throw new InvalidOperationException(
                    $"GitHub API rate limit exceeded.{resetInfo} Try again later.");
            }

            // Handle other 403 errors (permissions, not found for private repos, etc.)
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "GitHub API access forbidden. The repository may be private or you may lack permissions.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"GitHub API returned {(int)response.StatusCode} {response.ReasonPhrase}. Unable to fetch template files.");
            }

            // Use size-limited read to prevent OOM attacks
            var json = await GitHubUrlValidator.ReadSizeLimitedStringAsync(response.Content, MaxTreeSizeBytes, cancellationToken);
            GitHubTreeResponse? tree;

            try
            {
                tree = JsonSerializer.Deserialize(json, FunctionsJsonContext.Default.GitHubTreeResponse);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse tree response from {Url}", treeUrl);
                throw new InvalidOperationException(
                    $"Failed to parse GitHub response. Details: {ex.Message}", ex);
            }

            if (tree?.Tree is null || tree.Tree.Count == 0)
            {
                _logger.LogWarning("Empty tree response from {Repo}", repoPath);
                throw new InvalidOperationException(
                    $"GitHub returned empty file tree for '{repoPath}'. The repository may be empty or inaccessible.");
            }

            // Check for truncated response - GitHub may not return all files for large repos
            if (tree.Truncated)
            {
                _logger.LogWarning("GitHub tree response was truncated for {Repo}. Some files may be missing.", repoPath);
            }

            return tree;
        }
    }

    /// <summary>
    /// Checks if a 403 response indicates GitHub rate limiting by examining the X-RateLimit-Remaining header.
    /// GitHub returns 403 with X-RateLimit-Remaining: 0 when rate limited (unlike standard 429).
    /// </summary>
    private static bool IsRateLimited(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var values))
        {
            var remaining = values.FirstOrDefault();
            if (remaining != null && int.TryParse(remaining, out var count) && count == 0)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Filters tree items to files within the specified folder.
    /// </summary>
    private static IReadOnlyList<(string FullPath, string RelativePath, long Size)> FilterTreeToFolder(
        GitHubTreeResponse treeResponse,
        string folderPrefix)
    {
        var folderPrefixWithSlash = folderPrefix.TrimEnd('/') + "/";
        var results = new List<(string, string, long)>();

        foreach (var item in treeResponse.Tree)
        {
            if (item.Type != "blob" || item.Path is null)
            {
                continue;
            }

            if (!item.Path.StartsWith(folderPrefixWithSlash, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var relativePath = item.Path[folderPrefixWithSlash.Length..];
            results.Add((item.Path, relativePath, item.Size));
        }

        return results;
    }

    /// <summary>
    /// Fetches files by downloading the repository zipball.
    /// </summary>
    internal async Task<IReadOnlyList<ProjectTemplateFile>> FetchTemplateFilesViaArchiveAsync(
        string repositoryUrl,
        string folderPath,
        CancellationToken cancellationToken)
    {
        var repoPath = GitHubUrlValidator.ExtractGitHubRepoPath(repositoryUrl)
            ?? throw new ArgumentException("Invalid repository URL format.", nameof(repositoryUrl));

        var zipUrl = $"https://api.github.com/repos/{repoPath}/zipball/{DefaultBranch}";
        var normalizedFolder = GitHubUrlValidator.NormalizeFolderPath(folderPath, allowRoot: true) ?? string.Empty;

        using var client = _httpClientFactory.CreateClient();

        _logger.LogInformation("Downloading repository archive from {Url}", zipUrl);

        using var response = await client.GetAsync(new Uri(zipUrl), HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var files = new List<ProjectTemplateFile>();

        await using var zipStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        string? rootPrefix = null;

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
            {
                continue;
            }

            rootPrefix ??= GetZipRootPrefix(entry.FullName);

            var relativePath = entry.FullName;
            if (rootPrefix is not null && relativePath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath[rootPrefix.Length..];
            }

            if (!string.IsNullOrEmpty(normalizedFolder) && normalizedFolder != "." && normalizedFolder != "..")
            {
                if (!relativePath.StartsWith(normalizedFolder + "/", StringComparison.OrdinalIgnoreCase) &&
                    !relativePath.Equals(normalizedFolder, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (relativePath.StartsWith(normalizedFolder + "/", StringComparison.OrdinalIgnoreCase))
                {
                    relativePath = relativePath[(normalizedFolder.Length + 1)..];
                }
            }

            if (relativePath.Contains("..", StringComparison.Ordinal))
            {
                _logger.LogWarning("Skipping file {Name} - path traversal detected", entry.FullName);
                continue;
            }

            if (entry.Length > MaxFileSizeBytes)
            {
                _logger.LogWarning("Skipping file {Name} - exceeds max size", relativePath);
                continue;
            }

            try
            {
                using var stream = entry.Open();
                var bufferSize = (int)MaxFileSizeBytes + 1;
                var buffer = new char[bufferSize];
                using var reader = new StreamReader(stream);
                var charsRead = await reader.ReadBlockAsync(buffer.AsMemory(0, bufferSize), cancellationToken);

                if (charsRead > MaxFileSizeBytes)
                {
                    _logger.LogWarning("Skipping file {Name} - exceeds max size", relativePath);
                    continue;
                }

                var content = new string(buffer, 0, charsRead);

                files.Add(new ProjectTemplateFile
                {
                    FileName = relativePath,
                    Content = content
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading file {Name} from archive", entry.FullName);
            }
        }

        _logger.LogInformation("Extracted {Count} files from archive", files.Count);
        return files;
    }

    /// <summary>
    /// Extracts the root prefix from a GitHub zipball entry path.
    /// GitHub creates entries like "owner-repo-sha/path/to/file".
    /// </summary>
    internal static string? GetZipRootPrefix(string entryPath)
    {
        var firstSlash = entryPath.IndexOf('/');
        return firstSlash > 0 ? entryPath[..(firstSlash + 1)] : null;
    }

    /// <summary>
    /// Gets the template name from a manifest entry.
    /// Always uses entry.Id for consistency - folderPath is only used for download logic.
    /// </summary>
    internal static string ExtractTemplateName(TemplateManifestEntry entry) => entry.Id ?? string.Empty;
}
