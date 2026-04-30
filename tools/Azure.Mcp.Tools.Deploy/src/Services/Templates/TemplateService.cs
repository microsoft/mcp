// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text;

namespace Azure.Mcp.Tools.Deploy.Services.Templates;

/// <summary>
/// Service for loading and processing embedded template resources.
/// </summary>
public static class TemplateService
{
    private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
    private const string TemplateNamespace = "Azure.Mcp.Tools.Deploy.Templates";

    /// <summary>
    /// Loads an embedded template resource by name.
    /// </summary>
    /// <param name="templateName">The name of the template file (without extension).</param>
    /// <returns>The template content as a string.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the template is not found.</exception>
    public static string LoadTemplate(string templateName)
    {
        string fileNamespace = TemplateNamespace;
        if (templateName.Contains('/'))
        {
            fileNamespace += "." + templateName.Split('/')[0];
            templateName = templateName.Split('/')[1];
        }
        var resourceName = $"{fileNamespace}.{templateName}.md";

        using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Template '{templateName}' not found in embedded resources.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Loads a template and replaces placeholders with provided values.
    /// </summary>
    /// <param name="templateName">The name of the template file (without extension).</param>
    /// <param name="replacements">Dictionary of placeholder names and their replacement values.</param>
    /// <returns>The processed template with placeholders replaced.</returns>
    public static string ProcessTemplate(string templateName, Dictionary<string, string> replacements)
    {
        var template = LoadTemplate(templateName);
        return ProcessTemplateContent(template, replacements);
    }

    /// <summary>
    /// Processes template content by replacing placeholders with provided values.
    /// </summary>
    /// <param name="templateContent">The template content to process.</param>
    /// <param name="replacements">Dictionary of placeholder names and their replacement values.</param>
    /// <returns>The processed template with placeholders replaced.</returns>
    public static string ProcessTemplateContent(string templateContent, Dictionary<string, string> replacements)
    {
        var result = new StringBuilder(templateContent);

        foreach (var (placeholder, value) in replacements)
        {
            var token = $"{{{{{placeholder}}}}}"; // {{placeholder}}
            result.Replace(token, value);
        }

        return result.ToString();
    }

}
