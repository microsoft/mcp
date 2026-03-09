// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Functions.Models;

namespace Azure.Mcp.Tools.Functions.Services;

/// <summary>
/// Provides language metadata for Azure Functions development.
/// This is the single source of truth for all language-related data.
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
    /// Complete language information including runtime versions, setup instructions,
    /// project structure, and template parameters.
    /// </summary>
    /// <remarks>
    /// Update these values when new runtime versions are released or deprecated.
    /// @see https://learn.microsoft.com/azure/azure-functions/functions-versions
    /// @see https://learn.microsoft.com/azure/azure-functions/supported-languages
    /// </remarks>
    private static readonly IReadOnlyDictionary<string, LanguageInfo> s_languageInfo =
        new Dictionary<string, LanguageInfo>(StringComparer.OrdinalIgnoreCase)
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
                BuildCommand = null,
                ProjectFiles = ["requirements.txt"],
                RuntimeVersions = new RuntimeVersionInfo
                {
                    Supported = ["3.10", "3.11", "3.12", "3.13"],
                    Preview = ["3.14"],
                    Default = "3.11"
                },
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
                TemplateParameters = null
            },
            ["typescript"] = new LanguageInfo
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
                RuntimeVersions = new RuntimeVersionInfo
                {
                    Supported = ["20", "22"],
                    Preview = ["24"],
                    Default = "22"
                },
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
                TemplateParameters =
                [
                    new TemplateParameter
                    {
                        Name = "nodeVersion",
                        Description = "Node.js version for @types/node. Detect from user environment or ask preference.",
                        DefaultValue = "20",
                        ValidValues = ["20", "22", "24"]
                    }
                ],
                RecommendationNotes = "Recommended for Node.js runtime for type safety and better tooling support."
            },
            ["javascript"] = new LanguageInfo
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
                RuntimeVersions = new RuntimeVersionInfo
                {
                    Supported = ["20", "22"],
                    Preview = ["24"],
                    Default = "22"
                },
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
                TemplateParameters =
                [
                    new TemplateParameter
                    {
                        Name = "nodeVersion",
                        Description = "Node.js version. Detect from user environment or ask preference.",
                        DefaultValue = "20",
                        ValidValues = ["20", "22", "24"]
                    }
                ]
            },
            ["java"] = new LanguageInfo
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
                RuntimeVersions = new RuntimeVersionInfo
                {
                    Supported = ["8", "11", "17", "21"],
                    Preview = ["25"],
                    Default = "21"
                },
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
                TemplateParameters =
                [
                    new TemplateParameter
                    {
                        Name = "javaVersion",
                        Description = "Java version for compilation and runtime. Detect from user environment or ask preference.",
                        DefaultValue = "21",
                        ValidValues = ["8", "11", "17", "21", "25"]
                    }
                ]
            },
            ["csharp"] = new LanguageInfo
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
                RuntimeVersions = new RuntimeVersionInfo
                {
                    Supported = ["8", "9", "10"],
                    Deprecated = ["6", "7"],
                    Default = "8",
                    FrameworkSupported = ["4.8.1"]
                },
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
                TemplateParameters = null
            },
            ["powershell"] = new LanguageInfo
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
                RuntimeVersions = new RuntimeVersionInfo
                {
                    Supported = ["7.4"],
                    Deprecated = ["7.2"],
                    Default = "7.4"
                },
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
                TemplateParameters = null
            }
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
    public LanguageInfo? GetLanguageInfo(string language) =>
        s_languageInfo.TryGetValue(language, out var info) ? info : null;

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<string, LanguageInfo>> GetAllLanguages() =>
        s_languageInfo;

    /// <inheritdoc />
    public IReadOnlySet<string> KnownProjectFiles => s_knownProjectFiles.Value;

    /// <inheritdoc />
    public void ValidateRuntimeVersion(string language, string runtimeVersion)
    {
        if (!s_languageInfo.TryGetValue(language, out var languageInfo))
        {
            return;
        }

        var runtime = languageInfo.RuntimeVersions;
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
}
