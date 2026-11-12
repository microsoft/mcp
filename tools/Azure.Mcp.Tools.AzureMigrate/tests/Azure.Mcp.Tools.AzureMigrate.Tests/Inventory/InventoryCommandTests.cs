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
}
