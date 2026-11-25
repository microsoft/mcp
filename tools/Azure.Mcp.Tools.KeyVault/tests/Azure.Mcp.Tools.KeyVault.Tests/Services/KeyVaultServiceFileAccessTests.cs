// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.KeyVault.Services;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Services.Host;
using NSubstitute;
using Xunit;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Azure.Mcp.Tools.KeyVault.Tests.Services;

public sealed class KeyVaultServiceFileAccessTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HttpMode_InlineCertificateDoesNotAccessFiles(bool pem)
    {
        byte[] expected = pem ? Encoding.UTF8.GetBytes("-----BEGIN CERTIFICATE-----") : [1, 2, 3];
        var input = pem ? "-----BEGIN CERTIFICATE-----" : Convert.ToBase64String(expected);
        var fileSystem = new FileSystem(OptionsFactory.Create(new ServerRuntimeConfiguration { Transport = "http" }));
        var service = new KeyVaultService(Substitute.For<IAzureService>(), fileSystem);

        var bytes = await service.ReadCertificateDataAsync(input, TestContext.Current.CancellationToken);

        Assert.Equal(expected, bytes);
    }

    [Fact]
    public async Task LocalMode_Base64CompatibleFileName_TakesPrecedenceOverInlineData()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem.IsAccessAllowed.Returns(true);
        fileSystem.FileExists("cert").Returns(true);
        byte[] expected = [1, 2, 3];
        fileSystem.ReadAllBytesAsync("cert", Arg.Any<CancellationToken>()).Returns(expected);
        var service = new KeyVaultService(Substitute.For<IAzureService>(), fileSystem);

        var bytes = await service.ReadCertificateDataAsync("cert", TestContext.Current.CancellationToken);

        Assert.Equal(expected, bytes);
    }

    [Fact]
    public async Task HttpMode_CertificateFileIsDenied()
    {
        var fileSystem = new FileSystem(OptionsFactory.Create(new ServerRuntimeConfiguration { Transport = "http" }));
        var service = new KeyVaultService(Substitute.For<IAzureService>(), fileSystem);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ReadCertificateDataAsync("certificate.pfx", TestContext.Current.CancellationToken));
    }
}
