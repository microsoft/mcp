// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

/// <inheritdoc />
public sealed class ArtifactWriter(ILogger<ArtifactWriter> logger) : IArtifactWriter
{
    /// <summary>
    /// Folder created under the caller's working directory. Artifacts are grouped rather than written
    /// loose so a turn that produces several templates does not scatter files across the workspace.
    /// </summary>
    private const string OutputFolderName = "azure-resiliency";

    private readonly ILogger<ArtifactWriter> _logger = logger;

    public IReadOnlyList<WrittenArtifact> Write(IEnumerable<AgentArtifact> artifacts, string conversationId)
    {
        List<WrittenArtifact> written = [];
        string root = Path.Combine(Environment.CurrentDirectory, OutputFolderName);

        HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);

        foreach (AgentArtifact artifact in artifacts)
        {
            if (string.IsNullOrWhiteSpace(artifact.Content))
            {
                continue;
            }

            string? fileName = SafeFileName(artifact);
            if (fileName is null)
            {
                _logger.LogWarning("Skipped an artifact whose name could not be used as a file name.");
                continue;
            }

            fileName = MakeUniqueFileName(fileName, usedNames);

            try
            {
                Directory.CreateDirectory(root);
                string path = Path.Combine(root, fileName);
                File.WriteAllText(path, artifact.Content);
                written.Add(new WrittenArtifact(
                    fileName,
                    path,
                    CreateMarkdownLink(fileName, path),
                    artifact.Format,
                    artifact.Description));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // A conversation that produced a good answer should not fail because the workspace is
                // read-only, so the artifact is dropped from the list and the answer still returns.
                _logger.LogWarning(ex, "Could not write artifact {Name}.", fileName);
            }
        }

        if (written.Count > 0)
        {
            _logger.LogInformation(
                "Wrote {Count} artifact(s) to {Root}. ConversationId: {ConversationId}",
                written.Count, root, conversationId);
        }

        return written;
    }

    internal static string CreateMarkdownLink(string displayName, string path)
    {
        string fileUri = new Uri(Path.GetFullPath(path)).AbsoluteUri
            .Replace("(", "%28", StringComparison.Ordinal)
            .Replace(")", "%29", StringComparison.Ordinal)
            .Replace("[", "%5B", StringComparison.Ordinal)
            .Replace("]", "%5D", StringComparison.Ordinal);

        return $"[{EscapeMarkdownLinkLabel(displayName)}]({fileUri})";
    }

    private static string EscapeMarkdownLinkLabel(string value)
    {
        var builder = new System.Text.StringBuilder(value.Length);
        foreach (char character in value)
        {
            if (character is '\\' or '`' or '*' or '_' or '[' or ']' or '(' or ')' or '<' or '>' or '#' or '!' or '|')
            {
                builder.Append('\\');
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Reduces an agent-supplied artifact name to a bare file name.
    /// </summary>
    /// <remarks>
    /// The name arrives from a remote service, so it is treated as untrusted: any directory component
    /// is stripped and any character that is invalid on this platform is replaced, which prevents a
    /// name such as <c>../../.ssh/authorized_keys</c> from escaping the output folder.
    /// </remarks>
    private static string? SafeFileName(AgentArtifact artifact)
    {
        string? name = artifact.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        string trimmedName = name.Trim();
        int lastSeparator = trimmedName.LastIndexOfAny(['/', '\\']);
        string candidate = lastSeparator >= 0 ? trimmedName[(lastSeparator + 1)..] : trimmedName;
        if (string.IsNullOrWhiteSpace(candidate) || candidate is "." or "..")
        {
            return null;
        }

        candidate = NormalizeWhitespace(candidate);

        foreach (char invalid in Path.GetInvalidFileNameChars())
        {
            candidate = candidate.Replace(invalid, '_');
        }

        string cleaned = candidate.Trim('-', '.', ' ');
        if (cleaned.Length == 0)
        {
            return null;
        }

        return EnsureUsefulExtension(cleaned, artifact);
    }

    private static string NormalizeWhitespace(string value)
    {
        var builder = new System.Text.StringBuilder(value.Length);
        bool whitespaceRun = false;

        foreach (char character in value)
        {
            if (char.IsWhiteSpace(character))
            {
                if (!whitespaceRun && builder.Length > 0)
                {
                    builder.Append('-');
                }

                whitespaceRun = true;
                continue;
            }

            builder.Append(character);
            whitespaceRun = false;
        }

        return builder.ToString().Trim('-', '.', ' ');
    }

    private static string EnsureUsefulExtension(string fileName, AgentArtifact artifact)
    {
        string extension = Path.GetExtension(fileName);

        if (string.Equals(artifact.Format, "bicep", StringComparison.OrdinalIgnoreCase))
        {
            return ReplaceGenericExtension(fileName, extension, ".bicep");
        }

        if (string.Equals(artifact.MimeType, "text/markdown", StringComparison.OrdinalIgnoreCase))
        {
            return ReplaceGenericExtension(fileName, extension, ".md");
        }

        return fileName;
    }

    private static string ReplaceGenericExtension(string fileName, string extension, string usefulExtension)
    {
        if (string.Equals(extension, usefulExtension, StringComparison.OrdinalIgnoreCase))
        {
            return fileName;
        }

        if (extension.Length == 0
            || extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".json", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".md", StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFileNameWithoutExtension(fileName) + usefulExtension;
        }

        return fileName;
    }

    private static string MakeUniqueFileName(string fileName, HashSet<string> usedNames)
    {
        if (usedNames.Add(fileName))
        {
            return fileName;
        }

        string extension = Path.GetExtension(fileName);
        string stem = Path.GetFileNameWithoutExtension(fileName);

        for (int suffix = 2; ; suffix++)
        {
            string candidate = $"{stem}-{suffix}{extension}";
            if (usedNames.Add(candidate))
            {
                return candidate;
            }
        }
    }
}
