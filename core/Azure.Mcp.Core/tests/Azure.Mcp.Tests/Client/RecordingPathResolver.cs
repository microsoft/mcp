// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text;

namespace Azure.Mcp.Tests.Client;

/// <summary>
/// Provides path resolution for session records and related assets.
/// </summary>
internal sealed class RecordingPathResolver
{
    private static readonly char[] _invalidChars = ['\\', '/', ':', '*', '?', '"', '<', '>', '|'];

    private readonly string _repoRoot;

    public RecordingPathResolver()
    {
        _repoRoot = ResolveRepositoryRoot() ?? Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Attempt to locate the repository root by walking up until a .git directory/file or global.json is found.
    /// </summary>
    private static string? ResolveRepositoryRoot()
    {
        var dir = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent;
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")) ||
                File.Exists(Path.Combine(dir.FullName, ".git")) ||
                File.Exists(Path.Combine(dir.FullName, "global.json")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Unable to locate repository root. Ensure tests are running in a cloned repository.");
    }

    public string RepositoryRoot => _repoRoot;

    /// <summary>
    /// Sanitizes a test display/name into a file-system friendly component.
    /// </summary>
    public static string Sanitize(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "(unknown)";
        Span<char> buffer = stackalloc char[name.Length];
        int i = 0;
        foreach (var c in name)
        {
            buffer[i++] = _invalidChars.Contains(c) ? '_' : c;
        }
        return new string(buffer);
    }

    /// <summary>
    /// Builds the session directory path: <relative path to test project>/SessionRecords/<TestClassName or variant>
    /// Example: tools/Azure.Mcp.Tools.KeyVault/tests/Azure.Mcp.Tools.KeyVault.LiveTests/SessionRecords/RecordedKeyVaultCommandTests
    /// </summary>
    public string GetSessionDirectory(Type testType, string? variantSuffix = null)
    {
        // Locate the test project directory by ascending from the assembly location until a matching *.csproj exists.
        var projectDir = GetProjectDirectory(testType);

        // Compute relative path from repo root.
        var relativeProjectPath = Path.GetRelativePath(_repoRoot, projectDir)
            .Replace('\\', '/'); // Normalize separators for consistency.

        // Append SessionRecords and suffix.
        var sessionDir = Path.Combine(relativeProjectPath, "SessionRecords")
            .Replace('\\', '/');

        // TODO: Consider caching projectDir per assembly for performance if needed.
        return sessionDir;
    }

    private static string GetProjectDirectory(Type testType)
    {
        // Locate the test project directory by ascending from the assembly location until a matching *.csproj exists.
        var assemblyDir = Path.GetDirectoryName(testType.Assembly.Location)!;
        var projectDir = FindProjectDirectory(assemblyDir, testType);

        return projectDir;
    }

    private static string FindProjectDirectory(string startDirectory, Type testType)
    {
        var current = new DirectoryInfo(startDirectory);
        var expectedProjectName = testType.Assembly.GetName().Name; // Typically matches .csproj file name.

        while (current != null)
        {
            // Look for any .csproj; prefer one matching assembly name.
            var csprojFiles = current.GetFiles("*.csproj", SearchOption.TopDirectoryOnly);
            if (csprojFiles.Length > 0)
            {
                var matching = csprojFiles.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f.Name) == expectedProjectName);
                return (matching ?? csprojFiles.First()).Directory!.FullName;
            }
            current = current.Parent;
        }

        throw new InvalidOperationException($"Unable to locate project directory for test type {testType.FullName} starting from {startDirectory}.");
    }

    /// <summary>
    /// Builds a deterministic file name from sanitized test name.
    /// TODO: Add version qualifier / async suffix when those concepts are introduced.
    /// </summary>
    public static string BuildFileName(string sanitizedDisplayName, bool isAsync, string? versionQualifier = null)
    {
        var versionPart = string.IsNullOrWhiteSpace(versionQualifier) ? string.Empty : $"[{versionQualifier}]"; // TODO: provide real version qualifier
        var asyncPart = isAsync ? "Async" : string.Empty; // TODO: This is literally looking at the test name. Probably not good enough.
        return $"{sanitizedDisplayName}{versionPart}{asyncPart}.json";
    }

    /// <summary>
    /// Attempts to find a nearest assets.json walking upwards.
    /// </summary>
    public string? GetAssetsJson(Type testType)
    {
        var projectDir = GetProjectDirectory(testType);

        var current = new DirectoryInfo(projectDir);

        var assetsFile = Path.Combine(current.FullName, "assets.json");

        if (File.Exists(assetsFile))
        {
            return assetsFile;
        }

        return null;
    }
}
