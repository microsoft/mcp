// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

/// <inheritdoc />
public sealed class ArtifactWriter : IArtifactWriter
{
    /// <summary>
    /// Root folder created under the caller's working directory. Each completed result is published
    /// beneath it as one UTC-stamped artifact set.
    /// </summary>
    private const string OutputFolderName = "azure-resiliency";
    private static readonly UTF8Encoding s_utf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly ILogger<ArtifactWriter> _logger;
    private readonly Func<string, Stream> _createFile;
    private readonly TimeProvider _timeProvider;

    public ArtifactWriter(ILogger<ArtifactWriter> logger)
        : this(
            logger,
            static path => new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None),
            TimeProvider.System)
    {
    }

    public ArtifactWriter(
        ILogger<ArtifactWriter> logger,
        TimeProvider timeProvider)
        : this(
            logger,
            static path => new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None),
            timeProvider)
    {
    }

    internal ArtifactWriter(
        ILogger<ArtifactWriter> logger,
        Func<string, Stream> createFile,
        TimeProvider? timeProvider = null)
    {
        _logger = logger;
        _createFile = createFile;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public IReadOnlyList<ArtifactWriteResult> Write(
        IEnumerable<AgentArtifact> artifacts,
        string conversationId)
    {
        List<(AgentArtifact Artifact, string FileName)> prepared = [];
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

            prepared.Add((artifact, fileName));
        }

        if (prepared.Count == 0)
        {
            return [];
        }

        string root = Path.Combine(Environment.CurrentDirectory, OutputFolderName);
        string family = ArtifactSetFamily(prepared.Select(item => item.Artifact));
        DateTimeOffset timestamp = _timeProvider.GetUtcNow();
        string finalDirectory = BuildFinalDirectory(root, family, timestamp);
        string relativeDirectory = Path.GetRelativePath(Environment.CurrentDirectory, finalDirectory);
        string temporaryDirectory = Path.Combine(
            root,
            $".{family}-{Guid.NewGuid():N}.tmp");

        if (prepared.Select(item => item.FileName).Distinct(StringComparer.OrdinalIgnoreCase).Count() != prepared.Count)
        {
            _logger.LogWarning(
                "Could not materialize artifact set because two artifacts resolve to the same file name.");
            return FailureResults(prepared, relativeDirectory);
        }

        try
        {
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(temporaryDirectory);
            foreach ((AgentArtifact artifact, string fileName) in prepared)
            {
                string path = Path.Combine(temporaryDirectory, fileName);
                WriteFile(path, artifact.Content!);
            }

            while (true)
            {
                try
                {
                    Directory.Move(temporaryDirectory, finalDirectory);
                    break;
                }
                catch (IOException) when (Directory.Exists(finalDirectory))
                {
                    timestamp = timestamp.AddMilliseconds(1);
                    finalDirectory = BuildFinalDirectory(root, family, timestamp);
                    relativeDirectory = Path.GetRelativePath(Environment.CurrentDirectory, finalDirectory);
                }
            }

            ArtifactWriteResult[] results = prepared
                .Select(item =>
                {
                    string path = Path.Combine(finalDirectory, item.FileName);
                    return new ArtifactWriteResult(
                        item.Artifact,
                        item.FileName,
                    new WrittenArtifact(
                        item.FileName,
                        path,
                        CreateMarkdownLink(item.FileName, path),
                        item.Artifact.Format,
                        item.Artifact.Description),
                        Path.Combine(relativeDirectory, item.FileName));
                })
                .ToArray();
            _logger.LogInformation(
                "Wrote {Count} artifact(s) to {Directory}. ConversationId: {ConversationId}",
                results.Length, finalDirectory, conversationId);
            return results;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or EncoderFallbackException)
        {
            _logger.LogWarning(ex, "Could not materialize artifact set under {Root}.", root);
            TryDeleteDirectory(temporaryDirectory);
            return FailureResults(prepared, relativeDirectory);
        }
    }

    private static string BuildFinalDirectory(
        string root,
        string family,
        DateTimeOffset timestamp)
    {
        string timestampSegment = timestamp.UtcDateTime.ToString(
            "yyyyMMdd'T'HHmmssfff'Z'",
            CultureInfo.InvariantCulture);
        return Path.Combine(root, $"{family}-{timestampSegment}");
    }

    private static ArtifactWriteResult[] FailureResults(
        IEnumerable<(AgentArtifact Artifact, string FileName)> artifacts,
        string relativeDirectory) =>
        artifacts
            .Select(item => new ArtifactWriteResult(
                item.Artifact,
                item.FileName,
                null,
                Path.Combine(relativeDirectory, item.FileName)))
            .ToArray();

    private static string ArtifactSetFamily(IEnumerable<AgentArtifact> artifacts)
    {
        AgentArtifact[] values = [.. artifacts];
        if (values.Any(artifact => string.Equals(artifact.Format, "bicep", StringComparison.OrdinalIgnoreCase)))
        {
            return "bicep";
        }

        if (values.Any(artifact => string.Equals(artifact.Format, "arm", StringComparison.OrdinalIgnoreCase)))
        {
            return "arm";
        }

        if (values.Any(artifact =>
                string.Equals(artifact.Format, "terraform", StringComparison.OrdinalIgnoreCase)
                || string.Equals(artifact.Format, "tf", StringComparison.OrdinalIgnoreCase)))
        {
            return "terraform";
        }

        string? family = values
            .Select(artifact => artifact.Family)
            .FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate));
        return SafeFolderSegment(family ?? "reports");
    }

    private static string SafeFolderSegment(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (char character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        string result = builder.ToString().Trim('-');
        return result.Length == 0 ? "artifacts" : result;
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

    private static string EnsureUsefulExtension(string fileName, AgentArtifact artifact)
    {
        string extension = Path.GetExtension(fileName);

        if (string.Equals(artifact.Format, "bicep", StringComparison.OrdinalIgnoreCase))
        {
            return ReplaceGenericExtension(fileName, extension, ".bicep");
        }

        if (string.Equals(artifact.Format, "arm", StringComparison.OrdinalIgnoreCase))
        {
            return ReplaceGenericExtension(fileName, extension, ".json");
        }

        if (string.Equals(artifact.Format, "terraform", StringComparison.OrdinalIgnoreCase))
        {
            return ReplaceGenericExtension(fileName, extension, ".tf");
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

    private void WriteFile(string path, string content)
    {
        Stream stream = _createFile(path);
        try
        {
            using (stream)
            using (var writer = new StreamWriter(stream, s_utf8NoBom))
            {
                writer.Write(content);
            }
        }
        catch
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception cleanupException) when (
                cleanupException is IOException or UnauthorizedAccessException)
            {
                // Preserve the original write failure; cleanup is best effort.
            }

            throw;
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (Exception cleanupException) when (
            cleanupException is IOException or UnauthorizedAccessException)
        {
            // Preserve the original materialization failure; cleanup is best effort.
        }
    }
}
