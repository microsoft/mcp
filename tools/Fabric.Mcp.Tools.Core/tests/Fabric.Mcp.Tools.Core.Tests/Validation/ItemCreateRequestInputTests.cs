// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Validation;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Validation;

public sealed class ItemCreateRequestInputTests()
{
    private const string LabelId = "b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f";
    private const string FolderId = "bbbbbbbb-1111-2222-3333-cccccccccccc";

    [Fact]
    public static void NoAdditionalInputs_PreservesOriginalRequestOmissions()
    {
        var request = CreateRequest(Options());

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(request, CoreJsonContext.Default.CreateItemRequest));
        Assert.Equal(["displayName", "type"], document.RootElement.EnumerateObject().Select(static property => property.Name).Order());
        Assert.Null(request.FolderId);
        Assert.Null(request.CreationPayload);
        Assert.Null(request.Options);
        Assert.Null(request.SensitivityLabelSettings);
    }

    [Theory]
    [InlineData(FolderId)]
    [InlineData("{BBBBBBBB-1111-2222-3333-CCCCCCCCCCCC}")]
    [InlineData("bbbbbbbb111122223333cccccccccccc")]
    public static void Folder_NormalizesSuppliedUuidWithoutInventingOtherProperties(string value)
    {
        var options = Options();
        options.FolderId = value;

        var request = CreateRequest(options);

        Assert.Equal(Guid.Parse(FolderId), request.FolderId);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(request, CoreJsonContext.Default.CreateItemRequest));
        Assert.Equal(FolderId, document.RootElement.GetProperty("folderId").GetString());
        Assert.Equal(3, document.RootElement.EnumerateObject().Count());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("private-folder-name")]
    [InlineData("../private-folder")]
    public static void Folder_RejectsEmptyOrInvalidUuid(string value)
    {
        var options = Options();
        options.FolderId = value;

        var error = Assert.Throws<FabricItemInputException>(() => CreateRequest(options));

        Assert.Contains("--folder-id", error.Message);
        Assert.DoesNotContain("private-", error.Message);
    }

    [Theory]
    [InlineData("definition")]
    [InlineData("directory")]
    [InlineData("files")]
    [InlineData("format")]
    public static void CreationPayload_ConflictsWithEveryDefinitionOptionIncludingIncompleteInputs(string input)
    {
        var options = Options();
        options.CreationPayload = "{}";
        options.DefinitionInput = new();
        switch (input)
        {
            case "definition":
                options.DefinitionInput.Definition = "";
                break;
            case "directory":
                options.DefinitionInput.DefinitionDirectory = "";
                break;
            case "files":
                options.DefinitionInput.DefinitionFiles = "[]";
                break;
            case "format":
                options.DefinitionInput.DefinitionFormat = "format";
                break;
        }

        var error = Assert.Throws<FabricItemInputException>(() => CreateRequest(options));

        Assert.Contains("--creation-payload cannot be combined", error.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static void WorkloadOptions_RequireDefinitionRatherThanCreationPayload(bool withPayload)
    {
        var options = Options();
        options.Options = "{}";
        options.CreationPayload = withPayload ? "{}" : null;

        var error = Assert.Throws<FabricItemInputException>(() => CreateRequest(options));

        Assert.Contains("--options requires", error.Message);
    }

    [Fact]
    public static void EmptySourceGroup_DoesNotBecomeADefinitionOrConflictWithCreationPayload()
    {
        var options = Options();
        options.DefinitionInput = new();
        options.CreationPayload = "{}";

        var request = CreateRequest(options);

        Assert.Null(request.Definition);
        Assert.NotNull(request.CreationPayload);
        options.Options = "{}";
        var error = Assert.Throws<FabricItemInputException>(() => CreateRequest(options));
        Assert.Contains("--options requires", error.Message);
    }

    [Theory]
    [InlineData("creation-payload", "{}")]
    [InlineData("options", "{}")]
    [InlineData("creation-payload", """{"futureProperty":{"enabled":false,"empty":"","absent":null,"values":[1,2.50,9007199254740993,true,null]},"sync":true}""")]
    [InlineData("options", """{"futureProperty":{"enabled":false,"empty":"","absent":null,"values":[1,2.50,9007199254740993,true,null]},"sync":true}""")]
    public static void WorkloadObjects_PreserveOpaqueValuesWithSourceGeneratedRequestSerialization(string name, string json)
    {
        var options = WithJson(name, json);

        var request = CreateRequest(options);

        using var expected = JsonDocument.Parse(json);
        using var body = JsonDocument.Parse(JsonSerializer.Serialize(request, CoreJsonContext.Default.CreateItemRequest));
        var propertyName = name == "options" ? "options" : "creationPayload";
        Assert.Equal(JsonValueKind.Object, body.RootElement.GetProperty(propertyName).ValueKind);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, body.RootElement.GetProperty(propertyName)));
        var roundTrip = JsonSerializer.Deserialize(body.RootElement, CoreJsonContext.Default.CreateItemRequest)!;
        Assert.True(JsonElement.DeepEquals(expected.RootElement,
            (name == "options" ? roundTrip.Options : roundTrip.CreationPayload)!.Value));
        Assert.False(options.Sync);
        Assert.Equal(120, options.MaxWaitSeconds);
        Assert.True(options.EarlyPoll);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("false")]
    [InlineData("12")]
    [InlineData("\"private-path.json\"")]
    [InlineData("{")]
    [InlineData("""{"setting":1,"setting":2}""")]
    [InlineData("""{"setting":{"private-duplicate":1,"private-duplicate":2}}""")]
    [InlineData("""{"setting":[{"private-duplicate":1,"private-duplicate":2}]}""")]
    [InlineData("""{"setting":1,}""")]
    public static void AllJsonInputs_RejectInvalidShapesAndDuplicatesWithoutLeakingData(string json)
    {
        foreach (var name in new[] { "creation-payload", "options", "sensitivity-label-settings" })
        {
            var error = Assert.Throws<FabricItemInputException>(() => CreateRequest(WithJson(name, json)));

            Assert.Contains("--" + name, error.Message);
            Assert.DoesNotContain("private-", error.Message);
        }
    }

    [Theory]
    [InlineData("creation-payload", 'x')]
    [InlineData("creation-payload", '\u00e9')]
    [InlineData("options", 'x')]
    [InlineData("options", '\u00e9')]
    public static void WorkloadObjects_EnforceExactUtf8ByteBudget(string name, char character)
    {
        const string prefix = "{\"data\":\"";
        const string suffix = "\"}";
        var bytesPerCharacter = Encoding.UTF8.GetByteCount(character.ToString());
        var contentBytes = FabricItemJsonInput.MaxObjectBytes - prefix.Length - suffix.Length;
        var json = prefix + new string(character, contentBytes / bytesPerCharacter) +
            new string('x', contentBytes % bytesPerCharacter) + suffix;
        Assert.Equal(FabricItemJsonInput.MaxObjectBytes, Encoding.UTF8.GetByteCount(json));

        Assert.NotNull(CreateRequest(WithJson(name, json)));
        var error = Assert.Throws<FabricItemInputException>(() => CreateRequest(WithJson(name, json + " ")));

        Assert.Contains("1048576 UTF-8 bytes", error.Message);
    }

    [Theory]
    [InlineData("creation-payload")]
    [InlineData("options")]
    public static void WorkloadObjects_EnforceExactDepthWhileLeavingRoomForTheRequestEnvelope(string name)
    {
        static string Nested(int depth) => "{\"data\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";

        var request = CreateRequest(WithJson(name, Nested(FabricItemJsonInput.MaxObjectDepth)));
        Assert.NotEmpty(JsonSerializer.Serialize(request, CoreJsonContext.Default.CreateItemRequest));
        var error = Assert.Throws<FabricItemInputException>(() =>
            CreateRequest(WithJson(name, Nested(FabricItemJsonInput.MaxObjectDepth + 1))));

        Assert.Contains("32 nesting levels", error.Message);
    }

    [Theory]
    [InlineData("""{"labelId":"b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f"}""")]
    [InlineData("""{"labelId":"b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f","sensitivityLabelApplyStrategy":"ApplyOrFail"}""")]
    [InlineData("""{"labelId":"b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f","sensitivityLabelApplyStrategy":"Ignore"}""")]
    public static void LabelSettings_AreTypedAndPreserveOmittedStrategyWithoutSelectingADefault(string json)
    {
        var request = CreateRequest(WithJson("sensitivity-label-settings", json));

        var settings = Assert.IsType<FabricSensitivityLabelSettings>(request.SensitivityLabelSettings);
        using var expected = JsonDocument.Parse(json);
        Assert.Equal(Guid.Parse(LabelId), settings.LabelId);
        Assert.Equal(expected.RootElement.TryGetProperty("sensitivityLabelApplyStrategy", out var strategy) ? strategy.GetString() : null,
            settings.SensitivityLabelApplyStrategy);
        using var body = JsonDocument.Parse(JsonSerializer.Serialize(request, CoreJsonContext.Default.CreateItemRequest));
        Assert.True(JsonElement.DeepEquals(expected.RootElement, body.RootElement.GetProperty("sensitivityLabelSettings")));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"sensitivityLabelApplyStrategy":"ApplyOrFail"}""")]
    [InlineData("""{"sensitivityLabelApplyStrategy":"Ignore"}""")]
    [InlineData("""{"labelId":null}""")]
    [InlineData("""{"labelId":12}""")]
    [InlineData("""{"labelId":""}""")]
    [InlineData("""{"labelId":"private-invalid-id"}""")]
    [InlineData("""{"labelId":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("""{"labelId":"b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f","sensitivityLabelApplyStrategy":null}""")]
    [InlineData("""{"labelId":"b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f","sensitivityLabelApplyStrategy":0}""")]
    [InlineData("""{"labelId":"b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f","sensitivityLabelApplyStrategy":false}""")]
    [InlineData("""{"labelId":"b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f","sensitivityLabelApplyStrategy":""}""")]
    [InlineData("""{"labelId":"b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f","sensitivityLabelApplyStrategy":"ignore"}""")]
    [InlineData("""{"labelId":"b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f","sensitivityLabelApplyStrategy":"private-unknown-strategy"}""")]
    [InlineData("""{"labelId":"b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f","private-unknown-property":true}""")]
    [InlineData("""{"LabelId":"b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f"}""")]
    public static void InvalidLabelSettings_RejectRatherThanSilentlyDroppingOrWeakeningPolicy(string json)
    {
        var error = Assert.Throws<FabricItemInputException>(() => CreateRequest(WithJson("sensitivity-label-settings", json)));

        Assert.DoesNotContain("private-", error.Message);
    }

    [Fact]
    public static void LabelSettings_EnforceExactSmallObjectBudget()
    {
        var settings = $$"""{"labelId":"{{LabelId}}"}""";
        var json = settings + new string(' ', ItemCreateRequestInput.MaxLabelSettingsBytes - settings.Length);

        Assert.NotNull(CreateRequest(WithJson("sensitivity-label-settings", json)).SensitivityLabelSettings);
        var error = Assert.Throws<FabricItemInputException>(() => CreateRequest(WithJson("sensitivity-label-settings", json + " ")));

        Assert.Contains("4096 UTF-8 bytes", error.Message);
    }

    [Fact]
    public static void Cancellation_StopsInputPreparation()
    {
        var cancellationToken = new CancellationToken(canceled: true);

        var error = Assert.Throws<OperationCanceledException>(() =>
            ItemCreateRequestInput.CreateRequest(WithJson("creation-payload", "private-invalid-json"), cancellationToken));

        Assert.Equal(cancellationToken, error.CancellationToken);
    }

    private static ItemCreateOptions Options() => new() { DisplayName = "Created", ItemType = "FutureType" };

    private static CreateItemRequest CreateRequest(ItemCreateOptions options) =>
        ItemCreateRequestInput.CreateRequest(options, TestContext.Current.CancellationToken);

    private static ItemCreateOptions WithJson(string name, string json)
    {
        var options = Options();
        switch (name)
        {
            case "creation-payload":
                options.CreationPayload = json;
                break;
            case "options":
                options.DefinitionInput = new() { Definition = "{}" };
                options.Options = json;
                break;
            case "sensitivity-label-settings":
                options.SensitivityLabelSettings = json;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(name));
        }
        return options;
    }
}
