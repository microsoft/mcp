// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Azure.Mcp.Tools.AzureTerraform.Models;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

public sealed partial class AzureRMDocsService(IHttpClientFactory httpClientFactory) : IAzureRMDocsService
{
    private const string BaseResourcesUrl =
        "https://raw.githubusercontent.com/hashicorp/terraform-provider-azurerm/main/website/docs/r";
    private const string BaseDataSourcesUrl =
        "https://raw.githubusercontent.com/hashicorp/terraform-provider-azurerm/main/website/docs/d";

    public async Task<AzureRMDocsResult> GetDocumentationAsync(
        string resourceTypeName,
        string docType = "resource",
        string? argumentName = null,
        string? attributeName = null,
        CancellationToken cancellationToken = default)
    {
        // Normalize resource type — remove azurerm_ prefix if present
        string normalizedType = resourceTypeName.ToLowerInvariant().Replace("azurerm_", "", StringComparison.Ordinal);

        bool isDataSource = docType.Equals("data-source", StringComparison.OrdinalIgnoreCase)
            || docType.Equals("datasource", StringComparison.OrdinalIgnoreCase)
            || docType.Equals("data_source", StringComparison.OrdinalIgnoreCase);

        string docUrl = isDataSource
            ? $"{BaseDataSourcesUrl}/{normalizedType}.html.markdown"
            : $"{BaseResourcesUrl}/{normalizedType}.html.markdown";

        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept", "text/plain");
        client.DefaultRequestHeaders.Add("User-Agent", "Azure-Terraform-MCP-Server");

        string? markdownContent = null;
        bool isDataSourceUrl = isDataSource;

        var response = await client.GetAsync(new Uri(docUrl), cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            // Try the other type as fallback
            string fallbackUrl = isDataSource
                ? $"{BaseResourcesUrl}/{normalizedType}.html.markdown"
                : $"{BaseDataSourcesUrl}/{normalizedType}.html.markdown";

            var fallbackResponse = await client.GetAsync(new Uri(fallbackUrl), cancellationToken).ConfigureAwait(false);
            if (fallbackResponse.IsSuccessStatusCode)
            {
                markdownContent = await fallbackResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                docUrl = fallbackUrl;
                isDataSourceUrl = fallbackUrl.Contains("docs/d/", StringComparison.Ordinal);
            }
            else
            {
                return new AzureRMDocsResult
                {
                    ResourceType = resourceTypeName,
                    DocumentationUrl = docUrl,
                    Summary = $"Documentation not found for {resourceTypeName} (HTTP {(int)response.StatusCode}). " +
                        "Please double-check the resource type name is correct. " +
                        "If this resource is not available in the AzureRM provider, consider using the AzAPI provider instead."
                };
            }
        }

        markdownContent ??= await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var result = new AzureRMDocsResult
        {
            ResourceType = resourceTypeName,
            DocumentationUrl = docUrl,
            Summary = ExtractSummary(markdownContent, resourceTypeName, isDataSourceUrl),
            Arguments = ExtractArguments(markdownContent, isDataSourceUrl),
            Attributes = ExtractAttributes(markdownContent),
            Examples = ExtractExamples(markdownContent, normalizedType, isDataSourceUrl),
            Notes = ExtractNotes(markdownContent)
        };

        // Populate block argument definitions
        var blockDefinitions = ExtractBlockDefinitions(markdownContent);
        foreach (var arg in result.Arguments)
        {
            if (arg.Type == "Block" && blockDefinitions.TryGetValue(arg.Name, out var blockArgs))
            {
                arg.BlockArguments = blockArgs;
            }
        }

        if (!string.IsNullOrEmpty(argumentName))
        {
            result = FilterToArgument(result, argumentName);
        }

        if (!string.IsNullOrEmpty(attributeName))
        {
            result = FilterToAttribute(result, attributeName);
        }

        return result;
    }

