// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Options;

public static class FunctionsOptionDefinitions
{
    public const string LanguageName = "language";
    public const string RuntimeVersionName = "runtime-version";
    public const string TemplateName = "template";

    /// <summary>
    /// Supported languages for validation (must match LanguageMetadataProvider keys).
    /// </summary>
    public static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "python", "typescript", "javascript", "java", "csharp", "powershell"
    };

    public static readonly Option<string> Language = new($"--{LanguageName}")
    {
        Description = $"Programming language for the Azure Functions project. " +
            $"Valid values: {string.Join(", ", SupportedLanguages)}.",
        Required = true
    };

    public static readonly Option<string> RuntimeVersion = new($"--{RuntimeVersionName}")
    {
        Description = "Optional runtime version for Java or TypeScript/JavaScript. " +
            "When provided, template placeholders like {{javaVersion}} or {{nodeVersion}} are replaced automatically. " +
            "See 'functions language list' for supported versions.",
        Required = false
    };

    public static readonly Option<string> Template = new($"--{TemplateName}")
    {
        Description = "Name of the function template to retrieve (e.g., HttpTrigger, BlobTrigger). " +
            "Omit to list all available templates for the specified language.",
        Required = true
    };
}
