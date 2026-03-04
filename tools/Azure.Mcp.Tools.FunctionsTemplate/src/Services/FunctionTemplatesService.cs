// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.FunctionsTemplate.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.FunctionsTemplate.Services;

/// <summary>
/// Service for Azure Functions template operations.
/// Fetches template data from the CDN manifest and GitHub repository.
/// Language metadata (Tool 1) uses small static data.
/// Project templates (Tool 2+) fetch live data from CDN + GitHub.
/// </summary>
public sealed class FunctionTemplatesService(
    IHttpClientFactory httpClientFactory,
    ICacheService cacheService,
    ILogger<FunctionTemplatesService> logger) : IFunctionTemplatesService
{
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    private const string ManifestUrl = "https://cdn-test.functions.azure.com/public/templates/manifest.json";
    private const string FunctionsRuntimeVersion = "4.x";
    private const string ExtensionBundleVersion = "[4.*, 5.0.0)";
    private const string DefaultBranch = "main";
    private const long MaxFileSizeBytes = 1_048_576; // 1 MB
    private const string CacheGroup = "functiontemplates";
    private const string ManifestCacheKey = "manifest";
    private static readonly TimeSpan s_manifestCacheDuration = TimeSpan.FromHours(1);

    private const string FunctionTemplateMergeInstructions =
        """
        ## Merging Template Files with Existing Project

        **Project files** (host.json, local.settings.json, etc.) may already exist if you used `functiontemplates project get`.
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

    private static readonly HashSet<string> s_validLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "python", "typescript", "java", "csharp"
    };

    /// <summary>
    /// Known project-level filenames per language. These are the files that initialize
    /// a new Azure Functions project (not function-specific code).
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> s_projectFileNames =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["python"] = ["host.json", "local.settings.json", "requirements.txt", ".funcignore"],
            ["typescript"] = ["host.json", "local.settings.json", "package.json", ".funcignore"],
            ["java"] = ["host.json", "local.settings.json", ".funcignore"],
            ["csharp"] = ["local.settings.json", ".funcignore"]
        };

    /// <summary>
    /// Flat set of known project-level filenames used to separate project files
    /// from function-specific files in template get mode.
    /// </summary>
    private static readonly HashSet<string> s_knownProjectFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "host.json", "local.settings.json", "requirements.txt", "package.json",
        "tsconfig.json", ".funcignore", ".gitignore", "pom.xml"
    };

    private static readonly IReadOnlyDictionary<string, LanguageInfo> s_languageInfo =
        new Dictionary<string, LanguageInfo>
        {
            ["python"] = new LanguageInfo
            {
                Name = "Python",
                Runtime = "python",
                ProgrammingModel = "v2 (Decorator-based)",
                Prerequisites = ["Python 3.10+", "Azure Functions Core Tools v4"],
                DevelopmentTools = ["VS Code with Azure Functions extension", "Azure Functions Core Tools"],
                InitCommand = "func init --worker-runtime python --model V2",
                RunCommand = "func start",
                BuildCommand = null
            },
            ["typescript"] = new LanguageInfo
            {
                Name = "TypeScript",
                Runtime = "node",
                ProgrammingModel = "v4 (Schema-based)",
                Prerequisites = ["Node.js 20+", "Azure Functions Core Tools v4", "TypeScript 4.x+"],
                DevelopmentTools = ["VS Code with Azure Functions extension", "Azure Functions Core Tools"],
                InitCommand = "func init --worker-runtime node --language typescript --model V4",
                RunCommand = "npm start",
                BuildCommand = "npm run build"
            },
            ["java"] = new LanguageInfo
            {
                Name = "Java",
                Runtime = "java",
                ProgrammingModel = "Annotations-based",
                Prerequisites = ["JDK 8, 11, 17, or 21", "Apache Maven 3.x", "Azure Functions Core Tools v4"],
                DevelopmentTools = ["VS Code with Java + Azure Functions extensions", "IntelliJ IDEA", "Azure Functions Core Tools"],
                InitCommand = "func init --worker-runtime java",
                RunCommand = "mvn clean package && mvn azure-functions:run",
                BuildCommand = "mvn clean package"
            },
            ["csharp"] = new LanguageInfo
            {
                Name = "C#",
                Runtime = "dotnet",
                ProgrammingModel = "Isolated worker process",
                Prerequisites = [".NET 8 SDK or later", "Azure Functions Core Tools v4"],
                DevelopmentTools = ["Visual Studio 2022", "VS Code with C# + Azure Functions extensions", "Azure Functions Core Tools"],
                InitCommand = "func init --worker-runtime dotnet-isolated",
                RunCommand = "func start",
                BuildCommand = "dotnet build"
            }
        };

    private static readonly IReadOnlyDictionary<string, RuntimeVersionInfo> s_supportedRuntimes =
        new Dictionary<string, RuntimeVersionInfo>
        {
            ["python"] = new RuntimeVersionInfo
            {
                Supported = ["3.10", "3.11", "3.12", "3.13"],
                Preview = ["3.14"],
                Default = "3.11"
            },
            ["typescript"] = new RuntimeVersionInfo
            {
                Supported = ["20", "22"],
                Preview = ["24"],
                Default = "20"
            },
            ["java"] = new RuntimeVersionInfo
            {
                Supported = ["8", "11", "17", "21"],
                Preview = ["25"],
                Default = "21"
            },
            ["csharp"] = new RuntimeVersionInfo
            {
                Supported = ["8", "9", "10"],
                Deprecated = ["6", "7"],
                Default = "8",
                FrameworkSupported = ["4.8.1"]
            }
        };

    /// <summary>
    /// Static init instructions per language. These are small guidance strings,
    /// not template file content.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> s_initInstructions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["python"] = """
                ## Python Azure Functions Project Setup

                1. Create a virtual environment:
                   ```bash
                   python -m venv .venv
                   source .venv/bin/activate  # On Windows: .venv\Scripts\activate
                   ```

                2. Install dependencies:
                   ```bash
                   pip install -r requirements.txt
                   ```

                3. Create your first function in `function_app.py`

                4. Run locally:
                   ```bash
                   func start
                   ```
                """.Replace("                ", ""),
            ["typescript"] = """
                ## TypeScript Azure Functions Project Setup

                1. Install dependencies:
                   ```bash
                   npm install
                   ```

                2. Create your functions in `src/functions/` directory

                3. Build and run locally:
                   ```bash
                   npm start
                   ```

                4. For development with auto-rebuild:
                   ```bash
                   npm run watch
                   # In another terminal: func start
                   ```
                """.Replace("                ", ""),
            ["java"] = """
                ## Java Azure Functions Project Setup

                **Note**: pom.xml content is available in `get_azure_functions_template`. Copy/Merge the pom.xml from the function template you choose.

                1. Build the project:
                   ```bash
                   mvn clean package
                   ```

                2. Create your functions in `src/main/java/com/function/` directory

                3. Run locally:
                   ```bash
                   mvn azure-functions:run
                   ```
                """.Replace("                ", ""),
            ["csharp"] = """
                ## C# Azure Functions Project Setup

                1. Create project using .NET CLI:
                   ```bash
                   func init --dotnet-isolated
                   ```

                2. Or use Visual Studio / VS Code with Azure Functions extension

                3. Build and run:
                   ```bash
                   dotnet build
                   func start
                   ```

                **Note**: C# projects are typically initialized using `func init` or Visual Studio
                templates which create the .csproj file with proper dependencies.
                Use `func new` to add functions after project initialization.
                """.Replace("                ", "")
        };

    /// <summary>
    /// Static project structure descriptions per language.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> s_projectStructure =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["python"] =
            [
                "function_app.py    # Main application file with all functions",
                "host.json          # Azure Functions host configuration",
                "local.settings.json # Local development settings (do not commit)",
                "requirements.txt   # Python dependencies",
                ".funcignore        # Files to exclude from deployment"
            ],
            ["typescript"] =
            [
                "src/functions/     # Function implementation files",
                "host.json          # Azure Functions host configuration",
                "local.settings.json # Local development settings (do not commit)",
                "package.json       # Node.js dependencies and scripts",
                ".funcignore        # Files to exclude from deployment"
            ],
            ["java"] =
            [
                "src/main/java/     # Java source files",
                "pom.xml            # Maven project configuration (from template)",
                "host.json          # Azure Functions host configuration",
                "local.settings.json # Local development settings (do not commit)",
                ".funcignore        # Files to exclude from deployment"
            ],
            ["csharp"] =
            [
                "*.csproj            # C# project file",
                "Program.cs          # Application entry point",
                "host.json           # Azure Functions host configuration",
                "local.settings.json # Local development settings (do not commit)",
                ".funcignore         # Files to exclude from deployment"
            ]
        };

    /// <summary>
    /// Template parameters for languages that have customizable placeholders.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<TemplateParameter>> s_templateParameters =
        new Dictionary<string, IReadOnlyList<TemplateParameter>>(StringComparer.OrdinalIgnoreCase)
        {
            ["typescript"] =
            [
                new TemplateParameter
                {
                    Name = "nodeVersion",
                    Description = "Node.js version for @types/node. Detect from user environment or ask preference.",
                    DefaultValue = "20",
                    ValidValues = ["20", "22", "24"]
                }
            ],
            ["java"] =
            [
                new TemplateParameter
                {
                    Name = "javaVersion",
                    Description = "Java version for compilation and runtime. Detect from user environment or ask preference.",
                    DefaultValue = "21",
                    ValidValues = ["8", "11", "17", "21", "25"]
                }
            ]
        };

    public Task<LanguageListResult> GetLanguageListAsync(CancellationToken cancellationToken = default)
    {
        var languages = new List<LanguageDetails>();

        foreach (var kvp in s_languageInfo)
        {
            var key = kvp.Key;
            if (s_supportedRuntimes.TryGetValue(key, out var runtimeVersions))
            {
                languages.Add(new LanguageDetails
                {
                    Language = key,
                    Info = kvp.Value,
                    RuntimeVersions = runtimeVersions
                });
            }
        }

        var result = new LanguageListResult
        {
            FunctionsRuntimeVersion = FunctionsRuntimeVersion,
            ExtensionBundleVersion = ExtensionBundleVersion,
            Languages = languages
        };

        return Task.FromResult(result);
    }

    public async Task<ProjectTemplateResult> GetProjectTemplateAsync(
        string language,
        string? runtimeVersion,
        CancellationToken cancellationToken = default)
    {
        var normalizedLanguage = language.ToLowerInvariant();

        if (!s_validLanguages.Contains(normalizedLanguage))
        {
            throw new ArgumentException(
                $"Invalid language: \"{language}\". Valid languages are: {string.Join(", ", s_validLanguages)}.");
        }

        if (runtimeVersion is not null)
        {
            ValidateRuntimeVersion(normalizedLanguage, runtimeVersion);
        }

        // Fetch manifest and find a candidate template for this language
        var manifest = await FetchManifestAsync(cancellationToken);
        var candidateTemplate = SelectCandidateTemplate(manifest, normalizedLanguage);

        if (candidateTemplate is null)
        {
            throw new InvalidOperationException(
                $"No template found for language \"{normalizedLanguage}\" in the CDN manifest.");
        }

        // Fetch project-level files from the candidate template's GitHub repository
        var files = await FetchProjectFilesAsync(
            candidateTemplate, normalizedLanguage, runtimeVersion, cancellationToken);

        var shouldReplace = runtimeVersion is not null &&
            (normalizedLanguage is "java" or "typescript");

        s_initInstructions.TryGetValue(normalizedLanguage, out var initInstructions);
        s_projectStructure.TryGetValue(normalizedLanguage, out var projectStructure);
        s_templateParameters.TryGetValue(normalizedLanguage, out var parameters);

        return new ProjectTemplateResult
        {
            Language = normalizedLanguage,
            Files = files,
            InitInstructions = initInstructions ?? string.Empty,
            ProjectStructure = projectStructure ?? [],
            Parameters = shouldReplace ? null : parameters
        };
    }

    public async Task<TemplateListResult> GetTemplateListAsync(
        string language,
        CancellationToken cancellationToken = default)
    {
        var normalizedLanguage = language.ToLowerInvariant();

        if (!s_validLanguages.Contains(normalizedLanguage))
        {
            throw new ArgumentException(
                $"Invalid language: \"{language}\". Valid languages are: {string.Join(", ", s_validLanguages)}.");
        }

        var manifest = await FetchManifestAsync(cancellationToken);

        var matchingEntries = manifest.Templates
            .Where(t => t.Language.Equals(normalizedLanguage, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(t.FolderPath))
            .OrderBy(t => t.Priority)
            .ToList();

        static TemplateSummary ToSummary(TemplateManifestEntry entry) => new()
        {
            TemplateName = ExtractTemplateName(entry.FolderPath),
            DisplayName = entry.DisplayName,
            ShortDescription = entry.ShortDescription,
            Resource = entry.Resource
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

        if (!s_validLanguages.Contains(normalizedLanguage))
        {
            throw new ArgumentException(
                $"Invalid language: \"{language}\". Valid languages are: {string.Join(", ", s_validLanguages)}.");
        }

        if (runtimeVersion is not null)
        {
            ValidateRuntimeVersion(normalizedLanguage, runtimeVersion);
        }

        var manifest = await FetchManifestAsync(cancellationToken);

        var entry = manifest.Templates
            .Where(t => t.Language.Equals(normalizedLanguage, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(t.FolderPath)
                && !string.IsNullOrWhiteSpace(t.RepositoryUrl)
                && ExtractTemplateName(t.FolderPath).Equals(template, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t.Priority)
            .FirstOrDefault();

        if (entry is null)
        {
            var availableNames = manifest.Templates
                .Where(t => t.Language.Equals(normalizedLanguage, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(t.FolderPath))
                .Select(t => ExtractTemplateName(t.FolderPath))
                .OrderBy(n => n)
                .ToList();

            throw new ArgumentException(
                $"Template \"{template}\" not found for language \"{normalizedLanguage}\". " +
                $"Available templates: {string.Join(", ", availableNames)}. " +
                "Use this tool without --template to list all templates with details.");
        }

        var allFiles = await FetchTemplateFilesAsync(entry, normalizedLanguage, runtimeVersion, cancellationToken);

        var functionFiles = allFiles.Where(f => !s_knownProjectFiles.Contains(GetFileName(f.FileName))).ToList();
        var projectFiles = allFiles.Where(f => s_knownProjectFiles.Contains(GetFileName(f.FileName))).ToList();

        return new FunctionTemplateResult
        {
            Language = normalizedLanguage,
            TemplateName = ExtractTemplateName(entry.FolderPath),
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
    /// Fetches and caches the CDN template manifest with TTL-based expiration.
    /// </summary>
    internal async Task<TemplateManifest> FetchManifestAsync(CancellationToken cancellationToken)
    {
        var cached = await _cacheService.GetAsync<TemplateManifest>(CacheGroup, ManifestCacheKey, s_manifestCacheDuration, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        try
        {
            using var client = httpClientFactory.CreateClient();
            using var response = await client.GetAsync(new Uri(ManifestUrl), cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var manifest = JsonSerializer.Deserialize(json, FunctionTemplatesManifestJsonContext.Default.TemplateManifest)
                ?? throw new InvalidOperationException("Failed to deserialize the CDN manifest.");

            await _cacheService.SetAsync(CacheGroup, ManifestCacheKey, manifest, s_manifestCacheDuration, cancellationToken);
            return manifest;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch CDN manifest from {Url}", ManifestUrl);
            throw new InvalidOperationException(
                $"Could not fetch the Azure Functions templates manifest. Details: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Selects a good candidate template from the manifest for the given language.
    /// Prefers templates sorted by priority (lower = better).
    /// Validates the template entry has the required fields.
    /// </summary>
    internal static TemplateManifestEntry? SelectCandidateTemplate(TemplateManifest manifest, string language)
    {
        return manifest.Templates
            .Where(t => t.Language.Equals(language, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(t.RepositoryUrl)
                && !string.IsNullOrWhiteSpace(t.FolderPath))
            .OrderBy(t => t.Priority)
            .FirstOrDefault();
    }

    /// <summary>
    /// Fetches project-level files from the GitHub repository using raw URLs
    /// constructed from the template's repositoryUrl and folderPath.
    /// </summary>
    internal async Task<IReadOnlyList<ProjectTemplateFile>> FetchProjectFilesAsync(
        TemplateManifestEntry template,
        string language,
        string? runtimeVersion,
        CancellationToken cancellationToken)
    {
        if (!s_projectFileNames.TryGetValue(language, out var fileNames))
        {
            return [];
        }

        var rawBaseUrl = ConvertToRawGitHubUrl(template.RepositoryUrl, template.FolderPath);
        var shouldReplace = runtimeVersion is not null && (language is "java" or "typescript");
        var files = new List<ProjectTemplateFile>();

        using var client = httpClientFactory.CreateClient();

        foreach (var fileName in fileNames)
        {
            var fileUrl = $"{rawBaseUrl}/{fileName}";

            try
            {
                using var response = await client.GetAsync(new Uri(fileUrl), cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "Project file {FileName} not found at {Url} (HTTP {Status}). Skipping.",
                        fileName, fileUrl, response.StatusCode);
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (shouldReplace)
                {
                    content = ReplaceRuntimeVersion(content, language, runtimeVersion!);
                }

                files.Add(new ProjectTemplateFile
                {
                    FileName = fileName,
                    Content = content
                });
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "Error fetching project file {FileName} from {Url}", fileName, fileUrl);
            }
        }

        return files;
    }

    /// <summary>
    /// Converts a GitHub repository URL and folder path into a raw.githubusercontent.com URL.
    /// </summary>
    internal static string ConvertToRawGitHubUrl(string repositoryUrl, string folderPath)
    {
        // repositoryUrl: "https://github.com/Azure/azure-functions-templates-mcp-server"
        // folderPath: "templates/python/BlobTriggerWithEventGrid"
        // result: "https://raw.githubusercontent.com/Azure/azure-functions-templates-mcp-server/main/templates/python/..."

        var repoPath = repositoryUrl
            .Replace("https://github.com/", string.Empty, StringComparison.OrdinalIgnoreCase)
            .TrimEnd('/');

        return $"https://raw.githubusercontent.com/{repoPath}/{DefaultBranch}/{folderPath.TrimStart('/')}";
    }

    /// <summary>
    /// Fetches all files from a template directory using the GitHub Contents API,
    /// then returns them as <see cref="ProjectTemplateFile"/> instances.
    /// Files are separated into function and project files by the caller.
    /// </summary>
    internal async Task<IReadOnlyList<ProjectTemplateFile>> FetchTemplateFilesAsync(
        TemplateManifestEntry template,
        string language,
        string? runtimeVersion,
        CancellationToken cancellationToken)
    {
        var contentsUrl = ConstructGitHubContentsApiUrl(template.RepositoryUrl, template.FolderPath);
        var fileEntries = await ListGitHubDirectoryAsync(contentsUrl, cancellationToken);

        var shouldReplace = runtimeVersion is not null && (language is "java" or "typescript");
        var files = new List<ProjectTemplateFile>();
        var folderPrefix = template.FolderPath.TrimEnd('/') + "/";

        using var client = httpClientFactory.CreateClient();

        foreach (var entry in fileEntries)
        {
            if (entry.Size > MaxFileSizeBytes)
            {
                logger.LogWarning("Skipping file {Name} ({Size} bytes) - exceeds max size", entry.Name, entry.Size);
                continue;
            }

            if (entry.DownloadUrl is null)
            {
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

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (shouldReplace)
                {
                    content = ReplaceRuntimeVersion(content, language, runtimeVersion!);
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
    /// Lists all files in a GitHub directory recursively using the Contents API.
    /// </summary>
    internal async Task<IReadOnlyList<GitHubContentEntry>> ListGitHubDirectoryAsync(
        string contentsUrl,
        CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Azure-MCP-Server");

        try
        {
            return await ListGitHubDirectoryRecursiveAsync(client, contentsUrl, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to list GitHub directory at {Url}", contentsUrl);
            throw new InvalidOperationException(
                $"Could not list template files from GitHub. Details: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Constructs a GitHub Contents API URL from a repository URL and folder path.
    /// </summary>
    internal static string ConstructGitHubContentsApiUrl(string repositoryUrl, string folderPath)
    {
        var repoPath = repositoryUrl
            .Replace("https://github.com/", string.Empty, StringComparison.OrdinalIgnoreCase)
            .TrimEnd('/');

        return $"https://api.github.com/repos/{repoPath}/contents/{folderPath.TrimStart('/')}";
    }

    /// <summary>
    /// Extracts the template name from a CDN manifest folder path.
    /// e.g., "templates/python/HttpTrigger" → "HttpTrigger"
    /// </summary>
    internal static string ExtractTemplateName(string folderPath)
    {
        var trimmed = folderPath.TrimEnd('/');
        var lastSlash = trimmed.LastIndexOf('/');
        return lastSlash >= 0 ? trimmed[(lastSlash + 1)..] : trimmed;
    }

    /// <summary>
    /// Gets the filename component from a potentially nested path.
    /// </summary>
    private static string GetFileName(string path)
    {
        var lastSlash = path.LastIndexOf('/');
        return lastSlash >= 0 ? path[(lastSlash + 1)..] : path;
    }

    private static async Task<IReadOnlyList<GitHubContentEntry>> ListGitHubDirectoryRecursiveAsync(
        HttpClient client,
        string contentsUrl,
        CancellationToken cancellationToken)
    {
        var allFiles = new List<GitHubContentEntry>();

        using var response = await client.GetAsync(new Uri(contentsUrl), cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var entries = JsonSerializer.Deserialize(json, FunctionTemplatesManifestJsonContext.Default.ListGitHubContentEntry)
            ?? [];

        foreach (var entry in entries)
        {
            if (entry.Type == "file")
            {
                allFiles.Add(entry);
            }
            else if (entry.Type == "dir" && entry.Url is not null)
            {
                var subFiles = await ListGitHubDirectoryRecursiveAsync(client, entry.Url, cancellationToken);
                allFiles.AddRange(subFiles);
            }
        }

        return allFiles;
    }

    internal static void ValidateRuntimeVersion(string language, string runtimeVersion)
    {
        if (!s_supportedRuntimes.TryGetValue(language, out var runtime))
        {
            return;
        }

        var allVersions = new List<string>(runtime.Supported);
        if (runtime.Preview is not null)
        {
            allVersions.AddRange(runtime.Preview);
        }

        if (!allVersions.Contains(runtimeVersion))
        {
            var previewNote = runtime.Preview is { Count: > 0 }
                ? $" (preview: {string.Join(", ", runtime.Preview)})"
                : string.Empty;

            throw new ArgumentException(
                $"Invalid runtime version \"{runtimeVersion}\" for {language}. " +
                $"Supported versions: {string.Join(", ", runtime.Supported)}{previewNote}. " +
                $"Default: {runtime.Default}");
        }
    }

    internal static string ReplaceRuntimeVersion(string content, string language, string runtimeVersion)
    {
        if (language == "java")
        {
            var mavenVersion = runtimeVersion == "8" ? "1.8" : runtimeVersion;

            content = content.Replace(
                $"<java.version>{{{{javaVersion}}}}</java.version>",
                $"<java.version>{mavenVersion}</java.version>");

            content = content.Replace("{{javaVersion}}", runtimeVersion);
        }
        else if (language == "typescript")
        {
            content = content.Replace("{{nodeVersion}}", runtimeVersion);
        }

        return content;
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
