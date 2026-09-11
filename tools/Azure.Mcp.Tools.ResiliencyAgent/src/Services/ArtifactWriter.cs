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

        // Environment.CurrentDirectory is the directory the MCP client launched this server in, which
        // for an editor is the user's workspace. The same assumption the Azure Migrate tool makes.
        string root = Path.Combine(Environment.CurrentDirectory, OutputFolderName);

        foreach (AgentArtifact artifact in artifacts)
        {
            if (string.IsNullOrWhiteSpace(artifact.Content))
            {
                continue;
            }

            string? fileName = SafeFileName(artifact.Name);
            if (fileName is null)
            {
                _logger.LogWarning("Skipped an artifact whose name could not be used as a file name.");
                continue;
            }

            try
            {
                Directory.CreateDirectory(root);
                string path = Path.Combine(root, fileName);
                File.WriteAllText(path, artifact.Content);
                written.Add(new WrittenArtifact(fileName, path, artifact.Format, artifact.Description));
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

    /// <summary>
    /// Reduces an agent-supplied artifact name to a bare file name.
    /// </summary>
    /// <remarks>
    /// The name arrives from a remote service, so it is treated as untrusted: any directory component
    /// is stripped and any character that is invalid on this platform is replaced, which prevents a
    /// name such as <c>../../.ssh/authorized_keys</c> from escaping the output folder.
    /// </remarks>
    private static string? SafeFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        string candidate = Path.GetFileName(name.Trim());
        if (string.IsNullOrWhiteSpace(candidate) || candidate is "." or "..")
        {
            return null;
        }

        foreach (char invalid in Path.GetInvalidFileNameChars())
        {
            candidate = candidate.Replace(invalid, '_');
        }

        // Guard against a name that is only separators or dots once the above has run.
        return candidate.Trim('.', ' ') is { Length: > 0 } cleaned ? cleaned : null;
    }
}