    private static AzureRMDocsResult FilterToArgument(AzureRMDocsResult result, string argumentName)
    {
        var matching = result.Arguments
            .Where(a => a.Name.Equals(argumentName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        result.Arguments = matching;
        result.Summary = matching.Count > 0
            ? $"Argument details for '{argumentName}' in {result.ResourceType}"
            : $"Argument '{argumentName}' not found in {result.ResourceType}";

        return result;
    }

    private static AzureRMDocsResult FilterToAttribute(AzureRMDocsResult result, string attributeName)
    {
        var matching = result.Attributes
            .Where(a => a.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        result.Attributes = matching;
        result.Summary = matching.Count > 0
            ? $"Attribute details for '{attributeName}' in {result.ResourceType}"
            : $"Attribute '{attributeName}' not found in {result.ResourceType}";

        return result;
    }

    internal static string ExtractSummary(string markdownContent, string resourceType, bool isDataSource)
    {
        string[] lines = markdownContent.Split('\n');
        bool inFrontmatter = false;
        bool frontmatterEnded = false;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (line == "---")
            {
                if (!inFrontmatter)
                {
                    inFrontmatter = true;
                    continue;
                }
                else
                {
                    frontmatterEnded = true;
                    continue;
                }
            }

            if (inFrontmatter && !frontmatterEnded)
            {
                continue;
            }

            if (frontmatterEnded && line.Length > 20 && !line.StartsWith('#'))
            {
                return line;
            }
        }

        return GenerateDefaultSummary(resourceType, isDataSource);
    }

    private static string GenerateDefaultSummary(string resourceType, bool isDataSource)
    {
        string displayName = Regex.Replace(resourceType.Replace('_', ' '), @"\b\w", m => m.Value.ToUpperInvariant());

        return isDataSource
            ? $"Use this data source to access information about an existing {displayName}."
            : $"Manages an Azure {displayName} resource.";
    }

    internal static List<ArgumentDetail> ExtractArguments(string markdownContent, bool isDataSource)
    {
        var args = new List<ArgumentDetail>();
        string[] lines = markdownContent.Split('\n');
        bool inArgumentsSection = false;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (ArgumentsSectionHeader().IsMatch(line))
            {
                inArgumentsSection = true;
                continue;
            }

            if (inArgumentsSection)
            {
                if (line.StartsWith("## ", StringComparison.Ordinal)
                    && !ArgumentsSectionHeader().IsMatch(line))
                {
                    break;
                }

                if (BlockDefinitionHeader().IsMatch(line))
                {
                    break;
                }
            }

            if (!inArgumentsSection || string.IsNullOrEmpty(line))
            {
                continue;
            }

            var match = ArgumentDefinition().Match(line);
            if (!match.Success)
            {
                continue;
            }

            string argName = match.Groups[1].Value.Trim();
            string description = match.Groups[2].Value.Trim();
            bool required = description.Contains("(Required)", StringComparison.OrdinalIgnoreCase);

            string cleanedDescription = RequiredOptionalTag().Replace(description, "").Trim();
            cleanedDescription = LeadingDash().Replace(cleanedDescription, "").Trim();

            bool isBlock = description.Contains("block", StringComparison.OrdinalIgnoreCase);

            args.Add(new ArgumentDetail
            {
                Name = argName,
                Description = cleanedDescription,
                Required = required,
                Type = isBlock ? "Block" : "Single",
                BlockArguments = isBlock ? [] : null
            });
        }

        if (args.Count == 0)
        {
            return GetDefaultArguments(isDataSource);
        }

        return args;
    }

    private static List<ArgumentDetail> GetDefaultArguments(bool isDataSource)
    {
        if (isDataSource)
        {
            return
            [
                new() { Name = "name", Description = "Specifies the name of the resource to retrieve information about.", Required = false, Type = "Single" },
                new() { Name = "resource_group_name", Description = "The name of the resource group containing the resource.", Required = false, Type = "Single" }
            ];
        }

        return
        [
            new() { Name = "name", Description = "Specifies the name of the resource.", Required = true, Type = "Single" },
            new() { Name = "resource_group_name", Description = "The name of the resource group in which to create the resource.", Required = true, Type = "Single" },
            new() { Name = "location", Description = "Specifies the supported Azure location where the resource exists.", Required = true, Type = "Single" },
            new() { Name = "tags", Description = "A mapping of tags to assign to the resource.", Required = false, Type = "Single" }
        ];
    }

    internal static Dictionary<string, List<ArgumentDetail>> ExtractBlockDefinitions(string markdownContent)
    {
        var blockDefinitions = new Dictionary<string, List<ArgumentDetail>>(StringComparer.OrdinalIgnoreCase);
        string[] lines = markdownContent.Split('\n');

        string? currentBlockName = null;
        var currentBlockArgs = new List<ArgumentDetail>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            var headerMatch = BlockDefinitionHeader().Match(line);
            if (headerMatch.Success)
            {
                if (currentBlockName != null && currentBlockArgs.Count > 0)
                {
                    blockDefinitions[currentBlockName] = currentBlockArgs;
                }

                currentBlockName = headerMatch.Groups[1].Value.Trim();
                currentBlockArgs = [];
                continue;
            }

            if (currentBlockName == null)
            {
                continue;
            }

            string nextLine = i + 1 < lines.Length ? lines[i + 1].Trim() : "";

            if (line == "---" || line.StartsWith("## ", StringComparison.Ordinal)
                || (string.IsNullOrEmpty(line) && BlockDefinitionHeader().IsMatch(nextLine)))
            {
                if (currentBlockArgs.Count > 0)
                {
                    blockDefinitions[currentBlockName] = currentBlockArgs;
                }
                currentBlockName = null;
                currentBlockArgs = [];
                continue;
            }

            var argMatch = ArgumentDefinition().Match(line);
            if (!argMatch.Success)
            {
                continue;
            }

            string argName = argMatch.Groups[1].Value.Trim();
            string description = argMatch.Groups[2].Value.Trim();
            bool required = description.Contains("(Required)", StringComparison.OrdinalIgnoreCase);

            string cleanedDescription = RequiredOptionalTag().Replace(description, "").Trim();
            cleanedDescription = LeadingDash().Replace(cleanedDescription, "").Trim();

            bool isNestedBlock = description.Contains("block", StringComparison.OrdinalIgnoreCase);

            currentBlockArgs.Add(new ArgumentDetail
            {
                Name = argName,
                Description = cleanedDescription,
                Required = required,
                Type = isNestedBlock ? "Block" : "Single",
                BlockArguments = isNestedBlock ? [] : null
            });
        }

        if (currentBlockName != null && currentBlockArgs.Count > 0)
        {
            blockDefinitions[currentBlockName] = currentBlockArgs;
        }

        return blockDefinitions;
    }

