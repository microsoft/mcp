// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Xunit;

namespace Azure.Mcp.Tools.AzureMigrate.Tests.Inventory;

public class InventoryCommandTests
{
    private const string Sas = "https://acct.blob.core.windows.net/c/inv/input.zip?sv=2022-11-02&sp=w&sr=b&sig=abc%3D";

    [Fact]
    public void BuildUploadCommand_QuotesSasAndDisablesLengthCheck()
    {
        string? cmd = CreateOrUpdateInventoryCommand.BuildUploadCommand(Sas);

        Assert.Equal(
            $"azcopy copy \"<path to your file>\" \"{Sas}\" --from-to=LocalBlob --blob-type=BlockBlob --overwrite=true --check-length=false",
            cmd);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not a url")]
    [InlineData("ftp://acct/x")]
    [InlineData("https://acct/x?sig=\"bad")]
    public void BuildUploadCommand_RejectsInvalidUrl(string? url) =>
        Assert.Null(CreateOrUpdateInventoryCommand.BuildUploadCommand(url));

    [Theory]
    [InlineData("inv-0123456789abcdef", true)]
    [InlineData("a_b-C", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("has space", false)]
    [InlineData("abcdefghijklmnopqrstu", false)]
    public void IsValidSessionId(string? id, bool expected) =>
        Assert.Equal(expected, DsBackdoorClient.IsValidSessionId(id));

    [Fact]
    public void OperationStatusUrl_IsBuiltFromSessionAndOperation()
    {
        Guid op = Guid.Parse("11111111-2222-3333-4444-555555555555");

        Assert.Equal(
            "https://ds/mcprestapi/sessions/s1/operationstatus/11111111-2222-3333-4444-555555555555?api-version=2026-11-11-preview",
            DsBackdoorClient.OperationStatusUrl("https://ds", "s1", op));
    }

    [Fact]
    public void ReadActiveSessionId_ParsesConflictError()
    {
        const string body = "{\"error\":{\"code\":\"ActiveSessionAlreadyExists\",\"message\":\"An active Session 'inv-abc' already exists.\"}}";

        Assert.Equal("inv-abc", DsBackdoorClient.ReadActiveSessionId(body));
        Assert.Null(DsBackdoorClient.ReadActiveSessionId("{\"error\":{\"code\":\"Other\"}}"));
    }

    [Fact]
    public void ParseStatus_Running_IsNotCompleted()
    {
        var r = GetInventoryStatusCommand.Parse("s1", "op", new(200, "{\"id\":\"op\",\"status\":\"Running\"}", null, 30));

        Assert.True(r.Success);
        Assert.Equal("Running", r.Status);
        Assert.False(r.IsCompleted);
        Assert.Equal(30, r.RetryAfterSeconds);
    }

    [Fact]
    public void ParseStatus_Succeeded_ReturnsResult()
    {
        var r = GetInventoryStatusCommand.Parse("s1", "op",
            new(200, "{\"status\":\"Succeeded\",\"properties\":{\"result\":{\"servers\":3}}}", null, null));

        Assert.True(r.IsCompleted);
        Assert.Equal("{\"servers\":3}", r.Result);
        Assert.Null(r.RetryAfterSeconds);
    }

    [Fact]
    public void ParseStatus_Failed_ReturnsError()
    {
        var r = GetInventoryStatusCommand.Parse("s1", "op",
            new(200, "{\"status\":\"Failed\",\"error\":{\"code\":\"NoFileUploadedForImport\",\"message\":\"No file.\"}}", null, null));

        Assert.True(r.Success);
        Assert.True(r.IsCompleted);
        Assert.Equal("NoFileUploadedForImport", r.ErrorCode);
        Assert.Equal("No file.", r.ErrorMessage);
        Assert.Contains("not uploaded within the upload window", r.Instructions);
    }

    [Fact]
    public void ParseStatus_NotFound_Fails()
    {
        var r = GetInventoryStatusCommand.Parse("s1", "op", new(404, "{}", "x", null));

        Assert.False(r.Success);
        Assert.Equal(404, r.HttpStatus);
    }

    [Fact]
    public void ParseStatus_WithHaSubject_UsesItInMessages()
    {
        var running = GetInventoryStatusCommand.Parse("ha-1", "op", new(200, "{\"status\":\"Running\"}", null, null), "HA assessment");
        var notFound = GetInventoryStatusCommand.Parse("ha-1", "op", new(404, "{}", "x", null), "HA assessment");

        Assert.Contains("the HA assessment is still in progress", running.Instructions);
        Assert.Contains("No HA assessment operation was found", notFound.Error);
    }

    private const string ReadSas = "https://acct.blob.core.windows.net/c/inv/output.csv?sv=2022-11-02&sp=r&sr=b&sig=abc%3D";

    [Fact]
    public void ExportUrl_IsBuiltFromSession() =>
        Assert.Equal(
            "https://ds/mcprestapi/sessions/s1/export?api-version=2026-11-11-preview&artifacts=internal",
            DsBackdoorClient.ExportUrl("https://ds", "s1", "internal"));

    [Fact]
    public void BuildDownloadCommand_QuotesSas()
    {
        Assert.Equal(
            $"azcopy copy \"{ReadSas}\" \"<path to save the file>\" --from-to=BlobLocal",
            ExportInventoryCommand.BuildDownloadCommand(ReadSas));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not a url")]
    [InlineData("ftp://acct/x")]
    [InlineData("https://acct/x?sig=\"bad")]
    public void BuildDownloadCommand_RejectsInvalidUrl(string? url) =>
        Assert.Null(ExportInventoryCommand.BuildDownloadCommand(url));

    [Theory]
    [InlineData("uri")]
    [InlineData("Uri")]
    public void ParseExport_Success_ReturnsLinkAndCommand(string propertyName)
    {
        var r = ExportInventoryCommand.Parse("s1", "refined", new(200, $"{{\"{propertyName}\":\"{ReadSas}\"}}", null, null));

        Assert.True(r.Success);
        Assert.Equal(ReadSas, r.DownloadUrl);
        Assert.Equal(ExportInventoryCommand.BuildDownloadCommand(ReadSas), r.DownloadCommand);
        Assert.Contains("clickable link", r.Instructions);
    }

    [Fact]
    public void ParseExport_MissingUri_Fails()
    {
        var r = ExportInventoryCommand.Parse("s1", "refined", new(200, "{}", null, null));

        Assert.False(r.Success);
        Assert.Null(r.DownloadUrl);
    }

    [Fact]
    public void ParseExport_NotReady_TellsUserToCheckStatus()
    {
        var r = ExportInventoryCommand.Parse("s1", "refined", new(400, "{\"error\":{\"code\":\"InvalidParameter\"}}", "bad", null));

        Assert.False(r.Success);
        Assert.Equal(400, r.HttpStatus);
        Assert.Contains("refined inventory file is not available yet", r.Instructions);
    }

    [Fact]
    public void ParseExport_NotFound_Fails()
    {
        var r = ExportInventoryCommand.Parse("s1", "refined", new(404, "{}", "x", null));

        Assert.False(r.Success);
        Assert.Equal(404, r.HttpStatus);
        Assert.Contains("No inventory was found", r.Error);
    }

    [Theory]
    [InlineData(null, "refined", "external")]
    [InlineData("", "refined", "external")]
    [InlineData("Refined", "refined", "external")]
    [InlineData(" OFFLINE ", "offline", "internal")]
    public void NormalizeOutput_MapsToArtifacts(string? input, string expectedOutput, string expectedArtifacts)
    {
        string? output = ExportInventoryCommand.NormalizeOutput(input);

        Assert.Equal(expectedOutput, output);
        Assert.Equal(expectedArtifacts, ExportInventoryCommand.ToArtifacts(output!));
    }

    [Theory]
    [InlineData("internal")]
    [InlineData("external")]
    [InlineData("zip")]
    public void NormalizeOutput_RejectsUnknownValue(string input) =>
        Assert.Null(ExportInventoryCommand.NormalizeOutput(input));

    [Fact]
    public void ParseExport_Offline_NamesOfflinePackage()
    {
        var r = ExportInventoryCommand.Parse("s1", "offline", new(200, $"{{\"uri\":\"{ReadSas}\"}}", null, null));

        Assert.True(r.Success);
        Assert.Equal("offline", r.Output);
        Assert.Contains("offline zip package is ready", r.Instructions);
    }
}