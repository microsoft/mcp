// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Functions.Models;

namespace Azure.Mcp.Tools.Functions.Services;

/// <summary>
/// Provides language metadata for Azure Functions development.
/// Static language info is defined here; runtime versions come from manifest.json.
/// </summary>
public sealed class LanguageMetadataProvider : ILanguageMetadataProvider
{
    /// <summary>
    /// Azure Functions runtime version.
    /// </summary>
    public string FunctionsRuntimeVersion => "4.x";

    /// <summary>
    /// Extension bundle version range.
    /// </summary>
    public string ExtensionBundleVersion => "[4.*, 5.0.0)";

    /// <summary>
    /// Common project files present in all Azure Functions projects.
    /// </summary>
    private static readonly string[] s_commonProjectFiles =
        ["host.json", "local.settings.json", ".funcignore", ".gitignore"];

    /// <summary>
    /// Maps language keys to manifest runtime version keys.
    /// Manifest uses PascalCase keys (e.g., "Python", "CSharp").
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> s_languageToManifestKey =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["python"] = "Python",
            ["typescript"] = "TypeScript",
            ["javascript"] = "JavaScript",
            ["java"] = "Java",
            ["csharp"] = "CSharp",
            ["powershell"] = "PowerShell"
        };

    /// <summary>
    /// Static language information excluding runtime versions.
    /// Runtime versions are provided by manifest.json.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, LanguageInfoStatic> s_languageInfo =
        new Dictionary<string, LanguageInfoStatic>(StringComparer.OrdinalIgnoreCase)
        {
            ["python"] = new LanguageInfoStatic
            {
                Name = "Python",
                Runtime = "python",
                ProgrammingModel = "v2 (Decorator-based)",
                Prerequisites = ["Python 3.10+", "Azure Functions Core Tools v4"],
                DevelopmentTools = ["VS Code with Azure Functions extension", "Azure Functions Core Tools"],
                InitCommand = "func init --worker-runtime python --model V2",
                RunCommand = "func start",
                BuildCommand = null,
                ProjectFiles = ["requirements.txt"],
                InitInstructions = """
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
                    """,
                ProjectStructure =
                [
                    "function_app.py    # Main application file with all functions",
                    "host.json          # Azure Functions host configuration",
                    "local.settings.json # Local development settings (do not commit)",
                    "requirements.txt   # Python dependencies",
                    "README.md          # Project documentation",
                    ".gitignore         # Git ignore patterns",
                    ".funcignore        # Files to exclude from deployment"
                ],
                TemplateParameterName = null,
                RecommendationNotes = null
            },
            ["typescript"] = new LanguageInfoStatic
            {
                Name = "Node.js - TypeScript",
                Runtime = "node",
                ProgrammingModel = "v4 (Schema-based)",
                Prerequisites = ["Node.js 20+", "Azure Functions Core Tools v4", "TypeScript 4.x+"],
                DevelopmentTools = ["VS Code with Azure Functions extension", "Azure Functions Core Tools"],
                InitCommand = "func init --worker-runtime node --language typescript --model V4",
                RunCommand = "npm start",
                BuildCommand = "npm run build",
                ProjectFiles = ["package.json", "tsconfig.json"],
                InitInstructions = """
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
                    """,
                ProjectStructure =
                [
                    "src/functions/     # Function implementation files",
                    "host.json          # Azure Functions host configuration",
                    "local.settings.json # Local development settings (do not commit)",
                    "package.json       # Node.js dependencies and scripts",
                    "tsconfig.json      # TypeScript compiler configuration",
                    "README.md          # Project documentation",
                    ".gitignore         # Git ignore patterns",
                    ".funcignore        # Files to exclude from deployment"
                ],
                TemplateParameterName = "nodeVersion",
                RecommendationNotes = "Recommended for Node.js runtime for type safety and better tooling support."
            },
            ["javascript"] = new LanguageInfoStatic
            {
                Name = "Node.js - JavaScript",
                Runtime = "node",
                ProgrammingModel = "v4 (Schema-based)",
                Prerequisites = ["Node.js 20+", "Azure Functions Core Tools v4"],
                DevelopmentTools = ["VS Code with Azure Functions extension", "Azure Functions Core Tools"],
                InitCommand = "func init --worker-runtime node --language javascript --model V4",
                RunCommand = "npm start",
                BuildCommand = null,
                ProjectFiles = ["package.json"],
                InitInstructions = """
                    ## JavaScript Azure Functions Project Setup

                    1. Install dependencies:
                       ```bash
                       npm install
                       ```

                    2. Create your functions in `src/functions/` directory

                    3. Run locally:
                       ```bash
                       npm start
                       ```

                    4. For development:
                       ```bash
                       func start
                       ```
                    """,
                ProjectStructure =
                [
                    "src/functions/     # Function implementation files",
                    "host.json          # Azure Functions host configuration",
                    "local.settings.json # Local development settings (do not commit)",
                    "package.json       # Node.js dependencies and scripts",
                    "README.md          # Project documentation",
                    ".gitignore         # Git ignore patterns",
                    ".funcignore        # Files to exclude from deployment"
                ],
                TemplateParameterName = "nodeVersion",
                RecommendationNotes = null
            },
            ["java"] = new LanguageInfoStatic
            {
                Name = "Java",
                Runtime = "java",
                ProgrammingModel = "Annotations-based",
                Prerequisites = ["JDK (see RuntimeVersions for supported versions)", "Apache Maven 3.x", "Azure Functions Core Tools v4"],
                DevelopmentTools = ["VS Code with Java + Azure Functions extensions", "IntelliJ IDEA", "Azure Functions Core Tools"],
                InitCommand = "mvn archetype:generate -DarchetypeGroupId=com.microsoft.azure -DarchetypeArtifactId=azure-functions-archetype",
                RunCommand = "mvn clean package && mvn azure-functions:run",
                BuildCommand = "mvn clean package",
                ProjectFiles = ["pom.xml"],
                InitInstructions = """
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
                    """,
                ProjectStructure =
                [
                    "src/main/java/     # Java source files",
                    "pom.xml            # Maven project configuration (from template)",
                    "host.json          # Azure Functions host configuration",
                    "local.settings.json # Local development settings (do not commit)",
                    "README.md          # Project documentation",
                    ".gitignore        # Git ignore patterns",
                    ".funcignore        # Files to exclude from deployment"
                ],
                TemplateParameterName = "javaVersion",
                RecommendationNotes = null
            },
            ["csharp"] = new LanguageInfoStatic
            {
                Name = "dotnet-isolated - C#",
                Runtime = "dotnet",
                ProgrammingModel = "Isolated worker process",
                Prerequisites = [".NET 8 SDK or later", "Azure Functions Core Tools v4"],
                DevelopmentTools = ["Visual Studio 2022 or later", "VS Code with C# + Azure Functions extensions", "Azure Functions Core Tools"],
                InitCommand = "func init --worker-runtime dotnet-isolated",
                RunCommand = "func start",
                BuildCommand = "dotnet build",
                ProjectFiles = [],
                InitInstructions = """
                    ## C# Azure Functions Project Setup

                    1. Create project using .NET CLI:
                       ```bash
                       func init --worker-runtime dotnet-isolated
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
                    """,
                ProjectStructure =
                [
                    "*.csproj            # C# project file",
                    "Program.cs          # Application entry point",
                    "host.json           # Azure Functions host configuration",
                    "local.settings.json # Local development settings (do not commit)",
                    "README.md           # Project documentation",
                    ".gitignore          # Git ignore patterns",
                    ".funcignore         # Files to exclude from deployment"
                ],
                TemplateParameterName = null,
                RecommendationNotes = null
            },
            ["powershell"] = new LanguageInfoStatic
            {
                Name = "PowerShell",
                Runtime = "powershell",
                ProgrammingModel = "Script-based",
                Prerequisites = ["PowerShell 7.4+", "Azure Functions Core Tools v4"],
                DevelopmentTools = ["VS Code with PowerShell + Azure Functions extensions", "Azure Functions Core Tools"],
                InitCommand = "func init --worker-runtime powershell",
                RunCommand = "func start",
                BuildCommand = null,
                ProjectFiles = ["requirements.psd1", "profile.ps1"],
                InitInstructions = """
                    ## PowerShell Azure Functions Project Setup

                    1. Ensure PowerShell 7.4+ is installed

                    2. Create your functions in individual folders with `function.json` and `run.ps1`

                    3. Edit `profile.ps1` for app-level initialization code

                    4. Add module dependencies to `requirements.psd1`

                    5. Run locally:
                       ```powershell
                       func start
                       ```
                    """,
                ProjectStructure =
                [
                    "*/run.ps1          # Function script files",
                    "*/function.json    # Function binding configuration",
                    "host.json          # Azure Functions host configuration",
                    "local.settings.json # Local development settings (do not commit)",
                    "profile.ps1        # App-level initialization script",
                    "requirements.psd1  # PowerShell module dependencies",
                    "README.md          # Project documentation",
                    ".gitignore         # Git ignore patterns",
                    ".funcignore        # Files to exclude from deployment"
                ],
                TemplateParameterName = null,
                RecommendationNotes = null
            }
        };

    /// <summary>
    /// Fallback runtime versions used when manifest doesn't provide them.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, RuntimeVersionInfo> s_fallbackRuntimeVersions =
        new Dictionary<string, RuntimeVersionInfo>(StringComparer.OrdinalIgnoreCase)
        {
            ["python"] = new RuntimeVersionInfo { Supported = ["3.13"], Default = "3.13" },
            ["typescript"] = new RuntimeVersionInfo { Supported = ["20"], Default = "20" },
            ["javascript"] = new RuntimeVersionInfo { Supported = ["20"], Default = "20" },
            ["java"] = new RuntimeVersionInfo { Supported = ["21"], Default = "21" },
            ["csharp"] = new RuntimeVersionInfo { Supported = ["8"], Default = "8" },
            ["powershell"] = new RuntimeVersionInfo { Supported = ["7.4"], Default = "7.4" }
        };

    /// <summary>
    /// Flat set of known project-level filenames used to separate project files
    /// from function-specific files in template get mode.
    /// </summary>
    private static readonly Lazy<HashSet<string>> s_knownProjectFiles = new(() =>
        s_languageInfo.Values
            .SelectMany(l => l.ProjectFiles)
            .Concat(s_commonProjectFiles)
            .ToHashSet(StringComparer.OrdinalIgnoreCase));

    /// <inheritdoc />
    public IEnumerable<string> SupportedLanguages => s_languageInfo.Keys;

    /// <inheritdoc />
    public bool IsValidLanguage(string language) =>
        s_languageInfo.ContainsKey(language);

    /// <inheritdoc />
    public LanguageInfo? GetLanguageInfo(string language, IReadOnlyDictionary<string, RuntimeVersionInfo>? manifestRuntimeVersions = null)
    {
        if (!s_languageInfo.TryGetValue(language, out var staticInfo))
        {
            return null;
        }

        var runtimeVersions = GetRuntimeVersions(language, manifestRuntimeVersions);
        return BuildLanguageInfo(staticInfo, runtimeVersions);
    }

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<string, LanguageInfo>> GetAllLanguages(IReadOnlyDictionary<string, RuntimeVersionInfo>? manifestRuntimeVersions = null)
    {
        foreach (var kvp in s_languageInfo)
        {
            var runtimeVersions = GetRuntimeVersions(kvp.Key, manifestRuntimeVersions);
            var languageInfo = BuildLanguageInfo(kvp.Value, runtimeVersions);
            yield return new KeyValuePair<string, LanguageInfo>(kvp.Key, languageInfo);
        }
    }

    /// <inheritdoc />
    public IReadOnlySet<string> KnownProjectFiles => s_knownProjectFiles.Value;

    /// <inheritdoc />
    public void ValidateRuntimeVersion(string language, string runtimeVersion, IReadOnlyDictionary<string, RuntimeVersionInfo>? manifestRuntimeVersions = null)
    {
        if (!s_languageInfo.ContainsKey(language))
        {
            return;
        }

        var runtime = GetRuntimeVersions(language, manifestRuntimeVersions);
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

    /// <inheritdoc />
    public string ReplaceRuntimeVersion(string content, string language, string runtimeVersion)
    {
        if (language == "java")
        {
            // Maven requires Java 8 to be specified as "1.8" for compatibility
            var mavenVersion = runtimeVersion == "8" ? "1.8" : runtimeVersion;

            // First pass: Replace Maven's <java.version> property with Maven-compatible version (1.8 for Java 8)
            content = content.Replace(
                "<java.version>{{javaVersion}}</java.version>",
                $"<java.version>{mavenVersion}</java.version>");

            // Second pass: Replace all remaining {{javaVersion}} with original version (8 stays as 8)
            content = content.Replace("{{javaVersion}}", runtimeVersion);
        }
        else if (language is "typescript" or "javascript")
        {
            content = content.Replace("{{nodeVersion}}", runtimeVersion);
        }

        return content;
    }

    /// <summary>
    /// Gets runtime versions for a language, preferring manifest data over fallback.
    /// </summary>
    private RuntimeVersionInfo GetRuntimeVersions(string language, IReadOnlyDictionary<string, RuntimeVersionInfo>? manifestRuntimeVersions)
    {
        // Try manifest first using the PascalCase key mapping
        if (manifestRuntimeVersions is not null &&
            s_languageToManifestKey.TryGetValue(language, out var manifestKey) &&
            manifestRuntimeVersions.TryGetValue(manifestKey, out var manifestVersions))
        {
            return manifestVersions;
        }

        // Fall back to hardcoded defaults
        return s_fallbackRuntimeVersions.TryGetValue(language, out var fallback)
            ? fallback
            : new RuntimeVersionInfo { Supported = [], Default = string.Empty };
    }

    /// <summary>
    /// Builds a LanguageInfo from static data and runtime versions.
    /// </summary>
    private static LanguageInfo BuildLanguageInfo(LanguageInfoStatic staticInfo, RuntimeVersionInfo runtimeVersions)
    {
        // Build template parameters with valid values from runtime versions
        IReadOnlyList<TemplateParameter>? templateParameters = null;
        if (staticInfo.TemplateParameterName is not null)
        {
            var allVersions = new List<string>(runtimeVersions.Supported);
            if (runtimeVersions.Preview is not null)
            {
                allVersions.AddRange(runtimeVersions.Preview);
            }

            templateParameters =
            [
                new TemplateParameter
                {
                    Name = staticInfo.TemplateParameterName,
                    Description = staticInfo.TemplateParameterName == "javaVersion"
                        ? "Java version for compilation and runtime. Detect from user environment or ask preference."
                        : "Node.js version. Detect from user environment or ask preference.",
                    DefaultValue = runtimeVersions.Default,
                    ValidValues = allVersions
                }
            ];
        }

        return new LanguageInfo
        {
            Name = staticInfo.Name,
            Runtime = staticInfo.Runtime,
            ProgrammingModel = staticInfo.ProgrammingModel,
            Prerequisites = staticInfo.Prerequisites,
            DevelopmentTools = staticInfo.DevelopmentTools,
            InitCommand = staticInfo.InitCommand,
            RunCommand = staticInfo.RunCommand,
            BuildCommand = staticInfo.BuildCommand,
            ProjectFiles = staticInfo.ProjectFiles,
            RuntimeVersions = runtimeVersions,
            InitInstructions = staticInfo.InitInstructions,
            ProjectStructure = staticInfo.ProjectStructure,
            TemplateParameters = templateParameters,
            RecommendationNotes = staticInfo.RecommendationNotes
        };
    }

    /// <summary>
    /// Internal record for static language info without runtime versions.
    /// </summary>
    private sealed record LanguageInfoStatic
    {
        public required string Name { get; init; }
        public required string Runtime { get; init; }
        public required string ProgrammingModel { get; init; }
        public required IReadOnlyList<string> Prerequisites { get; init; }
        public required IReadOnlyList<string> DevelopmentTools { get; init; }
        public required string InitCommand { get; init; }
        public required string RunCommand { get; init; }
        public string? BuildCommand { get; init; }
        public required IReadOnlyList<string> ProjectFiles { get; init; }
        public required string InitInstructions { get; init; }
        public required IReadOnlyList<string> ProjectStructure { get; init; }
        public string? TemplateParameterName { get; init; }
        public string? RecommendationNotes { get; init; }
    }
}
