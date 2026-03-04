// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Areas.Server.Commands;

/// <summary>
/// Regression tests for content root configuration in host creation.
/// Verifies that the content root is set to AppContext.BaseDirectory instead of the current
/// working directory to prevent inotify watcher exhaustion on Linux.
/// See https://github.com/microsoft/mcp/issues/1930
/// </summary>
public sealed class ServiceStartCommandContentRootTests : IDisposable
{
    private readonly Action<IServiceCollection> _originalConfigureServices;

    public ServiceStartCommandContentRootTests()
    {
        // Save and replace the static ConfigureServices callback to avoid
        // pulling in the full Azure MCP Server service registrations.
        _originalConfigureServices = ServiceStartCommand.ConfigureServices;
        ServiceStartCommand.ConfigureServices = _ => { };
    }

    public void Dispose()
    {
        ServiceStartCommand.ConfigureServices = _originalConfigureServices;
    }

    [Fact]
    public void CreateHost_StdioTransport_ContentRootIsAppBaseDirectory()
    {
        // Arrange
        var command = new ServiceStartCommand();
        var options = new ServiceStartOptions
        {
            Transport = "stdio",
            Mode = "all",
        };

        // Act
        using var host = command.CreateHost(options);
        var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();

        // Assert — content root must be AppContext.BaseDirectory, not the current working directory.
        // When CWD is a large workspace, defaulting to it causes .NET's PhysicalFileProvider to
        // create a recursive FileSystemWatcher that exhausts inotify watchers on Linux.
        var expected = NormalizePath(AppContext.BaseDirectory);
        var actual = NormalizePath(hostEnvironment.ContentRootPath);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CreateHost_StdioTransport_ContentRootIsNotCurrentDirectory()
    {
        // Arrange — ensure CWD differs from AppContext.BaseDirectory
        var originalDir = Directory.GetCurrentDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            Directory.SetCurrentDirectory(tempDir);

            var command = new ServiceStartCommand();
            var options = new ServiceStartOptions
            {
                Transport = "stdio",
                Mode = "all",
            };

            // Act
            using var host = command.CreateHost(options);
            var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();

            // Assert — content root must NOT follow CWD
            var contentRoot = NormalizePath(hostEnvironment.ContentRootPath);
            var cwd = NormalizePath(tempDir);
            Assert.NotEqual(cwd, contentRoot);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            try { Directory.Delete(tempDir, recursive: true); } catch { /* best-effort cleanup */ }
        }
    }

    private static string NormalizePath(string path) =>
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
}
