// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

// Source-generated regex patterns for AOT compatibility
internal static partial class AzureRMDocsParser
{
    [GeneratedRegex(@"^##\s+(Arguments?\s+Reference|Argument\s+Reference)", RegexOptions.IgnoreCase)]
    internal static partial Regex ArgumentsSectionHeader();

    [GeneratedRegex(@"^##\s+(Attributes?\s+Reference|Attribute\s+Reference)", RegexOptions.IgnoreCase)]
    internal static partial Regex AttributesSectionHeader();

    [GeneratedRegex(@"^[*\-]\s*`([^`]+)`\s*[-–—]\s*(.+)")]
    internal static partial Regex ArgumentDefinition();

    [GeneratedRegex(@"^\s+[*\-]\s*`([^`]+)`\s*[-–—]\s*(.+)")]
    internal static partial Regex NestedArgumentDefinition();

    [GeneratedRegex(@"^(?:A|An|The)\s+`([^`]+)`\s+block\s+supports\s+the\s+following:", RegexOptions.IgnoreCase)]
    internal static partial Regex BlockDefinitionHeader();

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
