// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;

namespace Microsoft.Mcp.Tests.Client.Helpers;

/// <summary>
/// Provides path resolution for session records and related assets.
/// </summary>
public sealed class RecordingPathResolver : IRecordingPathResolver
{
    private static readonly char[] s_invalidChars = ['\\', '/', ':', '*', '?', '"', '<', '>', '|'];

    public string RepositoryRoot { get; } = ResolveRepositoryRoot() ?? Directory.GetCurrentDirectory();

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

    /// <summary>
    /// Sanitizes a test display/name into a file-system friendly component.
    /// </summary>
    public static string Sanitize(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "(unknown)";
        Span<char> buffer = stackalloc char[name.Length];
        int i = 0;
        foreach (var c in name)
        {
            buffer[i++] = s_invalidChars.Contains(c) ? '_' : c;
        }
        return new string(buffer);
    }

    /// <summary>
    /// Builds the session directory path: <relative path to test project>/SessionRecords/<TestClassName or variant>
    /// Example: tools/Azure.Mcp.Tools.KeyVault/tests/Azure.Mcp.Tools.KeyVault.Tests/SessionRecords/RecordedKeyVaultCommandTests
    /// </summary>
    public string GetSessionDirectory(Type testType, string? variantSuffix = null)
    {
        // Locate the test project directory by ascending from the assembly location until a matching *.csproj exists.
        var projectDir = GetSourcePath(testType.Assembly);

        // Compute relative path from repo root.
        var relativeProjectPath = Path.GetRelativePath(RepositoryRoot, projectDir)
            .Replace('\\', '/'); // Normalize separators for consistency.

        // Append SessionRecords and suffix.
        var sessionDir = Path.Combine(relativeProjectPath, "SessionRecords")
            .Replace('\\', '/');

        // TODO: Consider caching projectDir per assembly for performance if needed.
        return sessionDir;
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
    /// Generates a clear message for missing assets.json file to assist users in creating one when they hit the error.
    /// </summary>
    private static string BuildMissingAssetsErrorMessage(string testClass, string projectDir)
    {
        string projectDirName = new DirectoryInfo(projectDir).Name;

        string emptyAssets = $@"{{
    ""AssetsRepo"": ""Azure/azure-sdk-assets"",
    ""AssetsRepoPrefixPath"": """",
    ""TagPrefix"": ""{projectDirName}"",
    ""Tag"": """"
}}";

        return $"Unable to locate assets.json for test type {testClass}. Create a file named \"assets.json\" within {projectDir} directory with content of {Environment.NewLine}{emptyAssets}";
    }

    /// <summary>
    /// Get the assets.json file for the test type using the assembly's SourcePath metadata.
    /// <para>
    /// This method uses the <see cref="AssemblyMetadataAttribute"/> with the key "SourcePath" to determine the
    /// location of the test project and then looks for an "assets.json" file in that directory.
    /// </para>
    /// <param name="testType">The type of the test for which to find the assets.json file.</param>
    /// </summary>
    public string GetAssetsJson(Type testType)
    {
        var projectDir = GetSourcePath(testType.Assembly);

        var current = new DirectoryInfo(projectDir);

        var assetsFile = Path.Combine(current.FullName, "assets.json");

        if (File.Exists(assetsFile))
        {
            return assetsFile;
        }
        throw new FileNotFoundException(BuildMissingAssetsErrorMessage(testType.FullName ?? "UnknownTestClass", projectDir));
    }

    private static string GetSourcePath(Assembly assembly)
    {
        var sourcePaths = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Where(a => a.Key == "SourcePath");
        if (sourcePaths.Count() != 1)
        {
            throw new InvalidOperationException("No SourcePath metadata found. Ensure the test project is properly " +
                "configured with '<IsTestProject>true</IsTestProject>' in the .csproj file, uses 'Directory.Build.targets', " +
                "and has a single 'SourcePath' metadata.");
        }
        return sourcePaths.First().Value!;
    }
}
