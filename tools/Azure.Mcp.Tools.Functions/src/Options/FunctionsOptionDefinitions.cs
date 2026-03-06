// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Options;

public static class FunctionsOptionDefinitions
{
    public const string LanguageName = "language";
    public const string RuntimeVersionName = "runtime-version";
    public const string TemplateName = "template";

    public static readonly Option<string> Language = new($"--{LanguageName}")
    {
        Description = "Programming language for the Azure Functions project. " +
            "Valid values: python, typescript, java, csharp.",
        Required = true
    };

    public static readonly Option<string> RuntimeVersion = new($"--{RuntimeVersionName}")
    {
        Description = "Optional runtime version for the project. " +
            "For Java: JDK version (8, 11, 17, 21, 25). " +
            "For TypeScript: Node.js version (20, 22, 24). " +
            "When provided, template placeholders like {{javaVersion}} or {{nodeVersion}} are replaced automatically.",
        Required = false
    };

    public static readonly Option<string> Template = new($"--{TemplateName}")
    {
        Description = "Name of the function template to retrieve (e.g., HttpTrigger, BlobTrigger). " +
            "Omit to list all available templates for the specified language.",
        Required = true
    };
}
