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
        _repoRoot = ResolveRepositoryRoot() ?? Directory.GetCurrentDirectory(); // TODO: fallback strategy refinement
    }

    /// <summary>
    /// Attempt to locate the repository root by walking up until a .git directory/file or global.json is found.
    /// </summary>
    private static string? ResolveRepositoryRoot()
    {
        try
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
        }
        catch { }
        return null; // TODO: consider throwing if not found
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
    /// Builds the session directory path: <repoRoot>/SessionRecords/<TestClassName>
    /// TODO: Align with previous Azure.Core layout if/when needed.
    /// </summary>
    public string GetSessionDirectory(Type testType, string? variantSuffix = null)
    {
        var className = testType.Name;
        var suffix = string.IsNullOrWhiteSpace(variantSuffix) ? className : $"{className}({variantSuffix})";

        // todo: clarify this. We don't want to pass an absolute path, this should be a relative path within the repo.
        // right now it's just the test class name + test name + optional suffix. Need to add some structure reflecting
        // the path to the test project within the repo.
        return Path.Combine("SessionRecords", suffix);
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
    public static string? TryLocateAssetsJson(string startDirectory)
    {
        // todo: implement this based on what we have when running a test.
        return null;
    }
}
