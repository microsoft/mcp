// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Services.Host;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Services.Host;

public sealed class HostServicesTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LocalHostServices_ReadFilesAndEnvironmentVariables(bool registerRuntime)
    {
        var services = new ServiceCollection();
        services.InitializeConfigurationAndOptions(typeof(HostServicesTests).Assembly);
        if (registerRuntime)
        {
            services.AddAzureMcpServer(new() { Transport = TransportTypes.StdIo });
        }
        using var provider = services.BuildServiceProvider();
        var fileSystem = provider.GetRequiredService<IFileSystem>();
        var environment = provider.GetRequiredService<IEnvironmentVariables>();
        var directory = Path.Combine(fileSystem.GetTempPath(), $"mcp-host-{Guid.NewGuid():N}");
        var path = Path.Combine(directory, "input.txt");
        var variable = $"MCP_HOST_TEST_{Guid.NewGuid():N}";
        try
        {
            fileSystem.CreateDirectory(directory);
            var bytes = Encoding.UTF8.GetBytes("local content");
            await fileSystem.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);
            Assert.True(fileSystem.FileExists(path));
            Assert.Equal("local content", fileSystem.ReadAllText(path));
            Assert.Equal(bytes, await fileSystem.ReadAllBytesAsync(path, TestContext.Current.CancellationToken));
            Assert.Equal(bytes.Length, fileSystem.GetFileInfo(path).Length);

            Environment.SetEnvironmentVariable(variable, "local value");
            Assert.Equal("local value", environment.GetEnvironmentVariable(variable));
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, null);
            fileSystem.DeleteDirectory(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData("EnsureAccess")]
    [InlineData("FileExists")]
    [InlineData("DirectoryExists")]
    [InlineData("ReadAllText")]
    [InlineData("OpenRead")]
    [InlineData("Open")]
    [InlineData("OpenHandle")]
    [InlineData("GetFileInfo")]
    [InlineData("GetDriveInfo")]
    [InlineData("GetAttributes")]
    [InlineData("GetLastWriteTimeUtc")]
    [InlineData("DeleteFile")]
    [InlineData("CreateDirectory")]
    [InlineData("DeleteDirectory")]
    [InlineData("MoveDirectory")]
    [InlineData("GetFiles")]
    [InlineData("EnumerateFiles")]
    [InlineData("GetCurrentDirectory")]
    [InlineData("GetTempPath")]
    public void HttpFileSystem_RejectsOperationsBeforeAccessingHost(string operation)
    {
        using var provider = CreateHttpProvider();
        var fileSystem = provider.GetRequiredService<IFileSystem>();
        const string path = "\0";
        Action action = operation switch
        {
            "EnsureAccess" => fileSystem.EnsureAccess,
            "FileExists" => () => fileSystem.FileExists(path),
            "DirectoryExists" => () => fileSystem.DirectoryExists(path),
            "ReadAllText" => () => fileSystem.ReadAllText(path),
            "OpenRead" => () => fileSystem.OpenRead(path).Dispose(),
            "Open" => () => fileSystem.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None).Dispose(),
            "OpenHandle" => () => fileSystem.OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.None).Dispose(),
            "GetFileInfo" => () => fileSystem.GetFileInfo(path),
            "GetDriveInfo" => () => fileSystem.GetDriveInfo(path),
            "GetAttributes" => () => fileSystem.GetAttributes(path),
            "GetLastWriteTimeUtc" => () => fileSystem.GetLastWriteTimeUtc(path),
            "DeleteFile" => () => fileSystem.DeleteFile(path),
            "CreateDirectory" => () => fileSystem.CreateDirectory(path),
            "DeleteDirectory" => () => fileSystem.DeleteDirectory(path, recursive: true),
            "MoveDirectory" => () => fileSystem.MoveDirectory(path, path),
            "GetFiles" => () => fileSystem.GetFiles(path),
            "EnumerateFiles" => () => fileSystem.EnumerateFiles(path, "*", SearchOption.AllDirectories),
            "GetCurrentDirectory" => () => fileSystem.GetCurrentDirectory(),
            "GetTempPath" => () => fileSystem.GetTempPath(),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
        var exception = Assert.Throws<UnauthorizedAccessException>(action);
        Assert.Contains("HTTP", exception.Message);
    }

    [Fact]
    public async Task HttpFileSystem_RejectsAsyncReadsAndWrites()
    {
        using var provider = CreateHttpProvider();
        var fileSystem = provider.GetRequiredService<IFileSystem>();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fileSystem.ReadAllBytesAsync("\0", TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fileSystem.WriteAllBytesAsync("\0", [], TestContext.Current.CancellationToken));
    }

    [Fact]
    public void HttpEnvironment_RejectsVariableReadsAndProfileLookup()
    {
        using var provider = CreateHttpProvider();
        var environment = provider.GetRequiredService<IEnvironmentVariables>();
        Assert.Throws<UnauthorizedAccessException>(() => environment.GetEnvironmentVariable("\0"));
        Assert.Throws<UnauthorizedAccessException>(() => environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }

    private static ServiceProvider CreateHttpProvider()
    {
        var services = new ServiceCollection();
        services.InitializeConfigurationAndOptions(typeof(HostServicesTests).Assembly);
        services.AddAzureMcpServer(new() { Transport = TransportTypes.Http });
        return services.BuildServiceProvider();
    }
}
