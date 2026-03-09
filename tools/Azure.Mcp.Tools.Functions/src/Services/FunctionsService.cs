// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Services.Helpers;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Functions.Services;

/// <summary>
/// Service for Azure Functions template operations.
/// Fetches template data from the CDN manifest and GitHub repository.
/// Language metadata (Tool 1) uses small static data.
/// Project templates (Tool 2+) fetch live data from CDN + GitHub.
/// </summary>
public sealed class FunctionsService(
    IHttpClientFactory httpClientFactory,
    ILanguageMetadataProvider languageMetadata,
    IManifestService manifestService,
    ILogger<FunctionsService> logger) : IFunctionsService
{
    private readonly ILanguageMetadataProvider _languageMetadata = languageMetadata ?? throw new ArgumentNullException(nameof(languageMetadata));
    private readonly IManifestService _manifestService = manifestService ?? throw new ArgumentNullException(nameof(manifestService));

    private const string DefaultBranch = "main";
    private const long MaxFileSizeBytes = 1_048_576; // 1 MB

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

    public Task<LanguageListResult> GetLanguageListAsync(CancellationToken cancellationToken = default)
    {
        var languages = new List<LanguageDetails>();

        foreach (var kvp in _languageMetadata.GetAllLanguages())
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

        return Task.FromResult(result);
    }

    public Task<ProjectTemplateResult> GetProjectTemplateAsync(
        string language,
        CancellationToken cancellationToken = default)
    {
        var normalizedLanguage = language.ToLowerInvariant();

        if (!_languageMetadata.IsValidLanguage(normalizedLanguage))
        {
            throw new ArgumentException(
                $"Invalid language: \"{language}\". Valid languages are: {string.Join(", ", _languageMetadata.SupportedLanguages)}.");
        }

        // Return static metadata only - no HTTP calls needed
        // Agents can create the actual files based on this information
        var languageInfo = _languageMetadata.GetLanguageInfo(normalizedLanguage)!;

        var result = new ProjectTemplateResult
        {
            Language = normalizedLanguage,
            InitInstructions = languageInfo.InitInstructions,
            ProjectStructure = languageInfo.ProjectStructure
        };

        return Task.FromResult(result);
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

        if (runtimeVersion is not null)
        {
            _languageMetadata.ValidateRuntimeVersion(normalizedLanguage, runtimeVersion);
        }

        var manifest = await _manifestService.FetchManifestAsync(cancellationToken);

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
    /// Converts a GitHub repository URL and folder path into a raw.githubusercontent.com URL.
    /// </summary>
    internal static string ConvertToRawGitHubUrl(string repositoryUrl, string folderPath)
    {
        // repositoryUrl: "https://github.com/Azure/azure-functions-templates-mcp-server"
        // folderPath: "templates/python/BlobTriggerWithEventGrid"
        // result: "https://raw.githubusercontent.com/Azure/azure-functions-templates-mcp-server/main/templates/python/..."

        var repoPath = GitHubUrlValidator.ExtractGitHubRepoPath(repositoryUrl)
            ?? throw new ArgumentException("Invalid repository URL format.", nameof(repositoryUrl));

        var normalizedPath = GitHubUrlValidator.NormalizeFolderPath(folderPath)
            ?? throw new ArgumentException("Folder path must specify a valid subdirectory, not the repository root.", nameof(folderPath));

        return $"https://raw.githubusercontent.com/{repoPath}/{DefaultBranch}/{normalizedPath}";
    }

    /// <summary>
    /// Fetches all files from a template directory. Uses GitHub's zipball API for root/large folders,
    /// or the Contents API for specific subdirectories.
    /// </summary>
    internal async Task<IReadOnlyList<ProjectTemplateFile>> FetchTemplateFilesAsync(
        TemplateManifestEntry template,
        string language,
        string? runtimeVersion,
        CancellationToken cancellationToken)
    {
        var normalizedPath = template.FolderPath.Trim().TrimStart('/');
        var isRootOrLarge = string.IsNullOrEmpty(normalizedPath) || normalizedPath == "." || normalizedPath == "..";

        var langInfo = _languageMetadata.GetLanguageInfo(language);
        var hasTemplateParams = langInfo?.TemplateParameters is not null;
        var shouldReplace = runtimeVersion is not null && hasTemplateParams;

        IReadOnlyList<ProjectTemplateFile> files = isRootOrLarge
            ? await FetchTemplateFilesViaArchiveAsync(template.RepositoryUrl, normalizedPath, cancellationToken)
            : await FetchTemplateFilesViaContentsApiAsync(template.RepositoryUrl, template.FolderPath, cancellationToken);

        if (!shouldReplace)
        {
            return files;
        }

        // Apply runtime version replacements
        var result = new List<ProjectTemplateFile>(files.Count);
        foreach (var file in files)
        {
            var content = _languageMetadata.ReplaceRuntimeVersion(file.Content, language, runtimeVersion!);
            result.Add(new ProjectTemplateFile { FileName = file.FileName, Content = content });
        }

        return result;
    }

    /// <summary>
    /// Fetches files using the Contents API (efficient for small template folders).
    /// </summary>
    private async Task<IReadOnlyList<ProjectTemplateFile>> FetchTemplateFilesViaContentsApiAsync(
        string repositoryUrl,
        string folderPath,
        CancellationToken cancellationToken)
    {
        var contentsUrl = ConstructGitHubContentsApiUrl(repositoryUrl, folderPath);
        var fileEntries = await ListGitHubDirectoryAsync(contentsUrl, cancellationToken);

        var files = new List<ProjectTemplateFile>();
        var folderPrefix = folderPath.TrimEnd('/') + "/";

        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Azure-MCP-Server/1.0");

        foreach (var entry in fileEntries)
        {
            if (entry.Size > MaxFileSizeBytes)
            {
                logger.LogWarning("Skipping file {Name} ({Size} bytes) - exceeds max size", entry.Name, entry.Size);
                continue;
            }

            // Validate URL points to GitHub domain (SSRF prevention)
            if (entry.DownloadUrl is null || !GitHubUrlValidator.IsValidGitHubUrl(entry.DownloadUrl))
            {
                logger.LogWarning("Skipping file {Name} - invalid or missing download URL", entry.Name);
                continue;
            }

            try
            {
                using var response = await client.GetAsync(new Uri(entry.DownloadUrl), cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "Failed to fetch {Name} from {Url} (HTTP {Status})",
                        entry.Name, entry.DownloadUrl, response.StatusCode);
                    continue;
                }

                // Use size-limited reading to prevent DoS (protects against missing Content-Length header)
                string content;
                try
                {
                    content = await GitHubUrlValidator.ReadSizeLimitedStringAsync(response.Content, MaxFileSizeBytes, cancellationToken);
                }
                catch (InvalidOperationException)
                {
                    logger.LogWarning("Skipping file {Name} - size exceeds limit", entry.Name);
                    continue;
                }

                // Use relative path from the template folder root
                var relativePath = entry.Path;
                if (relativePath.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    relativePath = relativePath[folderPrefix.Length..];
                }

                files.Add(new ProjectTemplateFile
                {
                    FileName = relativePath,
                    Content = content
                });
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "Error fetching template file {Name}", entry.Name);
            }
        }

        return files;
    }

    /// <summary>
    /// Fetches files by downloading the repository as a zipball (efficient for root or large folders).
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

        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Azure-MCP-Server/1.0");

        logger.LogInformation("Downloading repository archive from {Url}", zipUrl);

        using var response = await client.GetAsync(new Uri(zipUrl), HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var files = new List<ProjectTemplateFile>();

        await using var zipStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        // GitHub zipball has a root folder like "owner-repo-commitsha/"
        // We need to strip this prefix and optionally filter by folderPath
        string? rootPrefix = null;

        foreach (var entry in archive.Entries)
        {
            // Skip directories (entries ending with /)
            if (string.IsNullOrEmpty(entry.Name))
            {
                continue;
            }

            // Detect root prefix from first file
            rootPrefix ??= GetZipRootPrefix(entry.FullName);

            // Get path relative to the root prefix
            var relativePath = entry.FullName;
            if (rootPrefix is not null && relativePath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath[rootPrefix.Length..];
            }

            // Filter by folder path if specified
            if (!string.IsNullOrEmpty(normalizedFolder) && normalizedFolder != "." && normalizedFolder != "..")
            {
                if (!relativePath.StartsWith(normalizedFolder + "/", StringComparison.OrdinalIgnoreCase) &&
                    !relativePath.Equals(normalizedFolder, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Remove the folder prefix from the relative path
                if (relativePath.StartsWith(normalizedFolder + "/", StringComparison.OrdinalIgnoreCase))
                {
                    relativePath = relativePath[(normalizedFolder.Length + 1)..];
                }
            }

            // Security: Reject paths with traversal sequences to prevent Zip Slip attacks
            if (relativePath.Contains("..", StringComparison.Ordinal))
            {
                logger.LogWarning("Skipping file {Name} - contains path traversal sequence", entry.FullName);
                continue;
            }

            // Skip files exceeding max size (note: entry.Length is uncompressed size)
            if (entry.Length > MaxFileSizeBytes)
            {
                logger.LogWarning("Skipping file {Name} ({Size} bytes uncompressed) - exceeds max size", relativePath, entry.Length);
                continue;
            }

            try
            {
                using var stream = entry.Open();

                // Read with size limit to prevent ZIP bomb attacks
                // Use int for buffer size (MaxFileSizeBytes is well under int.MaxValue)
                var bufferSize = (int)MaxFileSizeBytes + 1;
                var buffer = new char[bufferSize];
                using var reader = new StreamReader(stream);
                var charsRead = await reader.ReadBlockAsync(buffer.AsMemory(0, bufferSize), cancellationToken);

                if (charsRead > MaxFileSizeBytes)
                {
                    logger.LogWarning("Skipping file {Name} - uncompressed content exceeds max size", relativePath);
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
                logger.LogWarning(ex, "Error reading file {Name} from archive", entry.FullName);
            }
        }

        logger.LogInformation("Extracted {Count} files from archive", files.Count);
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
    /// Lists all files in a GitHub directory recursively using the Contents API.
    /// </summary>
    internal async Task<IReadOnlyList<GitHubContentEntry>> ListGitHubDirectoryAsync(
        string contentsUrl,
        CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Azure-MCP-Server/1.0");

        try
        {
            return await ListGitHubDirectoryRecursiveAsync(client, contentsUrl, depth: 0, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to list GitHub directory at {Url}", contentsUrl);
            throw new InvalidOperationException(
                $"Could not list template files from GitHub. Details: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse GitHub API response from {Url}", contentsUrl);
            throw new InvalidOperationException(
                $"Could not parse GitHub directory listing. Details: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Constructs a GitHub Contents API URL from a repository URL and folder path.
    /// </summary>
    internal static string ConstructGitHubContentsApiUrl(string repositoryUrl, string folderPath)
    {
        var repoPath = GitHubUrlValidator.ExtractGitHubRepoPath(repositoryUrl)
            ?? throw new ArgumentException("Invalid repository URL format.", nameof(repositoryUrl));

        var normalizedPath = GitHubUrlValidator.NormalizeFolderPath(folderPath)
            ?? throw new ArgumentException("Folder path must specify a valid subdirectory, not the repository root.", nameof(folderPath));

        return $"https://api.github.com/repos/{repoPath}/contents/{normalizedPath}";
    }

    /// <summary>
    /// Gets the template name from a manifest entry.
    /// Always uses entry.Id for consistency - folderPath is only used for download logic.
    /// </summary>
    internal static string ExtractTemplateName(TemplateManifestEntry entry) => entry.Id ?? string.Empty;

    private const int MaxRecursionDepth = 10;

    private async Task<IReadOnlyList<GitHubContentEntry>> ListGitHubDirectoryRecursiveAsync(
        HttpClient client,
        string contentsUrl,
        int depth,
        CancellationToken cancellationToken)
    {
        if (depth > MaxRecursionDepth)
        {
            logger.LogWarning("Max recursion depth {MaxDepth} exceeded for URL {Url}", MaxRecursionDepth, contentsUrl);
            return []; // Prevent infinite recursion from circular links or deep nesting
        }

        var allFiles = new List<GitHubContentEntry>();
        const long maxDirectoryListingSize = 1 * 1024 * 1024; // 1MB limit for directory listings

        try
        {
            using var response = await client.GetAsync(new Uri(contentsUrl), cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await GitHubUrlValidator.ReadSizeLimitedStringAsync(response.Content, maxDirectoryListingSize, cancellationToken);
            var entries = JsonSerializer.Deserialize(json, FunctionTemplatesManifestJsonContext.Default.ListGitHubContentEntry)
                ?? [];

            foreach (var entry in entries)
            {
                if (entry.Type == "file")
                {
                    allFiles.Add(entry);
                }
                else if (entry.Type == "dir" && entry.Url is not null && GitHubUrlValidator.IsValidGitHubUrl(entry.Url))
                {
                    var subFiles = await ListGitHubDirectoryRecursiveAsync(client, entry.Url, depth + 1, cancellationToken);
                    allFiles.AddRange(subFiles);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "HTTP error listing GitHub directory at {Url}", contentsUrl);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "JSON parsing error for GitHub directory response from {Url}", contentsUrl);
        }

        return allFiles;
    }
}

/// <summary>
/// AOT-safe JSON serialization context for CDN manifest and GitHub API deserialization.
/// </summary>
[JsonSerializable(typeof(TemplateManifest))]
[JsonSerializable(typeof(TemplateManifestEntry))]
[JsonSerializable(typeof(List<GitHubContentEntry>))]
[JsonSerializable(typeof(GitHubContentEntry))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class FunctionTemplatesManifestJsonContext : JsonSerializerContext;