    internal static List<AttributeDetail> ExtractAttributes(string markdownContent)
    {
        var attributes = new List<AttributeDetail>
        {
            new() { Name = "id", Description = "The ID of the resource." }
        };

        string[] lines = markdownContent.Split('\n');
        bool inAttributesSection = false;
        string? currentBlock = null;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (AttributesSectionHeader().IsMatch(line))
            {
                inAttributesSection = true;
                continue;
            }

            if (inAttributesSection && line.StartsWith("## ", StringComparison.Ordinal)
                && !AttributesSectionHeader().IsMatch(line))
            {
                break;
            }

            if (!inAttributesSection)
            {
                continue;
            }

            var match = ArgumentDefinition().Match(line);
            if (match.Success)
            {
                string attrName = match.Groups[1].Value.Trim();
                string description = match.Groups[2].Value.Trim();

                if (!attributes.Exists(a => a.Name == attrName))
                {
                    attributes.Add(new AttributeDetail { Name = attrName, Description = description });
                }

                if (line.Contains("block", StringComparison.OrdinalIgnoreCase))
                {
                    currentBlock = attrName;
                }
            }

            // Check for nested block attributes (indented)
            var nestedMatch = NestedArgumentDefinition().Match(rawLine);
            if (nestedMatch.Success && currentBlock != null)
            {
                string nestedAttr = nestedMatch.Groups[1].Value.Trim();
                string nestedDesc = nestedMatch.Groups[2].Value.Trim();
                string fullName = $"{currentBlock}.{nestedAttr}";

                if (!attributes.Exists(a => a.Name == fullName))
                {
                    attributes.Add(new AttributeDetail { Name = fullName, Description = $"(Block attribute) {nestedDesc}" });
                }
            }
        }

