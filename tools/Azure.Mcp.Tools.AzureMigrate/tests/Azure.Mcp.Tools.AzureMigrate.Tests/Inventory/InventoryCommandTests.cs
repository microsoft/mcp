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
    [InlineData(null, false, "{}")]
    [InlineData("only prod", false, "{\"message\":\"only prod\"}")]
    [InlineData("set web-01 OS", true, "{\"inputType\":\"Text\",\"message\":\"set web-01 OS\"}")]
    public void BuildInvokeBody_SelectsBlobOrTextInput(string? message, bool textOnly, string expected) =>
        Assert.Equal(expected, CreateOrUpdateInventoryCommand.BuildInvokeBody(message, textOnly));

    [Theory]
    [InlineData(false, null, null, false)]
    [InlineData(true, "inv-1", "memory is in GB", false)]
    [InlineData(true, null, "memory is in GB", true)]
    [InlineData(true, "inv-1", null, true)]
    public void ValidateTextOnly_RequiresSessionAndMessage(bool textOnly, string? sessionId, string? message, bool expectError) =>
        Assert.Equal(expectError, CreateOrUpdateInventoryCommand.ValidateTextOnly(textOnly, sessionId, message) is not null);

    [Fact]
    public void TextOnlyResult_HasNoUploadCommand()
    {
        var result = CreateOrUpdateInventoryCommand.TextOnlyResult("inv-1", "op-1", "req-1", 202);

        Assert.True(result.Success);
        Assert.Equal("inv-1", result.SessionId);
        Assert.Equal("op-1", result.OperationId);
        Assert.Null(result.UploadCommand);
        Assert.Null(result.UploadWindowMinutes);
        Assert.Contains("nothing needs to be uploaded", result.Instructions);
    }

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

    [Theory]
    [InlineData(false, "the inventory has started")]
    [InlineData(true, "updates their existing inventory, not a new one")]
    public void FileInstructions_StartTrackingWithoutWaitingForUser(bool reused, string opening)
    {
        string instructions = CreateOrUpdateInventoryCommand.FileInstructions(reused);

        Assert.Contains(opening, instructions);
        Assert.Contains("do not wait for the user to say the upload has finished", instructions);
    }

    [Theory]
    [InlineData("validation", null, false, "ask them for the correct value")]
    [InlineData("auth", null, false, "sign in again")]
    [InlineData("invokeSession", 404, true, "could not be found")]
    [InlineData("invokeSession", 400, true, "upload their inventory file first")]
    [InlineData("invokeSession", 503, true, "temporary service problem")]
    [InlineData("createSession", null, false, "temporary service problem")]
    [InlineData("invokeSession", 404, false, "quote 'error'")]
    [InlineData("createSession", 403, false, "quote 'error'")]
    public void GetFailureInstructions_SelectsByStepAndStatus(string step, int? status, bool textOnly, string expected) =>
        Assert.Contains(expected, CreateOrUpdateInventoryCommand.GetFailureInstructions(step, status, textOnly));

    [Fact]
    public void Fail_UsesExplicitInstructionsWhenGiven()
    {
        var r = CreateOrUpdateInventoryCommand.Fail("createSession", "inv-1", "req", 409, "conflict",
            instructions: CreateOrUpdateInventoryCommand.ActiveSessionInstructions("inv-old"));

        Assert.False(r.Success);
        Assert.Contains("'inv-old'", r.Instructions);
    }

    [Fact]
    public void ParseStatus_NotFound_InventoryOffersNewInventory_HaUnchanged()
    {
        var inventory = GetInventoryStatusCommand.Parse("s1", "op", new(404, "{}", "x", null));
        var ha = GetInventoryStatusCommand.Parse("ha-1", "op", new(404, "{}", "x", null), "HA assessment");

        Assert.Contains("offer to start a new inventory", inventory.Instructions);
        Assert.Null(ha.Instructions);
    }

    [Fact]
    public void GetInstructions_Succeeded_InventoryOffersNextSteps_HaUnchanged()
    {
        Assert.Contains("offer to export the inventory or create an assessment",
            GetInventoryStatusCommand.GetInstructions("Succeeded", true, null));
        Assert.DoesNotContain("offer to export", GetInventoryStatusCommand.GetInstructions("Succeeded", true, null, "HA assessment"));
    }

    [Fact]
    public void ParseStatus_InventoryRunning_TellsAgentToKeepTracking()
    {
        var r = GetInventoryStatusCommand.Parse("s1", "op", new(200, "{\"status\":\"Running\"}", null, null));

        Assert.Contains("keep checking until isCompleted is true", r.Instructions);
        Assert.Contains("do not repeat it on later checks", r.Instructions);
        Assert.DoesNotContain("wait for them to confirm the upload", r.Instructions);
    }

    private static readonly DsBackdoorClient.DsResponse s_running = new(200, "{\"status\":\"Running\"}", null, null);
    private static readonly DsBackdoorClient.DsResponse s_succeeded = new(200, "{\"status\":\"Succeeded\",\"properties\":{\"result\":\"ok\"}}", null, null);

    private static async Task<(GetInventoryStatusCommand.GetInventoryStatusCommandResult Result, int Calls, List<TimeSpan> Delays)> WaitAsync(
        params DsBackdoorClient.DsResponse[] responses)
    {
        int calls = 0;
        List<TimeSpan> delays = [];
        var result = await GetInventoryStatusCommand.WaitForCompletionAsync(
            "s1", "op",
            _ => Task.FromResult(responses[Math.Min(calls++, responses.Length - 1)]),
            (d, _) => { delays.Add(d); return Task.CompletedTask; },
            CancellationToken.None);
        return (result, calls, delays);
    }

    [Fact]
    public async Task WaitForCompletion_PollsUntilSucceeded()
    {
        var (result, calls, delays) = await WaitAsync(s_running, s_running, s_succeeded);

        Assert.True(result.IsCompleted);
        Assert.Equal("ok", result.Result);
        Assert.Equal(3, calls);
        Assert.Equal([TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4)], delays);
    }

    [Fact]
    public async Task WaitForCompletion_StillRunningAfterAllChecks_ReturnsRunning()
    {
        var (result, calls, delays) = await WaitAsync(s_running);

        Assert.True(result.Success);
        Assert.False(result.IsCompleted);
        Assert.Equal(GetInventoryStatusCommand.s_pollDelays.Length + 1, calls);
        Assert.Equal(GetInventoryStatusCommand.MaxWaitSeconds, (int)delays.Sum(d => d.TotalSeconds));
    }

    [Fact]
    public async Task WaitForCompletion_RetriesTransientError()
    {
        var (result, calls, _) = await WaitAsync(s_running, new(503, "{}", "busy", null), s_succeeded);

        Assert.True(result.IsCompleted);
        Assert.Equal(3, calls);
    }

    [Fact]
    public async Task WaitForCompletion_NotFound_ReturnsImmediately()
    {
        var (result, calls, delays) = await WaitAsync(new DsBackdoorClient.DsResponse(404, "{}", "x", null));

        Assert.False(result.Success);
        Assert.Equal(1, calls);
        Assert.Empty(delays);
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