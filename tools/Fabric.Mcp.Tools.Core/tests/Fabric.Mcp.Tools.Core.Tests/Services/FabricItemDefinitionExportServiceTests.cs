// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FabricItemDefinitionExportServiceTests()
{
    private readonly IFabricItemDefinitionFileService _files = Substitute.For<IFabricItemDefinitionFileService>();
    private CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(TransportTypes.StdIo)]
    [InlineData(TransportTypes.Http)]
    public void NoDestination_NeverAccessesFiles(string transport)
    {
        Assert.Null(CreateService(transport).ValidateDestination(null));
        Assert.Empty(_files.ReceivedCalls());
    }

    [Theory]
    [InlineData(TransportTypes.Http)]
    [InlineData("")]
    [InlineData("HTTP")]
    [InlineData("future-transport")]
    public async Task Export_RequiresLocalCapabilityBeforePathInspection(string transport)
    {
        var service = CreateService(transport);
        const string path = @"\\private-host\share";

        var validation = Assert.Throws<FabricItemDefinitionExportException>(() => service.ValidateDestination(path));
        var write = await Assert.ThrowsAsync<FabricItemDefinitionExportException>(() =>
            service.WriteAsync(path, null!, CancellationToken));

        Assert.False(service.SupportsLocalFiles);
        Assert.Contains("unavailable on this host", validation.Message);
        Assert.Equal(validation.Message, write.Message);
        Assert.DoesNotContain("private-host", validation.Message);
        Assert.Empty(_files.ReceivedCalls());
    }

    [Fact]
    public async Task Capability_IsRecheckedAtWriteEvenAfterLocalValidation()
    {
        using var root = new FabricDefinitionTestDirectory();
        var destination = Path.Combine(root.Path, "output");
        var runtime = new ServerRuntimeConfiguration { Transport = TransportTypes.StdIo };
        var service = new FabricItemDefinitionExportService(OptionsFactory.Create(runtime), _files);
        Assert.Equal(destination, service.ValidateDestination(destination));
        runtime.Transport = TransportTypes.Http;

        await Assert.ThrowsAsync<FabricItemDefinitionExportException>(() =>
            service.WriteAsync(destination, ItemDefinitionTestData.Definition, CancellationToken));

        Assert.Empty(_files.ReceivedCalls());
        Assert.Empty(Directory.EnumerateFileSystemEntries(root.Path));
    }

    [Fact]
    public async Task Export_ForwardsCompletedDefinitionAndCancellationWithoutChangingParts()
    {
        using var root = new FabricDefinitionTestDirectory();
        var destination = Path.Combine(root.Path, "output");
        var definition = ItemDefinitionTestData.Definition;
        _files.WriteAsync(destination, definition, CancellationToken).Returns(destination);

        var result = await CreateService().WriteAsync(destination, definition, CancellationToken);

        Assert.Equal(destination, result);
        await _files.Received(1).WriteAsync(destination, definition, CancellationToken);
    }

    [Theory]
    [InlineData("argument")]
    [InlineData("io")]
    [InlineData("access")]
    [InlineData("unsupported")]
    public async Task FileFailures_AreExplicitAndSanitized(string failure)
    {
        using var root = new FabricDefinitionTestDirectory();
        var destination = Path.Combine(root.Path, "output");
        Exception exception = failure switch
        {
            "argument" => new ArgumentException("private-path"),
            "io" => new IOException("private-file-contents"),
            "access" => new UnauthorizedAccessException("private-identity"),
            _ => new NotSupportedException("private-volume")
        };
        _files.WriteAsync(Arg.Any<string>(), Arg.Any<FabricItemDefinition>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        var error = await Assert.ThrowsAsync<FabricItemDefinitionExportException>(() =>
            CreateService().WriteAsync(destination, ItemDefinitionTestData.Definition, CancellationToken));

        Assert.Equal(HttpStatusCode.InternalServerError, error.StatusCode);
        Assert.Contains("local export failed", error.Message);
        Assert.DoesNotContain("private", error.Message);
        Assert.Null(error.InnerException);
    }

    [Fact]
    public async Task Cancellation_PropagatesWithoutBeingWrappedAsFileFailure()
    {
        using var root = new FabricDefinitionTestDirectory();
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
        await caller.CancelAsync();

        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateService().WriteAsync(Path.Combine(root.Path, "output"), ItemDefinitionTestData.Definition, caller.Token));

        Assert.Equal(caller.Token, error.CancellationToken);
        Assert.Empty(_files.ReceivedCalls());
    }

    private FabricItemDefinitionExportService CreateService(string transport = TransportTypes.StdIo) =>
        new(OptionsFactory.Create(new ServerRuntimeConfiguration { Transport = transport }), _files);
}
