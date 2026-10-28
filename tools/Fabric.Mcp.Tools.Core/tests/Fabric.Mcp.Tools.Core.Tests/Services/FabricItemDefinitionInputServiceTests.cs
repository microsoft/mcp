// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words ipynb tmdl tmsl pbism pbir

using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FabricItemDefinitionInputServiceTests()
{
    private readonly IFabricItemDefinitionFileService _files = Substitute.For<IFabricItemDefinitionFileService>();
    private CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(TransportTypes.StdIo)]
    [InlineData(TransportTypes.Http)]
    public async Task NoSource_PreservesCreationWithoutDefinitionOrFileAccess(string transport)
    {
        var result = await ReadAsync(CreateService(transport));

        Assert.Null(result);
        Assert.Empty(_files.ReceivedCalls());
    }

    [Theory]
    [InlineData(TransportTypes.StdIo)]
    [InlineData(TransportTypes.Http)]
    public async Task Inline_PreservesFormatPartsEmptyAndBinaryPayloadsWithoutFileAccess(string transport)
    {
        var definition = new FabricItemDefinition([Part("data", [0, 1, 255]), Part("empty", [])], "FutureFormat");

        var result = await ReadAsync(CreateService(transport), Json(definition));

        Assert.NotNull(result);
        Assert.Equal(definition.Format, result.Format);
        Assert.Equal(definition.Parts, result.Parts);
        Assert.Empty(_files.ReceivedCalls());
    }

    [Fact]
    public async Task Local_ForwardsOnlyExplicitMappingsFormatAndCancellation()
    {
        var expected = new FabricItemDefinition([Part("artifact.content.ipynb")], "ipynb");
        _files.ReadAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<FabricItemDefinitionFile>>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await ReadAsync(CreateService(), directory: "selected-directory",
            files: """[{"partPath":"artifact.content.ipynb","sourcePath":"notebooks/sales.ipynb"}]""",
            format: "ipynb", itemType: "Notebook");

        Assert.Same(expected, result);
        await _files.Received(1).ReadAsync("selected-directory",
            Arg.Is<IReadOnlyList<FabricItemDefinitionFile>>(files =>
                files.Count == 1 && files[0].PartPath == "artifact.content.ipynb" && files[0].SourcePath == "notebooks/sales.ipynb"),
            "ipynb", CancellationToken);
    }

    [Theory]
    [InlineData("{}", "directory", null, null)]
    [InlineData("{}", null, "[]", null)]
    [InlineData("{}", null, null, "ipynb")]
    [InlineData(null, "directory", null, null)]
    [InlineData(null, null, "[]", null)]
    [InlineData(null, null, null, "ipynb")]
    [InlineData(null, "", "[]", null)]
    [InlineData(null, "directory", " ", null)]
    public async Task ConflictingOrIncompleteSources_FailBeforeFileAccess(string? definition, string? directory, string? files, string? format)
    {
        await Assert.ThrowsAsync<FabricItemDefinitionInputException>(() =>
            ReadAsync(CreateService(), definition, directory, files, format));

        Assert.Empty(_files.ReceivedCalls());
    }

    [Theory]
    [InlineData(TransportTypes.Http)]
    [InlineData("")]
    [InlineData("HTTP")]
    [InlineData("future-transport")]
    public async Task LocalFiles_RequireExplicitLocalCapabilityBeforeParsingAnyPaths(string transport)
    {
        var service = CreateService(transport);

        var error = await Assert.ThrowsAsync<FabricItemDefinitionInputException>(() =>
            ReadAsync(service, directory: @"\\private-host\share", files: "not-json"));

        Assert.False(service.SupportsLocalFiles);
        Assert.Contains("inline --definition", error.Message);
        Assert.DoesNotContain("private-host", error.Message);
        Assert.Empty(_files.ReceivedCalls());
    }

    [Fact]
    public async Task LocalCapability_IsRecheckedAtInvocation()
    {
        var runtime = new ServerRuntimeConfiguration { Transport = TransportTypes.StdIo };
        var service = new FabricItemDefinitionInputService(OptionsFactory.Create(runtime), _files);
        Assert.True(service.SupportsLocalFiles);
        runtime.Transport = TransportTypes.Http;

        await Assert.ThrowsAsync<FabricItemDefinitionInputException>(() =>
            ReadAsync(service, directory: "directory", files: """[{"partPath":"data"}]"""));

        Assert.Empty(_files.ReceivedCalls());
    }

    [Theory]
    [InlineData("")]
    [InlineData("private-invalid-json")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"definition":{"parts":[]}}""")]
    [InlineData("""{"parts":null}""")]
    [InlineData("""{"parts":[]}""")]
    [InlineData("""{"parts":[null]}""")]
    [InlineData("""{"parts":[{}]}""")]
    [InlineData("""{"parts":[{"path":"data","payload":"e30="}]}""")]
    [InlineData("""{"parts":[{"path":null,"payload":"e30=","payloadType":"InlineBase64"}]}""")]
    [InlineData("""{"parts":[{"path":"data","payload":null,"payloadType":"InlineBase64"}]}""")]
    [InlineData("""{"parts":[{"path":"data","payload":"private-not-base64","payloadType":"InlineBase64"}]}""")]
    [InlineData("""{"parts":[{"path":"data","payload":"e30=","payloadType":"Uri"}]}""")]
    [InlineData("""{"parts":[{"path":"data","path":"other","payload":"e30=","payloadType":"InlineBase64"}]}""")]
    [InlineData("""{"parts":[{"path":"data","payload":"e30=","payloadType":"InlineBase64","private-unknown":true}]}""")]
    [InlineData("""{"parts":[{"path":"data","payload":"e30=","payloadType":"InlineBase64"}],"private-unknown":true}""")]
    [InlineData("""{"parts":[{"path":"data","payload":"e30=","payloadType":"InlineBase64"}],"format":"a","format":"b"}""")]
    [InlineData("""{"parts":[{"path":"data","payload":"e30=","payloadType":"InlineBase64"}],"format":" "}""")]
    public async Task InvalidInlineEnvelope_RejectsRatherThanSilentlyDroppingInput(string json)
    {
        var error = await Assert.ThrowsAsync<FabricItemDefinitionInputException>(() => ReadAsync(CreateService(), json));

        Assert.DoesNotContain("private-", error.Message);
        Assert.Empty(_files.ReceivedCalls());
    }

    [Theory]
    [InlineData("../outside", "other")]
    [InlineData("/absolute", "other")]
    [InlineData("data", "DATA")]
    [InlineData("data", "data/child")]
    [InlineData("data/child", "DATA")]
    [InlineData("data:stream", "other")]
    [InlineData("CON", "other")]
    public async Task Inline_UsesTheSamePortablePathRulesAsSavedFiles(string first, string second)
    {
        await Assert.ThrowsAsync<FabricItemDefinitionInputException>(() =>
            ReadAsync(CreateService(), Json(new([Part(first), Part(second)]))));

        Assert.Empty(_files.ReceivedCalls());
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("[null]")]
    [InlineData("[{}]")]
    [InlineData("""[{"partPath":"data","sourcePath":"a","sourcePath":"b"}]""")]
    [InlineData("""[{"partPath":"data","private-unknown":"value"}]""")]
    [InlineData("""[{"partPath":"data"},{"partPath":"DATA"}]""")]
    public async Task InvalidFileSelections_FailBeforeFileAccess(string files)
    {
        await Assert.ThrowsAsync<FabricItemDefinitionInputException>(() =>
            ReadAsync(CreateService(), directory: "directory", files: files));

        Assert.Empty(_files.ReceivedCalls());
    }

    [Theory]
    [InlineData("Notebook", "ipynb", "artifact.content.ipynb|.platform", true)]
    [InlineData("Notebook", "ipynb", "sales.ipynb", true)]
    [InlineData("Notebook", null, "notebook-content.py", true)]
    [InlineData("Notebook", "fabricGitSource", "notebook-content.sql", true)]
    [InlineData("Notebook", "FabricGitSource", "notebook-content.scala", true)]
    [InlineData("Notebook", "FabricGitSource", "notebook-content.r", true)]
    [InlineData("Notebook", null, "artifact.content.ipynb", false)]
    [InlineData("Notebook", "ipynb", "notebook-content.py", false)]
    [InlineData("Notebook", "ipynb", "one.ipynb|two.ipynb", false)]
    [InlineData("Notebook", "ipynb", "artifact.content.ipynb|notebook-content.py", false)]
    [InlineData("Notebook", "other", "notebook-content.py", false)]
    [InlineData("Notebook", null, ".platform", false)]
    [InlineData("DataPipeline", null, "pipeline-content.json|.platform", true)]
    [InlineData("DataPipeline", null, ".platform", false)]
    [InlineData("SemanticModel", null, "definition.pbism|definition/model.tmdl", true)]
    [InlineData("SemanticModel", "TMSL", "definition.pbism|model.bim", true)]
    [InlineData("SemanticModel", "TMSL", "model.bim", false)]
    [InlineData("SemanticModel", null, "definition.pbism|model.bim", false)]
    [InlineData("SemanticModel", "TMDL", "definition.pbism|model.bim|definition/model.tmdl", false)]
    [InlineData("SemanticModel", "other", "definition.pbism|definition/model.tmdl", false)]
    [InlineData("Report", null, "definition.pbir|definition/report.json", true)]
    [InlineData("Report", "PBIR-Legacy", "definition.pbir|report.json", true)]
    [InlineData("Report", "PBIR-Legacy", "report.json", false)]
    [InlineData("Report", null, "definition.pbir|report.json", false)]
    [InlineData("Report", "PBIR", "definition.pbir|report.json|definition/report.json", false)]
    [InlineData("Report", "other", "definition.pbir|definition/report.json", false)]
    [InlineData("FutureItem", "FutureFormat", "content", true)]
    public async Task CuratedStructuralChecks_MatchDocumentedMinimumsWithoutClosingOtherItemTypes(
        string itemType, string? format, string paths, bool valid)
    {
        var definition = new FabricItemDefinition(paths.Split('|').Select(static path => Part(path)).ToArray(), format);

        if (valid)
        {
            var result = await ReadAsync(CreateService(), Json(definition), itemType: itemType);
            Assert.NotNull(result);
            Assert.Equal(definition.Parts, result.Parts);
        }
        else
        {
            await Assert.ThrowsAsync<FabricItemDefinitionInputException>(() =>
                ReadAsync(CreateService(), Json(definition), itemType: itemType));
        }
        Assert.Empty(_files.ReceivedCalls());
    }

    [Fact]
    public async Task LocalStructuralChecks_RunBeforeReadingSelectedFiles()
    {
        var error = await Assert.ThrowsAsync<FabricItemDefinitionInputException>(() =>
            ReadAsync(CreateService(), directory: "directory", files: """[{"partPath":".platform"}]""", itemType: "DataPipeline"));

        Assert.Contains("pipeline-content.json", error.Message);
        Assert.Empty(_files.ReceivedCalls());
    }

    [Fact]
    public async Task Inline_EnforcesExactPartCountAndDecodedAndEncodedBudgets()
    {
        var service = CreateService(limits: new(3, 3, 5));
        var atLimit = new FabricItemDefinition([Part("a", [1, 2, 3]), Part("b", [4, 5]), Part("empty", [])]);
        Assert.NotNull(await ReadAsync(service, Json(atLimit)));
        FabricItemDefinition[] invalid =
        [
            new([Part("a", [1, 2, 3, 4])]),
            new([Part("a", [1, 2, 3]), Part("b", [4, 5, 6])]),
            new([Part("a", []), Part("b", []), Part("c", []), Part("d", [])]),
            new([new("a", Convert.ToBase64String([1, 2, 3]) + " ", "InlineBase64")])
        ];
        foreach (var definition in invalid)
        {
            await Assert.ThrowsAsync<FabricItemDefinitionInputException>(() => ReadAsync(service, Json(definition)));
        }
        await Assert.ThrowsAsync<FabricItemDefinitionInputException>(() =>
            ReadAsync(CreateService(limits: new(1, 1, 1)), new string(' ', 20_000)));
        Assert.Empty(_files.ReceivedCalls());
    }

    [Theory]
    [InlineData("io")]
    [InlineData("argument")]
    [InlineData("access")]
    [InlineData("budget")]
    public async Task LocalFailures_AreExplicitAndDoNotDisclosePathsOrContent(string failure)
    {
        Exception exception = failure switch
        {
            "io" => new IOException("private-host-path"),
            "argument" => new ArgumentException("private-host-path"),
            "access" => new UnauthorizedAccessException("private-host-path"),
            _ => new InvalidDataException("private-content")
        };
        _files.ReadAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<FabricItemDefinitionFile>>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var error = await Assert.ThrowsAsync<FabricItemDefinitionInputException>(() =>
            ReadAsync(CreateService(), directory: "private-host-path", files: """[{"partPath":"data"}]"""));

        Assert.Contains("Cannot package", error.Message);
        Assert.DoesNotContain("private-", error.Message);
    }

    [Fact]
    public async Task Cancellation_StopsBeforeFileAccessAndPropagatesDuringReading()
    {
        var service = CreateService();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ReadAsync("FutureItem", null, "directory", "[]", null, new CancellationToken(canceled: true)));
        Assert.Empty(_files.ReceivedCalls());
        _files.ReadAsync(Arg.Any<string>(), Arg.Any<IReadOnlyList<FabricItemDefinitionFile>>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException(CancellationToken));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ReadAsync(service, directory: "directory", files: """[{"partPath":"data"}]"""));
    }

    private FabricItemDefinitionInputService CreateService(
        string transport = TransportTypes.StdIo, FabricItemDefinitionFileLimits? limits = null) =>
        new(OptionsFactory.Create(new ServerRuntimeConfiguration { Transport = transport }), _files, limits);

    private Task<FabricItemDefinition?> ReadAsync(
        FabricItemDefinitionInputService service, string? definition = null, string? directory = null,
        string? files = null, string? format = null, string itemType = "FutureItem") =>
        service.ReadAsync(itemType, definition, directory, files, format, CancellationToken);

    private static FabricItemDefinitionPart Part(string path, byte[]? bytes = null) =>
        new(path, Convert.ToBase64String(bytes ?? [1]), "InlineBase64");

    private static string Json(FabricItemDefinition definition) =>
        JsonSerializer.Serialize(definition, CoreJsonContext.Default.FabricItemDefinition);
}
