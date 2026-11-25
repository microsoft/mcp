// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Win32.SafeHandles;

namespace Microsoft.Mcp.Core.Services.Host;

/// <summary>
/// Tool-time access to the host filesystem. Host reads and writes are denied in HTTP mode.
/// Process startup and embedded assembly resources do not use this service.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Gets whether tool-time access to the host filesystem is permitted.
    /// Checking this property does not access the filesystem.
    /// </summary>
    bool IsAccessAllowed { get; }

    /// <summary>
    /// Rejects filesystem access when the server is running in HTTP mode.
    /// </summary>
    void EnsureAccess();

    /// <summary>
    /// Determines whether a file exists at the specified path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><see langword="true"/> if the file exists; otherwise, <see langword="false"/>.</returns>
    bool FileExists(string? path);

    /// <summary>
    /// Determines whether a directory exists at the specified path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><see langword="true"/> if the directory exists; otherwise, <see langword="false"/>.</returns>
    bool DirectoryExists(string? path);

    /// <summary>
    /// Reads all text from the specified file.
    /// </summary>
    /// <param name="path">The file to read.</param>
    /// <returns>The file contents.</returns>
    string ReadAllText(string path);

    /// <summary>
    /// Asynchronously reads all bytes from the specified file.
    /// </summary>
    /// <param name="path">The file to read.</param>
    /// <param name="cancellationToken">A token that can cancel the read.</param>
    /// <returns>The file contents.</returns>
    Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously writes bytes to the specified file, replacing its contents.
    /// </summary>
    /// <param name="path">The file to write.</param>
    /// <param name="bytes">The contents to write.</param>
    /// <param name="cancellationToken">A token that can cancel the write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the specified file for reading.
    /// </summary>
    /// <param name="path">The file to open.</param>
    /// <returns>A read-only stream for the file.</returns>
    FileStream OpenRead(string path);

    /// <summary>
    /// Opens a file stream with the specified access and sharing options.
    /// </summary>
    /// <param name="path">The file to open.</param>
    /// <param name="mode">How the operating system should open or create the file.</param>
    /// <param name="access">The access to the file.</param>
    /// <param name="share">The sharing permission for other operations on the file.</param>
    /// <param name="bufferSize">The buffer size, in bytes.</param>
    /// <param name="useAsync"><see langword="true"/> to enable asynchronous I/O.</param>
    /// <returns>A stream for the file.</returns>
    FileStream Open(string path, FileMode mode, FileAccess access, FileShare share,
        int bufferSize = 4096, bool useAsync = false);

    /// <summary>
    /// Opens the specified file and returns its operating-system handle.
    /// </summary>
    /// <param name="path">The file to open.</param>
    /// <param name="mode">How the operating system should open or create the file.</param>
    /// <param name="access">The access to the file.</param>
    /// <param name="share">The sharing permission for other operations on the file.</param>
    /// <param name="options">File-opening options.</param>
    /// <returns>A safe handle for the opened file.</returns>
    SafeFileHandle OpenHandle(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options);

    /// <summary>
    /// Creates a <see cref="FileInfo"/> for the specified path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>Information about the file.</returns>
    FileInfo GetFileInfo(string path);

    /// <summary>
    /// Creates a <see cref="DriveInfo"/> for the specified path.
    /// </summary>
    /// <param name="path">A path on the drive.</param>
    /// <returns>Information about the drive containing the path.</returns>
    DriveInfo GetDriveInfo(string path);

    /// <summary>
    /// Gets the attributes of the specified file or directory.
    /// </summary>
    /// <param name="path">The file or directory path.</param>
    /// <returns>The attributes of the specified path.</returns>
    FileAttributes GetAttributes(string path);

    /// <summary>
    /// Gets the last-write time, in UTC, of the specified file or directory.
    /// </summary>
    /// <param name="path">The file or directory path.</param>
    /// <returns>The last-write time in UTC.</returns>
    DateTime GetLastWriteTimeUtc(string path);

    /// <summary>
    /// Deletes the specified file.
    /// </summary>
    /// <param name="path">The file to delete.</param>
    void DeleteFile(string path);

    /// <summary>
    /// Creates the specified directory and any missing parent directories.
    /// </summary>
    /// <param name="path">The directory to create.</param>
    /// <returns>Information about the created directory.</returns>
    DirectoryInfo CreateDirectory(string path);

    /// <summary>
    /// Deletes the specified directory.
    /// </summary>
    /// <param name="path">The directory to delete.</param>
    /// <param name="recursive"><see langword="true"/> to delete the directory and its contents.</param>
    void DeleteDirectory(string path, bool recursive);

    /// <summary>
    /// Moves a directory and its contents to a new path.
    /// </summary>
    /// <param name="source">The directory to move.</param>
    /// <param name="destination">The destination path.</param>
    void MoveDirectory(string source, string destination);

    /// <summary>
    /// Gets the paths of files in a directory that match a search pattern.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <param name="searchPattern">The search pattern to match against file names.</param>
    /// <param name="searchOption">Whether to search only the directory or all subdirectories.</param>
    /// <returns>The paths of matching files.</returns>
    string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);

    /// <summary>
    /// Enumerates the paths of files in a directory that match a search pattern.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <param name="searchPattern">The search pattern to match against file names.</param>
    /// <param name="searchOption">Whether to search only the directory or all subdirectories.</param>
    /// <returns>An enumerable collection of matching file paths.</returns>
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);

    /// <summary>
    /// Gets the current working directory.
    /// </summary>
    /// <returns>The current working directory path.</returns>
    string GetCurrentDirectory();

    /// <summary>
    /// Gets the path of the system temporary directory.
    /// </summary>
    /// <returns>The system temporary directory path.</returns>
    string GetTempPath();
}