        return attributes;
    }

    internal static List<string> ExtractExamples(string markdownContent, string normalizedType, bool isDataSource)
    {
        var examples = new List<string>();
        string[] lines = markdownContent.Split('\n');

        bool inCodeBlock = false;
        var currentCode = new List<string>();
        string codeBlockLang = "";

        foreach (string rawLine in lines)
        {
            string trimmed = rawLine.Trim();

            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                if (!inCodeBlock)
                {
                    inCodeBlock = true;
                    codeBlockLang = trimmed[3..].Trim().ToLowerInvariant();
                    currentCode.Clear();
                }
                else
                {
                    inCodeBlock = false;

                    if ((codeBlockLang is "hcl" or "terraform" or "") && currentCode.Count > 0)
                    {
                        string codeText = string.Join('\n', currentCode).Trim();
                        string blockType = isDataSource ? "data" : "resource";
                        string resourceName = normalizedType.Replace('-', '_');

                        if (codeText.Contains(blockType, StringComparison.Ordinal)
                            && (codeText.Contains($"azurerm_{resourceName}", StringComparison.Ordinal)
                                || codeText.Contains($"\"{resourceName}\"", StringComparison.Ordinal)
                                || codeText.Contains(resourceName, StringComparison.Ordinal)))
                        {
                            examples.Add(codeText);
                            if (examples.Count >= 3) break;
                        }
                    }

                    currentCode.Clear();
                    codeBlockLang = "";
                }
            }
            else if (inCodeBlock)
            {
                currentCode.Add(rawLine);
            }
        }

        if (examples.Count == 0)
        {
            examples.Add(GenerateDefaultExample(normalizedType, isDataSource));
        }

        return examples;
    }

    private static string GenerateDefaultExample(string normalizedType, bool isDataSource)
    {
        string resourceName = normalizedType.Replace('-', '_');

        if (isDataSource)
        {
            return $$"""
                data "azurerm_{{resourceName}}" "example" {
                  name                = "example-{{normalizedType}}"
                  resource_group_name = "example-resource-group"
                }

                output "{{resourceName}}_id" {
                  value = data.azurerm_{{resourceName}}.example.id
                }
                """;
        }

        return $$"""
            resource "azurerm_{{resourceName}}" "example" {
              name                = "example-{{normalizedType}}"
              resource_group_name = azurerm_resource_group.example.name
              location            = azurerm_resource_group.example.location

              tags = {
                Environment = "Development"
              }
            }
            """;
    }

    internal static List<string> ExtractNotes(string markdownContent)
    {
        var notes = new List<string>();
        string[] lines = markdownContent.Split('\n');

        bool inNoteBlock = false;
        var currentNote = new List<string>();

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            string? noteContent = TryMatchNote(line);
            if (noteContent != null)
            {
                if (currentNote.Count > 0)
                {
                    string noteText = string.Join(' ', currentNote).Trim();
                    if (noteText.Length > 0) notes.Add(noteText);
                }

                currentNote = noteContent.Length > 0 ? [noteContent] : [];
                inNoteBlock = true;
                continue;
            }

            if (inNoteBlock)
            {
                if (line.StartsWith('>') || line.StartsWith("->", StringComparison.Ordinal) || line.StartsWith("~>", StringComparison.Ordinal))
                {
                    string cleanLine = line;
                    cleanLine = LeadingBlockquote().Replace(cleanLine, "").Trim();
                    if (cleanLine.Length > 0) currentNote.Add(cleanLine);
                }
                else
                {
                    if (currentNote.Count > 0)
                    {
                        string noteText = string.Join(' ', currentNote).Trim();
                        if (noteText.Length > 0) notes.Add(noteText);
                        currentNote.Clear();
                    }
                    inNoteBlock = false;
                }
            }
        }

        if (currentNote.Count > 0)
        {
            string noteText = string.Join(' ', currentNote).Trim();
            if (noteText.Length > 0) notes.Add(noteText);
        }

        // Deduplicate and clean
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var cleaned = new List<string>();

        foreach (string note in notes)
        {
            string clean = BoldMarkdown().Replace(note, "$1");
            clean = ItalicMarkdown().Replace(clean, "$1");
            clean = CodeMarkdown().Replace(clean, "$1");
            clean = LinkMarkdown().Replace(clean, "$1");
            clean = clean.Trim();

            if (clean.Length > 10 && seen.Add(clean.ToLowerInvariant()))
            {
                cleaned.Add(clean);
            }
        }

        return cleaned;
    }

    private static string? TryMatchNote(string line)
    {
        var match = NotePattern1().Match(line);
        if (match.Success) return match.Groups[1].Value.Trim();

        match = NotePattern2().Match(line);
        if (match.Success) return match.Groups[1].Value.Trim();

        match = NotePattern3().Match(line);
        if (match.Success) return match.Groups[1].Value.Trim();

        match = NotePattern4().Match(line);
        if (match.Success) return match.Groups[1].Value.Trim();

        return null;
    }

    // Source-generated regex patterns for AOT compatibility
    [GeneratedRegex(@"^##\s+(Arguments?\s+Reference|Argument\s+Reference)", RegexOptions.IgnoreCase)]
    private static partial Regex ArgumentsSectionHeader();

    [GeneratedRegex(@"^##\s+(Attributes?\s+Reference|Attribute\s+Reference)", RegexOptions.IgnoreCase)]
    private static partial Regex AttributesSectionHeader();

    [GeneratedRegex(@"^[*\-]\s*`([^`]+)`\s*[-–—]\s*(.+)")]
    private static partial Regex ArgumentDefinition();

    [GeneratedRegex(@"^\s+[*\-]\s*`([^`]+)`\s*[-–—]\s*(.+)")]
    private static partial Regex NestedArgumentDefinition();

    [GeneratedRegex(@"^(?:A|An|The)\s+`([^`]+)`\s+block\s+supports\s+the\s+following:", RegexOptions.IgnoreCase)]
    private static partial Regex BlockDefinitionHeader();

    [GeneratedRegex(@"\s*\((?:Required|Optional)\)\s*[-–—]?\s*", RegexOptions.IgnoreCase)]
    private static partial Regex RequiredOptionalTag();

    [GeneratedRegex(@"^[-–—]\s*")]
    private static partial Regex LeadingDash();

    [GeneratedRegex(@"^~>\s*|^->\s*|^>\s*")]
    private static partial Regex LeadingBlockquote();

    [GeneratedRegex(@"\*\*(.*?)\*\*")]
    private static partial Regex BoldMarkdown();

    [GeneratedRegex(@"\*(.*?)\*")]
    private static partial Regex ItalicMarkdown();

    [GeneratedRegex(@"`(.*?)`")]
    private static partial Regex CodeMarkdown();

    [GeneratedRegex(@"\[([^\]]+)\]\([^)]+\)")]
    private static partial Regex LinkMarkdown();

    [GeneratedRegex(@"^>\s*\*\*NOTE:?\*\*\s*(.*)", RegexOptions.IgnoreCase)]
    private static partial Regex NotePattern1();

    [GeneratedRegex(@"^->\s*\*\*NOTE:?\*\*\s*(.*)", RegexOptions.IgnoreCase)]
    private static partial Regex NotePattern2();

    [GeneratedRegex(@"^~>\s*\*\*NOTE:?\*\*\s*(.*)", RegexOptions.IgnoreCase)]
    private static partial Regex NotePattern3();

    [GeneratedRegex(@"^\*\*NOTE:?\*\*\s*(.*)", RegexOptions.IgnoreCase)]
    private static partial Regex NotePattern4();
}
