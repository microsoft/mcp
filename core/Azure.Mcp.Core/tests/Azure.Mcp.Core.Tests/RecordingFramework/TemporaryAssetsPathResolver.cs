// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Microsoft.Mcp.Tests.Client.Helpers;

namespace Azure.Mcp.Core.Tests.RecordingFramework;

internal sealed class TemporaryAssetsPathResolver() : IRecordingPathResolver, IDisposable
{
    private readonly RecordingPathResolver _inner = new();
    private readonly string _repositoryRoot = CreateRepositoryRoot();
    private bool _disposed;

    public string RepositoryRoot => _repositoryRoot;

    public string GetSessionDirectory(Type testType, string? variantSuffix = null)
        => _inner.GetSessionDirectory(testType, variantSuffix);

    public string GetAssetsJson(Type testType)
    {
        var directory = Path.Combine(_repositoryRoot, "tools", "fake-tool");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "assets.json");
        if (!File.Exists(path))
        {
            var tagPrefix = testType.Assembly.GetName().Name ?? testType.Name;
            File.WriteAllText(path, $$"""
                {
                    "AssetsRepo": "Azure/azure-sdk-assets",
                    "AssetsRepoPrefixPath": "",
                    "TagPrefix": "{{tagPrefix}}",
                    "Tag": ""
                }
                """, Encoding.UTF8);
        }
        return path;
    }

    private static string CreateRepositoryRoot()
    {
        var directory = Directory.CreateTempSubdirectory("mcp-recordings-harness-tests-").FullName;
        using (File.Create(Path.Combine(directory, ".git")))
        {
        }
        return directory;
    }

    private static void DeleteDirectory(string directory)
    {
        File.SetAttributes(directory, FileAttributes.Normal);
        foreach (var file in Directory.GetFiles(directory))
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }
        foreach (var child in Directory.GetDirectories(directory))
        {
            DeleteDirectory(child);
        }
        Directory.Delete(directory);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (Directory.Exists(_repositoryRoot))
            {
                DeleteDirectory(_repositoryRoot);
            }
            _disposed = true;
        }
    }
}
