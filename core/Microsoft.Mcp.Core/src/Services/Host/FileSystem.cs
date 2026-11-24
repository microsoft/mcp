// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Mcp.Core.Services.Host;

/// <inheritdoc />
public sealed class FileSystem(IOptions<ServerRuntimeConfiguration> configuration) : IFileSystem
{
    public void EnsureAccess()
    {
        if (configuration.Value.IsHttpMode)
        {
            throw new UnauthorizedAccessException("Tool access to the host filesystem is not allowed in HTTP mode.");
        }
    }

    public bool FileExists(string? path)
    {
        EnsureAccess();
        return File.Exists(path);
    }

    public bool DirectoryExists(string? path)
    {
        EnsureAccess();
        return Directory.Exists(path);
    }

    public string ReadAllText(string path)
    {
        EnsureAccess();
        return File.ReadAllText(path);
    }

    public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        EnsureAccess();
        return File.ReadAllBytesAsync(path, cancellationToken);
    }

    public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        EnsureAccess();
        return File.WriteAllBytesAsync(path, bytes, cancellationToken);
    }

    public FileStream OpenRead(string path)
    {
        EnsureAccess();
        return File.OpenRead(path);
    }

    public FileStream Open(string path, FileMode mode, FileAccess access, FileShare share,
        int bufferSize = 4096, bool useAsync = false)
    {
        EnsureAccess();
        return new FileStream(path, mode, access, share, bufferSize, useAsync);
    }

    public SafeFileHandle OpenHandle(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options)
    {
        EnsureAccess();
        return File.OpenHandle(path, mode, access, share, options);
    }

    public FileInfo GetFileInfo(string path)
    {
        EnsureAccess();
        return new FileInfo(path);
    }

    public DriveInfo GetDriveInfo(string path)
    {
        EnsureAccess();
        return new DriveInfo(path);
    }

    public FileAttributes GetAttributes(string path)
    {
        EnsureAccess();
        return File.GetAttributes(path);
    }

    public DateTime GetLastWriteTimeUtc(string path)
    {
        EnsureAccess();
        return File.GetLastWriteTimeUtc(path);
    }

    public void DeleteFile(string path)
    {
        EnsureAccess();
        File.Delete(path);
    }

    public DirectoryInfo CreateDirectory(string path)
    {
        EnsureAccess();
        return Directory.CreateDirectory(path);
    }

    public void DeleteDirectory(string path, bool recursive)
    {
        EnsureAccess();
        Directory.Delete(path, recursive);
    }

    public void MoveDirectory(string source, string destination)
    {
        EnsureAccess();
        Directory.Move(source, destination);
    }

    public string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        EnsureAccess();
        return Directory.GetFiles(path, searchPattern, searchOption);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        EnsureAccess();
        return Directory.EnumerateFiles(path, searchPattern, searchOption);
    }

    public string GetCurrentDirectory()
    {
        EnsureAccess();
        return Directory.GetCurrentDirectory();
    }

    public string GetTempPath()
    {
        EnsureAccess();
        return Path.GetTempPath();
    }
}
